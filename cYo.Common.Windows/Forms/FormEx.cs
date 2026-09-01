using cYo.Common.Windows.Forms.Theme;
using System;
using System.Windows.Forms;

namespace cYo.Common.Windows.Forms
{
    public class FormEx : Form, ITheme
    {
        private const int WM_DPICHANGED = 0x02E0;

        public virtual UIComponent UIComponent => UIComponent.Window;

        public virtual void ApplyTheme(Control control = null)
        {
            ThemeExtensions.Theme(control ?? this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTheme();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_DPICHANGED)
            {
                FormUtility.RefreshDpiScale(this);
            }
        }
    }
}
