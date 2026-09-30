using System;
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
			if (string.IsNullOrWhiteSpace(onnxPath) || !File.Exists(onnxPath))
			{
				error = "ONNX model file not found: " + onnxPath;
				lastError = error;
				return false;
			}
			lock (gate)
			{
				DisposeSession_NoLock();
				try
				{
					var opts = new SessionOptions();
					// CPU first; DirectML/CUDA can be added later (T015b follow-up).
					session = new InferenceSession(onnxPath, opts);
					modelPath = Path.GetFullPath(onnxPath);
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
			InferenceSession s;
			lock (gate)
			{
				s = session;
			}
			if (s == null)
			{
				throw new InvalidOperationException(string.IsNullOrEmpty(lastError) ? "FBCNN model not loaded" : lastError);
			}
			cancel.ThrowIfCancellationRequested();

			int ow = source.Width;
			int oh = source.Height;
			Bitmap work = source;
			bool disposeWork = false;
			int longEdge = Math.Max(ow, oh);
			if (longEdge > maxLongEdge)
			{
				float scale = maxLongEdge / (float)longEdge;
				int nw = Math.Max(8, (int)(ow * scale) / 8 * 8);
				int nh = Math.Max(8, (int)(oh * scale) / 8 * 8);
				work = new Bitmap(nw, nh, PixelFormat.Format24bppRgb);
				disposeWork = true;
				using (Graphics g = Graphics.FromImage(work))
				{
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.DrawImage(source, 0, 0, nw, nh);
				}
			}

			try
			{
				cancel.ThrowIfCancellationRequested();
				float[] chw = BitmapToChw(work);
				var input = new DenseTensor<float>(chw, new[] { 1, 3, work.Height, work.Width });
				var inputs = new[] { NamedOnnxValue.CreateFromTensor("image", input) };
				using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = s.Run(inputs))
				{
					cancel.ThrowIfCancellationRequested();
					Tensor<float> restored = results[0].AsTensor<float>();
					Bitmap small = ChwToBitmap(restored);
					if (!disposeWork && small.Width == ow && small.Height == oh)
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
			finally
			{
				if (disposeWork)
				{
					work.Dispose();
				}
			}
		}

		private static float[] BitmapToChw(Bitmap bmp)
		{
			int w = bmp.Width;
			int h = bmp.Height;
			float[] data = new float[3 * h * w];
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
							// Format24bppRgb is BGR
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
			return data;
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
							int i = y * w + x;
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
