using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public class SWTriangle
    {
        private int _canvasWidth, _canvasHeight;

        public SWTriangle(int canvasWidth, int canvasHeight)
        {
            this._canvasHeight = canvasHeight;
            this._canvasWidth = canvasWidth;
        }

        public void RenderTriangle(DnaPoint p1, DnaPoint p2, DnaPoint p3, byte[] canvas, Color color)
        {
            FillTriangleSimple(canvas, this._canvasWidth, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y,color);
        }

        private static void swap(ref int p1, ref int p2)
        {
            int tmp =  p1;
            p1 = p2;
            p2 = tmp;
        }

        private static int GetREM(int alpha)
        {
            return 0x10000 - (0x10000 * alpha / 255);
        }


        private static int GetAXREM(int alpha, int colorChanel)
        {
            return (0x10000 * alpha / 255) * colorChanel;
        }

        private static void ApplyColor(byte[] canvas, int index, Color color)
        {
            int canvasIndex = index * 4;

            int aMult = 0x10000 * color.A / 255;
            int rem = 0x10000 - aMult;
            int arrem = aMult * color.R;
            int abrem = aMult * color.B;
            int agrem = aMult * color.G;

            canvas[canvasIndex] = (byte)((abrem + rem * canvas[canvasIndex]) >> 16);
            canvas[canvasIndex + 1] = (byte)((agrem + rem * canvas[canvasIndex + 1]) >> 16);
            canvas[canvasIndex + 2] = (byte)((arrem + rem * canvas[canvasIndex + 2]) >> 16);
        }

        private static void ApplyColor(byte[] canvas, int index, int axrem, int rem)
        {
            canvas[index] = (byte)((axrem + rem * canvas[index]) >> 16);
        }

        private static void FillTriangleSimple(byte[] canvas, int canvasWidth, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            int colorRem = GetREM(color.A);
            int colorABRrem = GetAXREM(color.A,color.B);
            int colorARRrem = GetAXREM(color.A,color.R);
            int colorAGRrem = GetAXREM(color.A,color.G);


            int width = canvasWidth;
            int height = canvas.Length / canvasWidth;
            // sort the points vertically
            if (y1 > y2)
            {
                swap(ref x1, ref x2);
                swap(ref y1, ref y2);
            }
            if (y0 > y1)
            {
                swap(ref x0, ref x1);
                swap(ref y0, ref y1);
            }
            if (y1 > y2)
            {
                swap(ref x1, ref x2);
                swap(ref y1, ref y2);
            }

            double dx_far = Convert.ToDouble(x2 - x0) / (y2 - y0 + 1);
            double dx_upper = Convert.ToDouble(x1 - x0) / (y1 - y0 + 1);
            double dx_low = Convert.ToDouble(x2 - x1) / (y2 - y1 + 1);
            double xf = x0;
            double xt = x0 + dx_upper; // if y0 == y1, special case
            for (int y = y0; y <= (y2 > height - 1 ? height - 1 : y2); y++)
            {
                if (y >= 0)
                {
                    for (int x = (xf > 0 ? Convert.ToInt32(xf) : 0); x <= (xt < width ? xt : width - 1); x++)
                    {
                        int index = Convert.ToInt32(x + y * width)*4;
                        //ApplyColor(canvas, index, color);
                        ApplyColor(canvas, index, colorABRrem, colorRem);
                        ApplyColor(canvas, index+1, colorAGRrem, colorRem);
                        ApplyColor(canvas, index+2, colorARRrem, colorRem);

                    }
                    for (int x = (xf < width ? Convert.ToInt32(xf) : width - 1); x >= (xt > 0 ? xt : 0); x--)
                    {
                        int index = Convert.ToInt32(x + y * width)*4;
                        //ApplyColor(canvas, index, color);
                        ApplyColor(canvas, index, colorABRrem, colorRem);
                        ApplyColor(canvas, index + 1, colorAGRrem, colorRem);
                        ApplyColor(canvas, index + 2, colorARRrem, colorRem);

                    }
                }
                xf += dx_far;
                if (y < y1)
                    xt += dx_upper;
                else
                    xt += dx_low;
            }
        }
    }
}
