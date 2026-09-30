using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>
	/// Lazy ONNX Runtime session for FBCNN color model (display-only; never writes archives).
	/// Inference is serialized: InferenceSession.Run is not concurrent-safe.
	/// CancellationToken is checked around work; ORT Run itself cannot be aborted mid-call.
	/// </summary>
	public sealed class FbcnnOnnxRunner : IDisposable
	{
		private readonly object gate = new object();
		private InferenceSession session;
		private string modelPath;
		private string lastError = string.Empty;
		private int maxLongEdge = 1024;

		public string LastError => lastError;

		public string ModelPath => modelPath;

		public int MaxLongEdge
		{
			get => maxLongEdge;
			set => maxLongEdge = Math.Max(256, value);
		}

		public bool IsLoaded
		{
			get
			{
				lock (gate)
				{
					return session != null;
				}
			}
		}

		public bool TryLoad(string onnxPath, out string error)
		{
			error = string.Empty;
			if (!FbcnnModelIntegrity.TryValidate(onnxPath, out error))
			{
				lastError = error;
				return false;
			}
			string fullPath = Path.GetFullPath(onnxPath);
			lock (gate)
			{
				DisposeSession_NoLock();
				try
				{
					var opts = new SessionOptions();
					session = new InferenceSession(fullPath, opts);
					modelPath = fullPath;
					lastError = string.Empty;
					return true;
				}
				catch (Exception ex)
				{
					error = ex.Message;
					lastError = error;
					session = null;
					modelPath = null;
					return false;
				}
			}
		}

		public void Unload()
		{
			lock (gate)
			{
				DisposeSession_NoLock();
				modelPath = null;
			}
		}

		/// <summary>
		/// Run blind FBCNN. Returns a new Bitmap (caller owns). Source is not disposed.
		/// Respects <see cref="MaxLongEdge"/> downscale then upscales to original size.
		/// </summary>
		public Bitmap Apply(Bitmap source, CancellationToken cancel)
		{
			if (source == null)
			{
				return null;
			}
			cancel.ThrowIfCancellationRequested();

			int ow = source.Width;
			int oh = source.Height;
			Bitmap work = null;
			bool disposeWork = false;
			float[] rented = null;
			try
			{
				work = EnsureBgr24(source, out disposeWork);
				int longEdge = Math.Max(work.Width, work.Height);
				if (longEdge > maxLongEdge)
				{
					float scale = maxLongEdge / (float)longEdge;
					int nw = Math.Max(8, (int)(work.Width * scale) / 8 * 8);
					int nh = Math.Max(8, (int)(work.Height * scale) / 8 * 8);
					Bitmap scaled = new Bitmap(nw, nh, PixelFormat.Format24bppRgb);
					using (Graphics g = Graphics.FromImage(scaled))
					{
						g.InterpolationMode = InterpolationMode.HighQualityBicubic;
						g.DrawImage(work, 0, 0, nw, nh);
					}
					if (disposeWork)
					{
						work.Dispose();
					}
					work = scaled;
					disposeWork = true;
				}

				cancel.ThrowIfCancellationRequested();
				int w = work.Width;
				int h = work.Height;
				int need = 3 * h * w;
				rented = ArrayPool<float>.Shared.Rent(need);
				BitmapToChw(work, rented);
				var input = new DenseTensor<float>(rented.AsMemory(0, need), new[] { 1, 3, h, w });
				var inputs = new[] { NamedOnnxValue.CreateFromTensor("image", input) };

				// Serialize all session use (load + Run): ORT sessions are not concurrent-safe.
				lock (gate)
				{
					if (session == null)
					{
						throw new InvalidOperationException(string.IsNullOrEmpty(lastError) ? "FBCNN model not loaded" : lastError);
					}
					cancel.ThrowIfCancellationRequested();
					using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs))
					{
						cancel.ThrowIfCancellationRequested();
						Tensor<float> restored = results[0].AsTensor<float>();
						Bitmap small = ChwToBitmap(restored);
						if (small.Width == ow && small.Height == oh)
						{
							return small;
						}
						Bitmap full = new Bitmap(ow, oh, PixelFormat.Format32bppArgb);
						using (Graphics g = Graphics.FromImage(full))
						{
							g.InterpolationMode = InterpolationMode.HighQualityBicubic;
							g.DrawImage(small, 0, 0, ow, oh);
						}
						small.Dispose();
						return full;
					}
				}
			}
			finally
			{
				if (rented != null)
				{
					ArrayPool<float>.Shared.Return(rented);
				}
				if (disposeWork && work != null)
				{
					work.Dispose();
				}
			}
		}

		/// <summary>Return a 24bpp BGR bitmap suitable for LockBits; may be source or a clone.</summary>
		private static Bitmap EnsureBgr24(Bitmap source, out bool disposeResult)
		{
			if (source.PixelFormat == PixelFormat.Format24bppRgb)
			{
				disposeResult = false;
				return source;
			}
			Bitmap copy = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
			using (Graphics g = Graphics.FromImage(copy))
			{
				g.DrawImage(source, 0, 0, source.Width, source.Height);
			}
			disposeResult = true;
			return copy;
		}

		private static void BitmapToChw(Bitmap bmp, float[] data)
		{
			if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
			{
				throw new InvalidOperationException("BitmapToChw requires Format24bppRgb");
			}
			int w = bmp.Width;
			int h = bmp.Height;
			var rect = new Rectangle(0, 0, w, h);
			BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
			try
			{
				int stride = bd.Stride;
				unsafe
				{
					byte* ptr = (byte*)bd.Scan0;
					for (int y = 0; y < h; y++)
					{
						byte* row = ptr + y * stride;
						for (int x = 0; x < w; x++)
						{
							byte b = row[x * 3 + 0];
							byte g = row[x * 3 + 1];
							byte r = row[x * 3 + 2];
							int i = y * w + x;
							data[0 * h * w + i] = r / 255f;
							data[1 * h * w + i] = g / 255f;
							data[2 * h * w + i] = b / 255f;
						}
					}
				}
			}
			finally
			{
				bmp.UnlockBits(bd);
			}
		}

		private static Bitmap ChwToBitmap(Tensor<float> t)
		{
			int h = t.Dimensions[2];
			int w = t.Dimensions[3];
			Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
			var rect = new Rectangle(0, 0, w, h);
			BitmapData bd = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			try
			{
				int stride = bd.Stride;
				unsafe
				{
					byte* ptr = (byte*)bd.Scan0;
					for (int y = 0; y < h; y++)
					{
						byte* row = ptr + y * stride;
						for (int x = 0; x < w; x++)
						{
							float rf = t[0, 0, y, x];
							float gf = t[0, 1, y, x];
							float bf = t[0, 2, y, x];
							row[x * 3 + 2] = (byte)Math.Max(0, Math.Min(255, (int)(rf * 255f + 0.5f)));
							row[x * 3 + 1] = (byte)Math.Max(0, Math.Min(255, (int)(gf * 255f + 0.5f)));
							row[x * 3 + 0] = (byte)Math.Max(0, Math.Min(255, (int)(bf * 255f + 0.5f)));
						}
					}
				}
			}
			finally
			{
				bmp.UnlockBits(bd);
			}
			return bmp;
		}

		private void DisposeSession_NoLock()
		{
			if (session != null)
			{
				session.Dispose();
				session = null;
			}
		}

		public void Dispose()
		{
			Unload();
		}
	}
}
