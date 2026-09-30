using System;
using System.Collections.Concurrent;
using System.Drawing;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>
	/// Registration for the active page-image filter. Default off.
	/// Per-window enable flags are stored by key; Active follows the current window.
	/// One shared ONNX runner (serialized) backs all windows when enabled.
	/// </summary>
	public static class PageImageFilterHost
	{
		private static readonly ConcurrentDictionary<object, ReaderWindowFilterState> windowStates =
			new ConcurrentDictionary<object, ReaderWindowFilterState>();

		private static FbcnnPageImageFilter fbcnn;
		private static IPageImageFilter active;
		private static object currentWindowKey = "default";
		private static string sharedModelPath;

		public static event EventHandler StatusChanged;

		public static IPageImageFilter Active => active;

		public static string StatusText => fbcnn?.StatusText ?? string.Empty;

		public static bool IsProcessing => fbcnn != null && fbcnn.IsProcessing;

		public static string LastError => fbcnn?.LastError ?? string.Empty;

		public static object CurrentWindowKey => currentWindowKey;

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
			RefreshActiveFromCurrentWindow();
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
				GetOrCreateState(currentWindowKey).Enabled = false;
				RefreshActiveFromCurrentWindow();
				return false;
			}
			sharedModelPath = onnxPath;
			ReaderWindowFilterState st = GetOrCreateState(currentWindowKey);
			st.Enabled = true;
			st.ModelPath = onnxPath;
			st.MaxLongEdge = maxLongEdge > 0 ? maxLongEdge : 1024;
			RefreshActiveFromCurrentWindow();
			return true;
		}

		public static void DisableFbcnn()
		{
			ReaderWindowFilterState st = GetOrCreateState(currentWindowKey);
			st.Enabled = false;
			bool anyEnabled = false;
			foreach (ReaderWindowFilterState s in windowStates.Values)
			{
				if (s.Enabled)
				{
					anyEnabled = true;
					break;
				}
			}
			if (!anyEnabled && fbcnn != null)
			{
				fbcnn.Disable();
			}
			RefreshActiveFromCurrentWindow();
		}

		public static void CancelInFlight()
		{
			fbcnn?.CancelWork(replaceToken: true);
		}

		/// <summary>
		/// Apply filter for a PageKey fingerprint captured when a window was enabled.
		/// Does not depend on <see cref="Active"/> / current window (prefetch-safe).
		/// </summary>
		public static Bitmap ApplyForFingerprint(string fingerprint, Bitmap source)
		{
			if (source == null || string.IsNullOrEmpty(fingerprint) || fbcnn == null || !fbcnn.IsLoadedEnough)
			{
				return source;
			}
			ReaderWindowFilterState matched = null;
			foreach (ReaderWindowFilterState s in windowStates.Values)
			{
				if (s.Enabled && string.Equals(s.Fingerprint, fingerprint, StringComparison.Ordinal))
				{
					matched = s;
					break;
				}
			}
			if (matched == null)
			{
				return source;
			}
			fbcnn.SyncFromWindowState(matched);
			return fbcnn.ApplyLoaded(source);
		}

		private static void RefreshActiveFromCurrentWindow()
		{
			ReaderWindowFilterState st = GetOrCreateState(currentWindowKey);
			if (st.Enabled && fbcnn != null && fbcnn.IsEnabled)
			{
				// Keep filter state.Enabled aligned with current window.
				fbcnn.State.Enabled = true;
				fbcnn.State.ModelPath = st.ModelPath ?? sharedModelPath;
				fbcnn.State.MaxLongEdge = st.MaxLongEdge > 0 ? st.MaxLongEdge : 1024;
				active = fbcnn;
			}
			else if (st.Enabled && fbcnn != null && !string.IsNullOrEmpty(st.ModelPath ?? sharedModelPath))
			{
				// Window wants enable but runner unloaded — leave Active null until EnableFbcnn.
				active = null;
			}
			else
			{
				if (fbcnn != null)
				{
					fbcnn.State.Enabled = false;
				}
				active = null;
			}
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
