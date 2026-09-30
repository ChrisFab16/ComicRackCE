using System;
using System.Collections.Concurrent;
using System.Drawing;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>
	/// Registration for the active page-image filter. Default off.
	/// Per-window state is stored by key; Active reflects the current reader window.
	/// </summary>
	public static class PageImageFilterHost
	{
		private static readonly ConcurrentDictionary<object, ReaderWindowFilterState> windowStates =
			new ConcurrentDictionary<object, ReaderWindowFilterState>();

		private static FbcnnPageImageFilter fbcnn;
		private static IPageImageFilter active;
		private static object currentWindowKey = "default";

		public static event EventHandler StatusChanged;

		public static IPageImageFilter Active
		{
			get => active;
			set => active = value;
		}

		public static string StatusText => fbcnn?.StatusText ?? string.Empty;

		public static bool IsProcessing => fbcnn != null && fbcnn.IsProcessing;

		public static string LastError => fbcnn?.LastError ?? string.Empty;

		public static string CurrentFingerprint
		{
			get
			{
				IPageImageFilter f = active;
				if (f == null || !f.IsEnabled)
				{
					return string.Empty;
				}
				return f.Fingerprint ?? string.Empty;
			}
		}

		public static void SetCurrentWindow(object windowKey)
		{
			currentWindowKey = windowKey ?? "default";
		}

		public static ReaderWindowFilterState GetOrCreateState(object windowKey)
		{
			object key = windowKey ?? "default";
			return windowStates.GetOrAdd(key, _ => new ReaderWindowFilterState());
		}

		/// <summary>Enable FBCNN for the current window. Lazy-loads ONNX. Returns false on failure.</summary>
		public static bool EnableFbcnn(string onnxPath, int maxLongEdge, out string error)
		{
			error = string.Empty;
			if (fbcnn == null)
			{
				fbcnn = new FbcnnPageImageFilter();
				fbcnn.StatusChanged += (s, e) => StatusChanged?.Invoke(null, EventArgs.Empty);
			}
			if (!fbcnn.TryEnable(onnxPath, maxLongEdge, out error))
			{
				active = null;
				GetOrCreateState(currentWindowKey).Enabled = false;
				return false;
			}
			ReaderWindowFilterState st = GetOrCreateState(currentWindowKey);
			st.Enabled = true;
			st.ModelPath = onnxPath;
			st.MaxLongEdge = maxLongEdge > 0 ? maxLongEdge : 1024;
			active = fbcnn;
			return true;
		}

		public static void DisableFbcnn()
		{
			if (fbcnn != null)
			{
				fbcnn.Disable();
			}
			GetOrCreateState(currentWindowKey).Enabled = false;
			active = null;
		}

		public static void CancelInFlight()
		{
			fbcnn?.CancelWork();
		}

		/// <summary>Enable a no-op clone filter for ImagePool pipeline spike (T007/T008).</summary>
		public static void EnableDevIdentityFilter(bool enabled)
		{
			if (!enabled)
			{
				if (active is DevIdentityPageImageFilter)
				{
					active = null;
				}
				return;
			}
			active = new DevIdentityPageImageFilter();
		}

		private sealed class DevIdentityPageImageFilter : IPageImageFilter
		{
			public string Fingerprint => "dev-identity:1";

			public bool IsEnabled => true;

			public Bitmap Apply(Bitmap source)
			{
				if (source == null)
				{
					return null;
				}
				return (Bitmap)source.Clone();
			}
		}
	}
}
