using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>
	/// FR-019: pin + path allowlist before ORT InferenceSession.
	/// Digests must match contracts/model-package.md in comicrack-artifact-cleaner.
	/// </summary>
	public static class FbcnnModelIntegrity
	{
		private static readonly Dictionary<string, string> PinnedSha256ByFileName =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "fbcnn_color.onnx", "a2b46206f6e705bc83dbc5641029b982e3d1ae0af1b1369f3792bee42088c9be" },
				{ "fbcnn_color_manual.onnx", "a6a7d028232f4510d052f520c64e6c76a40e6a29e60ea16549322d80c9a1b206" },
			};

		/// <summary>
		/// Validate path allowlist + filename pin + SHA-256. Fail closed with error message.
		/// </summary>
		public static bool TryValidate(string onnxPath, out string error)
		{
			error = string.Empty;
			if (string.IsNullOrWhiteSpace(onnxPath))
			{
				error = "ONNX model path is empty";
				return false;
			}
			string full;
			try
			{
				full = Path.GetFullPath(onnxPath);
			}
			catch (Exception ex)
			{
				error = "Invalid model path: " + ex.Message;
				return false;
			}
			if (!File.Exists(full))
			{
				error = "ONNX model file not found: " + full;
				return false;
			}
			if (!IsAllowedModelPath(full))
			{
				error = "Model path must be under an ArtifactCleaner directory (plugin Scripts or AppData)";
				return false;
			}
			string fileName = Path.GetFileName(full);
			if (!PinnedSha256ByFileName.TryGetValue(fileName, out string expected))
			{
				error = "Unrecognized model file name (expected fbcnn_color.onnx or fbcnn_color_manual.onnx): " + fileName;
				return false;
			}
			string actual;
			try
			{
				actual = ComputeSha256Hex(full);
			}
			catch (Exception ex)
			{
				error = "Could not hash model file: " + ex.Message;
				return false;
			}
			if (!FixedTimeEqualsHex(actual, expected))
			{
				error = "Model SHA-256 mismatch (corrupt or untrusted file). Expected pin for " + fileName;
				return false;
			}
			return true;
		}

		public static bool IsAllowedModelPath(string fullPath)
		{
			if (string.IsNullOrEmpty(fullPath))
			{
				return false;
			}
			string normalized = fullPath.Replace('/', Path.DirectorySeparatorChar);
			string marker = Path.DirectorySeparatorChar + "ArtifactCleaner" + Path.DirectorySeparatorChar;
			string markerEnd = Path.DirectorySeparatorChar + "ArtifactCleaner";
			int idx = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
			if (idx >= 0)
			{
				return true;
			}
			return normalized.EndsWith(markerEnd, StringComparison.OrdinalIgnoreCase);
		}

		public static string ComputeSha256Hex(string filePath)
		{
			using (var fs = File.OpenRead(filePath))
			using (var sha = SHA256.Create())
			{
				byte[] hash = sha.ComputeHash(fs);
				var sb = new StringBuilder(hash.Length * 2);
				for (int i = 0; i < hash.Length; i++)
				{
					sb.Append(hash[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}

		private static bool FixedTimeEqualsHex(string a, string b)
		{
			if (a == null || b == null || a.Length != b.Length)
			{
				return false;
			}
			int diff = 0;
			for (int i = 0; i < a.Length; i++)
			{
				diff |= char.ToLowerInvariant(a[i]) ^ char.ToLowerInvariant(b[i]);
			}
			return diff == 0;
		}
	}
}
