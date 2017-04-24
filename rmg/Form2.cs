using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rmg
{
    public partial class Form2 : Form
    {
        public Form2(Bitmap img)
        {
            InitializeComponent();
            pictureBox1.Size = new Size(img.Width,img.Height);
            pictureBox1.Image = img;
            this.Size = new Size(Math.Min(img.Width,1280), Math.Min(img.Height, 1024));
            this.AutoScroll = true;
        }
    }
}
