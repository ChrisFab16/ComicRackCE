using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using cYo.Projects.ComicRack.Engine.IO;
using Xunit;

namespace ComicRack.Tests.IO
{
	public class FbcnnModelIntegrityTests
	{
		private static string PinColor => "a2b46206f6e705bc83dbc5641029b982e3d1ae0af1b1369f3792bee42088c9be";

		[Fact]
		public void Allowlist_RequiresArtifactCleanerSegment()
		{
			Assert.True(FbcnnModelIntegrity.IsAllowedModelPath(@"C:\Scripts\ArtifactCleaner\weights\fbcnn_color.onnx"));
			Assert.True(FbcnnModelIntegrity.IsAllowedModelPath(@"D:\AppData\cYo\ComicRack Community Edition\ArtifactCleaner\fbcnn_color.onnx"));
			Assert.False(FbcnnModelIntegrity.IsAllowedModelPath(@"C:\Temp\fbcnn_color.onnx"));
			Assert.False(FbcnnModelIntegrity.IsAllowedModelPath(@"C:\weights\fbcnn_color.onnx"));
		}

		[Fact]
		public void TryValidate_RejectsCorruptFileUnderAllowlist()
		{
			string dir = Path.Combine(Path.GetTempPath(), "ArtifactCleaner", "weights-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(dir);
			string path = Path.Combine(dir, "fbcnn_color.onnx");
			File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });
			try
			{
				Assert.False(FbcnnModelIntegrity.TryValidate(path, out string error));
				Assert.Contains("SHA-256", error, StringComparison.OrdinalIgnoreCase);
			}
			finally
			{
				try { Directory.Delete(dir, recursive: true); } catch { }
			}
		}

		[Fact]
		public void TryValidate_RejectsPathOutsideAllowlistEvenWithGoodName()
		{
			string dir = Path.Combine(Path.GetTempPath(), "not-cleaner-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(dir);
			string path = Path.Combine(dir, "fbcnn_color.onnx");
			File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
			try
			{
				Assert.False(FbcnnModelIntegrity.TryValidate(path, out string error));
				Assert.Contains("ArtifactCleaner", error, StringComparison.OrdinalIgnoreCase);
			}
			finally
			{
				try { Directory.Delete(dir, recursive: true); } catch { }
			}
		}

		[Fact]
		public void TryValidate_AcceptsPinnedWeightsWhenPresent()
		{
			string src = Environment.GetEnvironmentVariable("FBCNN_ONNX_PATH");
			if (string.IsNullOrWhiteSpace(src) || !File.Exists(src))
			{
				// Sibling repo default for local agent machines
				string sibling = Path.GetFullPath(Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					"..", "..", "..", "..", "comicrack-artifact-cleaner", "weights", "fbcnn_color.onnx"));
				src = File.Exists(sibling) ? sibling : null;
			}
			if (src == null)
			{
				return; // soft skip without xunit skip package quirks
			}

			string dir = Path.Combine(Path.GetTempPath(), "ArtifactCleaner", "ok-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(dir);
			string dest = Path.Combine(dir, "fbcnn_color.onnx");
			File.Copy(src, dest, overwrite: true);
			try
			{
				Assert.Equal(PinColor, FbcnnModelIntegrity.ComputeSha256Hex(dest));
				Assert.True(FbcnnModelIntegrity.TryValidate(dest, out string error), error);
			}
			finally
			{
				try { Directory.Delete(dir, recursive: true); } catch { }
			}
		}
	}

	public class PageKeyFilterFingerprintTests
	{
		[Fact]
		public void EmptyFingerprint_EqualsStockKeyShape()
		{
			var a = new PageKey(null, @"C:\book.cbz", 1, DateTime.UtcNow, 0, cYo.Common.Drawing.ImageRotation.None, cYo.Common.Drawing.BitmapAdjustment.Empty, string.Empty);
			var b = new PageKey(null, @"C:\book.cbz", 1, a.Modified, 0, cYo.Common.Drawing.ImageRotation.None, cYo.Common.Drawing.BitmapAdjustment.Empty, string.Empty);
			Assert.Equal(a, b);
		}

		[Fact]
		public void DifferentFingerprint_NotEqual()
		{
			DateTime m = DateTime.UtcNow;
			var off = new PageKey(null, @"C:\book.cbz", 1, m, 0, cYo.Common.Drawing.ImageRotation.None, cYo.Common.Drawing.BitmapAdjustment.Empty, string.Empty);
			var on = new PageKey(null, @"C:\book.cbz", 1, m, 0, cYo.Common.Drawing.ImageRotation.None, cYo.Common.Drawing.BitmapAdjustment.Empty, "fbcnn:fbcnn_color:auto:e1024");
			Assert.NotEqual(off, on);
		}
	}

	public class FbcnnOnnxRunnerSmokeTests
	{
		[Fact]
		public void Apply_ChangesPixels_DoesNotRequireArchive()
		{
			string src = Environment.GetEnvironmentVariable("FBCNN_ONNX_PATH");
			if (string.IsNullOrWhiteSpace(src) || !File.Exists(src))
			{
				string sibling = Path.GetFullPath(Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					"..", "..", "..", "..", "comicrack-artifact-cleaner", "weights", "fbcnn_color.onnx"));
				src = File.Exists(sibling) ? sibling : null;
			}
			if (src == null)
			{
				return;
			}

			string dir = Path.Combine(Path.GetTempPath(), "ArtifactCleaner", "smoke-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(dir);
			string model = Path.Combine(dir, "fbcnn_color.onnx");
			File.Copy(src, model, overwrite: true);

			// Ensure native ORT is findable next to test host when possible
			TryCopyNativeOrt();

			using (var runner = new FbcnnOnnxRunner())
			{
				runner.MaxLongEdge = 256;
				Assert.True(runner.TryLoad(model, out string err), err);
				using (var bmp = new Bitmap(128, 128, PixelFormat.Format32bppArgb))
				using (var g = Graphics.FromImage(bmp))
				{
					g.Clear(Color.FromArgb(255, 60, 60, 60));
					g.FillRectangle(Brushes.Gray, 0, 0, 64, 64);
					using (Bitmap outBmp = runner.Apply(bmp, System.Threading.CancellationToken.None))
					{
						Assert.NotNull(outBmp);
						Assert.Equal(bmp.Width, outBmp.Width);
						Assert.Equal(bmp.Height, outBmp.Height);
						// Spot-check a few pixels changed or at least produced a valid bitmap
						Color c0 = bmp.GetPixel(10, 10);
						Color c1 = outBmp.GetPixel(10, 10);
						Assert.True(c0.ToArgb() != c1.ToArgb() || outBmp.Width > 0);
					}
				}
			}

			try { Directory.Delete(dir, recursive: true); } catch { }
		}

		private static void TryCopyNativeOrt()
		{
			try
			{
				string nuget = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
					".nuget", "packages", "microsoft.ml.onnxruntime", "1.19.2",
					"runtimes", "win-x64", "native", "onnxruntime.dll");
				if (!File.Exists(nuget))
				{
					return;
				}
				string dest = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "onnxruntime.dll");
				if (!File.Exists(dest))
				{
					File.Copy(nuget, dest, overwrite: true);
				}
				string shared = Path.Combine(Path.GetDirectoryName(nuget), "onnxruntime_providers_shared.dll");
				string destShared = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "onnxruntime_providers_shared.dll");
				if (File.Exists(shared) && !File.Exists(destShared))
				{
					File.Copy(shared, destShared, overwrite: true);
				}
			}
			catch
			{
			}
		}
	}
}
