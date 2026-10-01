using System;
using System.Drawing;
using System.Threading;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>Per reader-window artifact-reduction state (display-only).</summary>
	public sealed class ReaderWindowFilterState
	{
		public bool Enabled { get; set; }

		public string ModelId { get; set; } = "fbcnn_color";

		public string ModelPath { get; set; }

		public string QualityMode { get; set; } = "Automatic";

		public int? QualityFactor { get; set; }

		public bool JpegOnly { get; set; }

		public int MaxLongEdge { get; set; } = 1024;

		public string Fingerprint
		{
			get
			{
				if (!Enabled)
				{
					return string.Empty;
				}
				string q = QualityMode == "Manual" && QualityFactor.HasValue
					? "m" + QualityFactor.Value
					: "auto";
				return string.Format("fbcnn:{0}:{1}:e{2}", ModelId ?? "fbcnn_color", q, MaxLongEdge);
			}
		}
	}

	public sealed class FbcnnPageImageFilter : IPageImageFilter, IDisposable
	{
		private readonly FbcnnOnnxRunner runner = new FbcnnOnnxRunner();
		private ReaderWindowFilterState state = new ReaderWindowFilterState();
		private CancellationTokenSource cts = new CancellationTokenSource();
		private readonly object ctsGate = new object();
		private int processing;

		public event EventHandler StatusChanged;

		public string StatusText { get; private set; } = string.Empty;

		public bool IsProcessing => Interlocked.CompareExchange(ref processing, 0, 0) != 0;

		public string LastError => runner.LastError;

		public ReaderWindowFilterState State => state;

		public string Fingerprint => state.Fingerprint;

		public bool IsEnabled => state.Enabled && runner.IsLoaded;

		/// <summary>True when the ONNX session is loaded (window Enabled may still be false).</summary>
		public bool IsLoadedEnough => runner.IsLoaded;

		public bool TryEnable(string onnxPath, int maxLongEdge, out string error)
		{
			error = string.Empty;
			CancelWork(replaceToken: true);
			runner.MaxLongEdge = maxLongEdge > 0 ? maxLongEdge : 1024;
			if (!runner.TryLoad(onnxPath, out error))
			{
				state.Enabled = false;
				SetStatus(error);
				return false;
			}
			state.Enabled = true;
			state.ModelPath = onnxPath;
			state.MaxLongEdge = runner.MaxLongEdge;
			SetStatus("Artifact reduction on (" + runner.ActiveExecutionProvider + ")");
			return true;
		}

		public void Disable()
		{
			CancelWork(replaceToken: true);
			state.Enabled = false;
			SetStatus(string.Empty);
		}

		public void SyncFromWindowState(ReaderWindowFilterState windowState)
		{
			if (windowState == null)
			{
				return;
			}
			state.ModelPath = windowState.ModelPath;
			state.ModelId = windowState.ModelId;
			state.QualityMode = windowState.QualityMode;
			state.QualityFactor = windowState.QualityFactor;
			state.MaxLongEdge = windowState.MaxLongEdge > 0 ? windowState.MaxLongEdge : 1024;
			runner.MaxLongEdge = state.MaxLongEdge;
		}

		/// <summary>
		/// Cancel in-flight work. ORT Run cannot abort mid-call; after cancel, token is replaced
		/// so later Apply calls are not stuck cancelled while still enabled.
		/// </summary>
		public void CancelWork(bool replaceToken = true)
		{
			lock (ctsGate)
			{
				try
				{
					cts.Cancel();
				}
				catch
				{
				}
				if (replaceToken)
				{
					try
					{
						cts.Dispose();
					}
					catch
					{
					}
					cts = new CancellationTokenSource();
				}
			}
			Interlocked.Exchange(ref processing, 0);
		}

		public Bitmap Apply(Bitmap source)
		{
			if (!IsEnabled || source == null)
			{
				return source;
			}
			return ApplyLoaded(source);
		}

		/// <summary>
		/// Run inference when the ONNX session is loaded, ignoring per-window Enabled.
		/// Used when applying for a cache key whose fingerprint was captured while a window was on.
		/// </summary>
		public Bitmap ApplyLoaded(Bitmap source)
		{
			if (source == null || !runner.IsLoaded)
			{
				return source;
			}
			CancellationToken token;
			lock (ctsGate)
			{
				token = cts.Token;
			}
			Interlocked.Exchange(ref processing, 1);
			// Do not SetStatus on every Apply — floods StatusChanged → UI BeginInvoke and
			// amplifies perceived load while prefetch queues many pages.
			try
			{
				return runner.Apply(source, token);
			}
			catch (OperationCanceledException)
			{
				return source;
			}
			catch (Exception ex)
			{
				SetStatus("FBCNN failed: " + ex.Message);
				return source;
			}
			finally
			{
				Interlocked.Exchange(ref processing, 0);
			}
		}

		private void SetStatus(string text)
		{
			StatusText = text ?? string.Empty;
			StatusChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			CancelWork(replaceToken: false);
			runner.Dispose();
			lock (ctsGate)
			{
				try
				{
					cts.Dispose();
				}
				catch
				{
				}
			}
		}
	}
}
