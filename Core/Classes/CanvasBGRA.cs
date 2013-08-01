using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        

        public int WidthPixel
        {
            get { return this._width / 4; }
        }

        public int HeightPixel
        {
            get { return this._height / 4; }
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
                unsafe
                {
                    Marshal.Copy(bmdSRC.Scan0, canvas.Data, 0, canvas.Data.Length);
                }
            }
            finally
            {
                bmp.UnlockBits(bmdSRC);
            }

            return canvas;
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
    }
}
