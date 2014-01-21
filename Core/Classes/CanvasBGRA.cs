using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GenArt.Classes;

namespace GenArt.Core.Classes
{
    public class CanvasBGRA : Array2D
    {
        public const int CONST_PixelSize = 4; 

        public CanvasBGRA(int pixelWidth, int pixelHeight):
            base(pixelWidth, pixelHeight,CONST_PixelSize)

        {
            this._width = pixelWidth * CONST_PixelSize;
            this._height = pixelHeight * CONST_PixelSize;

        }

        

        public short WidthPixel
        {
            get { return (short)(this._width / 4); }
        }

        public short HeightPixel
        {
            get { return (short)(this._height / 4); }
        }

        public int CountPixels
        {
            get { return this.HeightPixel * this.WidthPixel; }
        }


        public static CanvasBGRA CreateCanvasFromBitmap(Bitmap bmp)
        {
            if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                throw new Exception("Not supported format bitmap;");

            CanvasBGRA canvas = new CanvasBGRA(bmp.Width, bmp.Height);

            BitmapData bmdSRC = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {
                Marshal.Copy(bmdSRC.Scan0, canvas.Data, 0, canvas.Data.Length);
            }
            finally
            {
                bmp.UnlockBits(bmdSRC);
            }

            return canvas;
        }

        public static Bitmap CreateBitmpaFromCanvas(CanvasBGRA canvas)
        {
            Bitmap result = new Bitmap(canvas.WidthPixel,canvas.HeightPixel,PixelFormat.Format32bppPArgb);

            BitmapData bmdSRC = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {
                Marshal.Copy(canvas.Data, 0, bmdSRC.Scan0, canvas.Data.Length);
            }
            finally
            {
                result.UnlockBits(bmdSRC);
            }

            return result;
        }


        public void FastClearColor(Color color)
        {
            if (this.Data.Length < 4)
                return;

            unsafe
            {
                fixed (byte * tmpcurrentPtr = this.Data)
                {
                    int canvasPixelCount = this.Data.Length >> 2;


                    ulong * currentPtr = (ulong*)(tmpcurrentPtr + 4);
                    uint * startPtr = (uint*)(tmpcurrentPtr);
                    uint colorForFill = (uint)((color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R));
                    ulong colorForFillLong = (((ulong)colorForFill) << 32) | colorForFill;

                    *(startPtr) = colorForFill;


                    //int totalLength = (Tools.MaxWidth * Tools.MaxHeight * 4);
                    ulong * end = currentPtr + (canvasPixelCount >> 1);


                    while (currentPtr < end)
                    {
                        if ((end - currentPtr) > 4)
                        {
                            *currentPtr = colorForFillLong;
                            *(currentPtr + 1) = colorForFillLong;
                            *(currentPtr + 2) = colorForFillLong;
                            *(currentPtr + 3) = colorForFillLong;
                            currentPtr += 4;
                        }
                        else
                        {
                            *currentPtr = colorForFillLong;
                            currentPtr++;
                        }
                        //*(currentPtr) = colorForFillLong;
                        //currentPtr++;
                    }


                }
            }
        }

        public void EasyColorReduction()
        {
            int index = 0;

            while (index < this.Data.Length)
            {
                this.Data[index] &= 0xf0;
                this.Data[index+1] &= 0xf8;
                this.Data[index+2] &= 0xf0;
                index+=CONST_PixelSize;
            }
        }

       public void ReduceNoiseMedian(bool meanSimilar = false)
        {
            Median8bit median = new Median8bit();

            int upRowIndex = 0;
            int midRowIndex = this._width;
            int downRowIndex = this._width * 2;

            byte [] img = new byte[this.Length];

            this.Data.CopyTo(img, 0);

            int threshold = 16;
            for (int y = 1; y < this.HeightPixel - 1; y++)
            {
                int upIndex = upRowIndex + CONST_PixelSize;
                int midIndex = midRowIndex + CONST_PixelSize;
                int downIndex = downRowIndex + CONST_PixelSize;

                
                for (int x = 1; x < this.WidthPixel - 1; x++)
                {
                    median.Clear();
                    median.InsertData(img[upIndex - CONST_PixelSize]);
                    median.InsertData(img[upIndex]);
                    median.InsertData(img[upIndex + CONST_PixelSize]);
                    median.InsertData(img[midIndex - CONST_PixelSize]);
                   // median.InsertData(img[midIndex]);
                    median.InsertData(img[midIndex + CONST_PixelSize]);
                    median.InsertData(img[downIndex - CONST_PixelSize]);
                    median.InsertData(img[downIndex]);
                    median.InsertData(img[downIndex + CONST_PixelSize]);

                    if ((!meanSimilar && Tools.fastAbs((int)median.Median - img[midIndex]) < threshold) ||
                        (meanSimilar && Tools.fastAbs((int)median.Median - img[midIndex]) > threshold)) 
                        this.Data[midIndex] = img[midIndex];
                    else
                        this.Data[midIndex] = (byte)median.Median;

                    upIndex++;
                    midIndex++;
                    downIndex++;

                    median.Clear();
                    median.InsertData(img[upIndex - CONST_PixelSize]);
                    median.InsertData(img[upIndex]);
                    median.InsertData(img[upIndex + CONST_PixelSize]);
                    median.InsertData(img[midIndex - CONST_PixelSize]);
                    //median.InsertData(img[midIndex]);
                    median.InsertData(img[midIndex + CONST_PixelSize]);
                    median.InsertData(img[downIndex - CONST_PixelSize]);
                    median.InsertData(img[downIndex]);
                    median.InsertData(img[downIndex + CONST_PixelSize]);

                    if ((!meanSimilar && Tools.fastAbs((int)median.Median - img[midIndex]) < threshold) ||
                        (meanSimilar && Tools.fastAbs((int)median.Median - img[midIndex]) > threshold))
                        this.Data[midIndex] = img[midIndex];
                    else
                        this.Data[midIndex] = (byte)median.Median;


                    upIndex++;
                    midIndex++;
                    downIndex++;

                    median.Clear();
                    median.InsertData(img[upIndex - CONST_PixelSize]);
                    median.InsertData(img[upIndex]);
                    median.InsertData(img[upIndex + CONST_PixelSize]);
                    median.InsertData(img[midIndex - CONST_PixelSize]);
                    //median.InsertData(img[midIndex]);
                    median.InsertData(img[midIndex + CONST_PixelSize]);
                    median.InsertData(img[downIndex - CONST_PixelSize]);
                    median.InsertData(img[downIndex]);
                    median.InsertData(img[downIndex + CONST_PixelSize]);

                    if ((!meanSimilar && Tools.fastAbs((int)median.Median - img[midIndex]) < threshold) ||
                                            (meanSimilar && Tools.fastAbs((int)median.Median - img[midIndex]) > threshold))
                        this.Data[midIndex] = img[midIndex];
                    else
                        this.Data[midIndex] = (byte)median.Median;


                    upIndex++;
                    midIndex++;
                    downIndex++;

                    upIndex++;
                    midIndex++;
                    downIndex++;

                }

                upRowIndex += this._width;
                midRowIndex += this._width;
                downRowIndex += this._width;
            }
        }
    }
}
