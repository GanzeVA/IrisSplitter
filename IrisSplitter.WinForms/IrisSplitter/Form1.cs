using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IrisSplitter
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Bitmap orgImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Load image";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(openFileDialog1.OpenFile());
                orgImage = new Bitmap(openFileDialog1.OpenFile());
                pictureBox1.Image = image;
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            image = binImage(image);
            pictureBox1.Image = image;
        }

        private Bitmap binImage(Bitmap b)
        {
            //grayscale
            int threshold = 0;
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    int n = (b.GetPixel(i, j).R + b.GetPixel(i, j).G + b.GetPixel(i, j).B) / 3;
                    b.SetPixel(i, j, Color.FromArgb(n, n, n));
                    threshold += n;
                }
            }

            //binaryzation
            threshold /= (b.Width * b.Height);
            threshold /= 2;
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    if (b.GetPixel(i, j).R < threshold)
                    {
                        b.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        b.SetPixel(i, j, Color.White);
                    }
                }
            }
            return b;
        }
    }
}
