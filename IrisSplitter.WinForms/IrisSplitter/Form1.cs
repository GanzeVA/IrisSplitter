using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IrisSplitter
{
    public partial class Form1 : Form
    {
        Bitmap orgImage;
        Bitmap outImage;
        Bitmap grayscaleImage;
        Bitmap pupilBinImage;
        Bitmap pupilOpenedImage;
        Bitmap pupilEdgeImage;
        Bitmap whiteBinImage;
        Bitmap whiteOpenedImage;
        Bitmap whiteEdgeImage;
        Bitmap irisRing;
        Bitmap irisRectangle;
        Point pupilMiddle;
        int pupilRadius;
        int irisRadius;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Load image";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                orgImage = new Bitmap(openFileDialog1.OpenFile());
                pictureBox1.Image = orgImage;
                irisRingPictureBox.Image = null;
                irisRectanglePictureBox.Image = null;
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (orgImage != null)
            {
                grayscaleImage = Grayscale(orgImage);
                pupilBinImage = BinaryzationPupil(grayscaleImage);
                pupilOpenedImage = Dilation(Erosion(pupilBinImage));
                pupilEdgeImage = RobertsCross(pupilOpenedImage);

                whiteBinImage = BinaryzationWhite(grayscaleImage);
                whiteOpenedImage = Dilation(Erosion(whiteBinImage));
                whiteEdgeImage = RobertsCross(whiteOpenedImage);

                FindPupil();
                FindIris();

                DrawPupilAndIrisBorder();
                SplitIrisRing();
                SplitIrisRectangle();

                Image saveImage = outImage;
                saveImage.Save("outImage.png", ImageFormat.Png);
                saveImage = irisRing;
                saveImage.Save("irisRing.png", ImageFormat.Png);
                saveImage = irisRectangle;
                saveImage.Save("irisRectangle.png", ImageFormat.Png);
            }
        }

        private void SplitIrisRing()
        {
            irisRing = new Bitmap(irisRadius * 2, irisRadius * 2);
            int xStart = pupilMiddle.X - irisRadius;
            int yStart = pupilMiddle.Y - irisRadius;
            
            for (int y = yStart; y < pupilMiddle.Y + irisRadius; y++)
            {
                for (int x = xStart; x < pupilMiddle.X + irisRadius; x++)
                {
                    double d = Math.Sqrt(Math.Pow(pupilMiddle.X - x, 2) + Math.Pow(pupilMiddle.Y - y, 2));
                    if (d <= irisRadius && d >= pupilRadius)
                    {
                        if (x - xStart >= 0 && y - yStart >= 0)
                            irisRing.SetPixel(x - xStart, y - yStart, orgImage.GetPixel(x, y));
                    }
                    else
                    {
                        if (x - xStart >= 0 && y - yStart >= 0)
                            irisRing.SetPixel(x - xStart, y - yStart, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }
            irisRingPictureBox.Image = irisRing;
        }

        private void SplitIrisRectangle()
        {
            irisRectangle = new Bitmap(200, 100);
            int offsetX, offsetY;
            offsetX = offsetY = 0;
            for (int i = 0; i < irisRing.Width; i++)
            {
                for (int j = 0; j < irisRing.Height; j++)
                {
                    if (irisRing.GetPixel(i, j).ToArgb() != Color.FromArgb(0, 0, 0, 0).ToArgb())
                    {
                        double angle = Math.Atan2(j - (irisRing.Height / 2), i - (irisRing.Width / 2)) - Math.Atan2(0, irisRing.Width / 2);
                        double d = Math.Sqrt(Math.Pow((irisRing.Width / 2) - i, 2) + Math.Pow((irisRing.Height / 2) - j, 2));
                        int x = (int)Math.Round(d * Math.Cos(angle));
                        int y = (int)Math.Round(d * Math.Sin(angle));
                        if (i == 0 && j == 0)
                        {
                            offsetX = -x;
                            offsetY = -y;
                        }
                        if (x + offsetX >= 0 && y + offsetY >= 0 && x + offsetX < irisRectangle.Width && y + offsetY < irisRectangle.Height)
                            irisRectangle.SetPixel(x + offsetX, y + offsetY, irisRing.GetPixel(i, j));
                    }
                }
            }
            irisRectanglePictureBox.Image = irisRectangle;
        }

        private void ChangeImage(Bitmap b)
        {
            pictureBox1.Image = b;
            pictureBox1.Refresh();
            System.Threading.Thread.Sleep(1000);
        }

        private void DrawPupilAndIrisBorder()
        {
            Bitmap b = new Bitmap(orgImage);
            Graphics g = Graphics.FromImage(b);
            Pen p = new Pen(Color.Red);
            g.DrawEllipse(p, pupilMiddle.X - pupilRadius, pupilMiddle.Y - pupilRadius, pupilRadius + pupilRadius, pupilRadius + pupilRadius);
            g.DrawEllipse(p, pupilMiddle.X - irisRadius, pupilMiddle.Y - irisRadius, irisRadius + irisRadius, irisRadius + irisRadius);
            ChangeImage(b);
            outImage = new Bitmap(b);
        }

        private Bitmap Grayscale(Bitmap borg)
        {
            Bitmap b = new Bitmap(borg);
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

        private Bitmap BinaryzationPupil(Bitmap borg)
        {
            Bitmap b = new Bitmap(borg);
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

        private Bitmap BinaryzationWhite(Bitmap borg)
        {
            Bitmap b = new Bitmap(borg);
            int threshold = (int)((double)Otsu(b) * 1.1);
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    if (b.GetPixel(i, j).R > threshold)
                    {
                        b.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        b.SetPixel(i, j, Color.Black);
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
                wB += histogram[i];
                if (wB == 0) continue;

                wF = total - wB;
                if (wF == 0) break;

                sumB += (float)(i * histogram[i]);

                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;

                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

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

        private void FindPupil()
        {
            int[] xValues = new int[pupilOpenedImage.Width];
            int[] yValues = new int[pupilOpenedImage.Height];
            for (int i = 0; i < pupilOpenedImage.Width; i++)
            {
                for (int j = 0; j < pupilOpenedImage.Height; j++)
                {
                    if (pupilOpenedImage.GetPixel(i, j).ToArgb() == Color.Black.ToArgb())
                    {
                        xValues[i]++;
                        yValues[j]++;
                    }
                }
            }
            int xMax, yMax;
            xMax = yMax = 0;
            for (int i = 0; i < pupilOpenedImage.Width; i++)
            {
                if (xValues[i] > xValues[xMax])
                    xMax = i;
            }
            for (int i = 0; i < pupilOpenedImage.Height; i++)
            {
                if (yValues[i] > yValues[yMax])
                    yMax = i;
            }
            pupilMiddle = new Point(xMax, yMax);

            int xLeft, xRight, yUp, yDown;
            xLeft = xRight = pupilMiddle.X;
            yUp = yDown = pupilMiddle.Y;
            for (int i = pupilMiddle.X; i >= 0; i--)
            {
                if (pupilOpenedImage.GetPixel(i, pupilMiddle.Y).ToArgb() != Color.Black.ToArgb())
                {
                    xLeft = i;
                    break;
                }
            }
            for (int i = pupilMiddle.X; i < pupilEdgeImage.Width; i++)
            {
                if (pupilEdgeImage.GetPixel(i, pupilMiddle.Y).ToArgb() != Color.Black.ToArgb())
                {
                    xRight = i;
                    break;
                }
            }
            for (int i = pupilMiddle.Y; i >= 0; i--)
            {
                if (pupilEdgeImage.GetPixel(pupilMiddle.X, i).ToArgb() != Color.Black.ToArgb())
                {
                    yUp = i;
                    break;
                }
            }
            for (int i = pupilMiddle.Y; i < pupilEdgeImage.Height; i++)
            {
                if (pupilEdgeImage.GetPixel(pupilMiddle.X, i).ToArgb() != Color.Black.ToArgb())
                {
                    yDown = i;
                    break;
                }
            }
            pupilMiddle = new Point((xLeft + xRight) / 2, (yUp + yDown) / 2);
            pupilRadius = Math.Max(Math.Max(Math.Max(Math.Abs(pupilMiddle.X - xLeft), Math.Abs(pupilMiddle.X - xRight)), Math.Abs(pupilMiddle.Y - yUp)), Math.Abs(pupilMiddle.Y - yDown));
        }

        private void FindIris()
        {
            int xLeft, xRight, yUp, yDown;
            xLeft = xRight = pupilMiddle.X;
            yUp = yDown = pupilMiddle.Y;
            for (int i = pupilMiddle.X; i >= 0; i--)
            {
                if (whiteOpenedImage.GetPixel(i, pupilMiddle.Y).ToArgb() != Color.Black.ToArgb())
                {
                    xLeft = i;
                    break;
                }
            }
            for (int i = pupilMiddle.X; i < pupilEdgeImage.Width; i++)
            {
                if (whiteOpenedImage.GetPixel(i, pupilMiddle.Y).ToArgb() != Color.Black.ToArgb())
                {
                    xRight = i;
                    break;
                }
            }
            irisRadius = Math.Max(Math.Max(Math.Abs(pupilMiddle.X - xLeft), Math.Abs(pupilMiddle.X - xRight)), pupilRadius);
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

        private void example1jpgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            orgImage = Properties.Resources.Example1;
            pictureBox1.Image = orgImage;
            irisRingPictureBox.Image = null;
            irisRectanglePictureBox.Image = null;
        }

        private void example2jpgToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void example3jpgToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}