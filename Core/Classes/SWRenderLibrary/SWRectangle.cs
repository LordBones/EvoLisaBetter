using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.Core.AST;
using GenArtCoreNative;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public class SWRectangle
    {
        private static NativeFunctions nativeFunc = new NativeFunctions();
        private short [] rangePoints  = new short[0];

        public SWRectangle()
        {
        }


        public void Render(DnaRectangle rectangle, CanvasBGRA canvas)
        {
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
                throw new Exception("Toto nesmi nastat");

            FillRectangle(canvas, rectangle.StartPoint.X, rectangle.StartPoint.Y,
               rectangle.Width, rectangle.Height, rectangle.Brush.BrushColor);
        }

        private void FillRectangle(CanvasBGRA canvas, int x, int y, int widht, int height, Color color)
        {
            int rowStartIndex = y * canvas.Width;

            //int indexY = minY * this._canvasWidth;
            for (int iy  = 0; iy < height; iy++)//, indexY += this._canvasWidth)
            {
                int index = rowStartIndex + x * 4;
                int endIndex = rowStartIndex + (x + widht - 1) * 4;


                //nativeFunc.RowApplyColorSSE64(canvas.Data, index, endIndex, color.R, color.G, color.B, color.A);
                //RowApplyColorSafe(canvas.Data, index, endIndex, color.R, color.G, color.B, color.A);

                rowStartIndex += canvas.Width;
            }
        }

        private void RowApplyColorSafe(byte[] data, int startIndex, int endIndex, int r, int g, int b, int alpha)
        {
            alpha = (alpha * 256) / 255;

            int invAlpha = 256 - alpha;



            while (startIndex < endIndex)
            {
                int tb = data[startIndex];
                int tg = data[startIndex + 1];
                int tr = data[startIndex + 2];


                tb = (b * alpha + (tb * invAlpha)) >> 8;
                tg = (g * alpha + (tg * invAlpha)) >> 8;
                tr = (r * alpha + (tr * invAlpha)) >> 8;

                /*tb = tb + (((b-tb)*alpha)>>8);
                tg=tg + (((g-tg)*alpha)>>8);
                tr=tr + (((r-tr)*alpha)>>8);*/

                data[startIndex] = (byte)tb;
                data[startIndex + 1] = (byte)tg;
                data[startIndex + 2] = (byte)tr;



                startIndex += 4;
            }

        }
    }
}
