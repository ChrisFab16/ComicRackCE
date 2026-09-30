using System.Drawing;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>
	/// Optional page bitmap post-process for reader display (never writes archives).
	/// Spike surface for FBCNN / artifact reduction (001-fbcnn-reader-toggle).
	/// </summary>
	public interface IPageImageFilter
	{
		/// <summary>Stable cache identity fragment (empty when inactive).</summary>
		string Fingerprint { get; }

		bool IsEnabled { get; }

		/// <summary>
		/// Transform a decoded page bitmap for display. Must not mutate comic archives.
		/// Return the input bitmap if unchanged; otherwise a new bitmap (caller disposes prior as needed).
		/// </summary>
		Bitmap Apply(Bitmap source);
	}
}
