using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using cYo.Common.Windows;
using cYo.Common.Windows.Forms;

namespace cYo.Projects.ComicRack.Viewer.Dialogs
{
	public partial class ProgressDialog : FormEx
	{
		private bool cancel;

		public ProgressDialog()
		{
			LocalizeUtility.UpdateRightToLeft(this);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScaleDimensions = new SizeF(96F, 96F);
			InitializeComponent();
			LocalizeUtility.Localize(this, null);
		}

		public bool Progress(int percentDone)
		{
			progressBar.Value = percentDone;
			Application.DoEvents();
			return cancel;
		}

		private void btCancel_Click(object sender, EventArgs e)
		{
			cancel = true;
		}
	}
}
