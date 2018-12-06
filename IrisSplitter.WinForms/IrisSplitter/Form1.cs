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
        Point pupilMiddle;

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
            if (image != null)
            {
                ChangeImage(Grayscale(image));
                ChangeImage(Binaryzation(image));
                ChangeImage(Erosion(image));
                ChangeImage(Dilation(image));

                ChangeImage(FindPupilMiddle(image));

                ChangeImage(RobertsCross(image));
            }

        }

        private void ChangeImage(Bitmap b)
        {
            image = b;
            pictureBox1.Image = image;
            pictureBox1.Refresh();
            System.Threading.Thread.Sleep(1000);
        }

        private Bitmap Grayscale(Bitmap b)
        {
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    int n = (b.GetPixel(i, j).R + b.GetPixel(i, j).G + b.GetPixel(i, j).B) / 3;
                    b.SetPixel(i, j, Color.FromArgb(n, n, n));
                }
            }
            return b;
        }

        private Bitmap Binaryzation(Bitmap b)
        {
            int threshold = Otsu(b) / 5;
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

        private int Otsu(Bitmap b)
        {
            int[] histogram = new int[256];
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    histogram[b.GetPixel(i, j).R]++;
                }
            }

            int total = b.Width * b.Height;

            float sum = 0;
            for (int i = 0; i < 256; i++)
                sum += i * histogram[i];

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            int threshold = 0;
            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];               // Weight Background
                if (wB == 0) continue;

                wF = total - wB;                 // Weight Foreground
                if (wF == 0) break;

                sumB += (float)(i * histogram[i]);

                float mB = sumB / wB;            // Mean Background
                float mF = (sum - sumB) / wF;    // Mean Foreground

                // Calculate Between Class Variance
                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = i;
                }
            }
            return threshold;
        }

        private Bitmap Dilation(Bitmap borg)
        {
            Bitmap bnew = new Bitmap(borg);
            for (int i = 0; i < borg.Width; i++)
            {
                for (int j = 0; j < borg.Height; j++)
                {
                    if (borg.GetPixel(i, j).ToArgb() == Color.Black.ToArgb() && i < borg.Width - 1)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            for (int dy = -2; dy <= 2; dy++)
                            {
                                if (i + dx >= 0 && i + dx < borg.Width && j + dy >= 0 && j + dy < borg.Height)
                                    bnew.SetPixel(i + dx, j + dy, Color.Black);
                            }
                        }
                    }
                }
            }
            return bnew;
        }

        private Bitmap Erosion(Bitmap borg)
        {
            Bitmap bnew = new Bitmap(borg);
            for (int i = 0; i < borg.Width; i++)
            {
                for (int j = 0; j < borg.Height; j++)
                {
                    if (borg.GetPixel(i, j).ToArgb() == Color.Black.ToArgb())
                    {
                        bool change = false;
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            for (int dy = -2; dy <= 2; dy++)
                            {
                                if (i + dx >= 0 && i + dx < borg.Width && j + dy >= 0 && j + dy < borg.Height)
                                    if (borg.GetPixel(i + dx, j + dy).ToArgb() == Color.White.ToArgb())
                                        change = true;
                            }
                        }
                        if (change)
                            bnew.SetPixel(i, j, Color.White);
                    }
                }
            }
            return bnew;
        }

        private Bitmap FindPupilMiddle(Bitmap b)
        {
            int[] xValues = new int[b.Width];
            int[] yValues = new int[b.Height];
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    if (b.GetPixel(i, j).ToArgb() == Color.Black.ToArgb())
                    {
                        xValues[i] ++;
                        yValues[j] ++;
                    }
                }
            }
            int xMax, yMax;
            xMax = yMax = 0;
            for (int i = 0; i < b.Width; i++)
            {
                if (xValues[i] > xValues[xMax])
                    xMax = i;
            }
            for (int i = 0; i < b.Height; i++)
            {
                if (yValues[i] > yValues[yMax])
                    yMax = i;
            }
            pupilMiddle = new Point(xMax, yMax);
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    b.SetPixel(xMax + i, yMax + j, Color.Red);
                }
            }
            
            return b;
        }

        private Bitmap RobertsCross(Bitmap borg)
        {
            Bitmap b = new Bitmap(borg);
            int t1, t2;
            int nc;
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    if (i + 1 < b.Width)
                        t1 = (borg.GetPixel(i + 1, j).R - borg.GetPixel(i, j).R) / 2;
                    else
                        t1 = -borg.GetPixel(i, j).R;

                    if (j + 1 < b.Height)
                        t2 = (borg.GetPixel(i, j + 1).R - borg.GetPixel(i, j).R) / 2;
                    else
                        t2 = -borg.GetPixel(i, j).R;

                    nc = (Math.Abs(t1) + Math.Abs(t2)) / 2;
                    b.SetPixel(i, j, Color.FromArgb(nc, nc, nc));
                }
            }
            return b;
        }
    }
}