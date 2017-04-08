using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace VirtualQuizBox
{
    public class ColorCheckBox : CheckBox
    {
        System.Drawing.Image uncheckedImage;
        System.Drawing.Image checkedImage;

        public ColorCheckBox()
        {
            uncheckedImage = Image.FromFile("");
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            //pevent.Graphics.DrawImage();
        }
    }
}
