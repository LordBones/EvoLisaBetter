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
    public class CanvasARGBSplit
    {
        public short Width;
        public short Height;
        public byte [] G;
        public byte [] B;
        public byte [] A;
        public byte [] R;

        public int CountPixels
        {
            get { return G.Length; }
        }

        public CanvasARGBSplit(short widht , short height )
        {
            Width = widht;
            Height = height;

            int totalSize = widht*height;
            G = new byte[totalSize];
            B = new byte[totalSize];

            R = new byte[totalSize];
            A = new byte[totalSize];
        }

        public static CanvasARGBSplit CreateCanvasFromBitmap(Bitmap bmp)
        {
            if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                throw new Exception("Not supported format bitmap;");

            CanvasARGBSplit canvas = new CanvasARGBSplit((short)bmp.Width, (short)bmp.Height);

            BitmapData bmdSRC = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);

            byte [] tmpBuffer = new byte[canvas.CountPixels * 4];

            try
            {
                Marshal.Copy(bmdSRC.Scan0, tmpBuffer, 0, tmpBuffer.Length);
            }
            finally
            {
                bmp.UnlockBits(bmdSRC);
            }

            int tmpIndex = 0;
            for (int i =0; i < canvas.CountPixels; i++)
            {
                canvas.B[i] = tmpBuffer[tmpIndex];
                canvas.G[i] = tmpBuffer[tmpIndex + 1]  ;
                canvas.R[i] = tmpBuffer[tmpIndex + 2]  ;
                canvas.A[i] = tmpBuffer[tmpIndex + 3]  ;
                tmpIndex += 4;
            }

            return canvas;
        }

        public static Bitmap CreateBitmpaFromCanvas(CanvasARGBSplit canvas)
        {
            byte [] tmpBuffer = new byte[canvas.CountPixels*4];

            int tmpIndex = 0;
            for (int i =0; i < canvas.CountPixels; i++)
            {
                tmpBuffer[tmpIndex] = canvas.B[i];
                tmpBuffer[tmpIndex+1] = canvas.G[i];
                tmpBuffer[tmpIndex+2] = canvas.R[i];
                tmpBuffer[tmpIndex+3] = canvas.A[i];
                tmpIndex += 4;
            }

            Bitmap result = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppPArgb);

            BitmapData bmdSRC = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppPArgb);
            try
            {
                
                Marshal.Copy(tmpBuffer, 0, bmdSRC.Scan0, tmpBuffer.Length);
            }
            finally
            {
                result.UnlockBits(bmdSRC);
            }

            return result;
        }

        public static CanvasARGB CreateCanvasARGBFromCanvas(CanvasARGBSplit canvas)
        {
            CanvasARGB result = new CanvasARGB(canvas.Width, canvas.Height);
            int tmpIndex = 0;
            for (int i =0; i < canvas.CountPixels; i++)
            {
                result.Data[tmpIndex] = canvas.B[i];
                result.Data[tmpIndex + 1] = canvas.G[i];
                result.Data[tmpIndex + 2] = canvas.R[i];
                result.Data[tmpIndex + 3] = canvas.A[i];
                tmpIndex += 4;
            }

            return result;
        }

        public static CanvasARGBSplit CreateCanvasFromCanvasARGB(CanvasARGB canvasargb)
        {
            CanvasARGBSplit canvas = new CanvasARGBSplit((short)canvasargb.WidthPixel, (short)canvasargb.HeightPixel);

            int tmpIndex = 0;
            for (int i =0; i < canvas.CountPixels; i++)
            {
                canvas.B[i] = canvasargb.Data[tmpIndex];
                canvas.G[i] = canvasargb.Data[tmpIndex + 1];
                canvas.R[i] = canvasargb.Data[tmpIndex + 2];
                canvas.A[i] = canvasargb.Data[tmpIndex + 3];
                tmpIndex += 4;
            }

            return canvas;
        }
    }
}
