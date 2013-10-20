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
    public class SWElipse
    {
        private static NativeFunctions nativeFunc = new NativeFunctions();

        public void Render(DnaElipse elipse, CanvasBGRA canvas)
        {
            if (elipse.Width <= 0 || elipse.Height <= 0)
                throw new Exception("Toto nesmi nastat");

         


            //if((elipse.Height&1) == 0)
            // FillElipseEven(canvas, elipse.Middle.X, elipse.Middle.Y, elipse.Width, elipse.Height, elipse.Brush.BrushColor);
            //else
            //FillElipseOdd(canvas, elipse.Middle.X, elipse.Middle.Y, elipse.Width, elipse.Height, elipse.Brush.BrushColor);
            FillElipseOddFill(canvas, elipse.StartPoint.X, elipse.StartPoint.Y, elipse.Width, elipse.Height, elipse.Brush.BrushColor);

        }

        private void FillElipseOddFill(CanvasBGRA canvas, int x, int y, int width, int height, Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;
            int a = color.A;

            float hw = width / 2.0f;
            float hwSquare = hw * hw;

            int hh = (height - 1) / 2;
            float fhh = height / 2.0f;
            float fhhSquare = fhh * fhh;
            //TestPoint(canvas, x, y + hh);
            //TestPoint(canvas, x + width - 1, y + hh);

            int upY =  (y + hh) * canvas.Width;
            int upX1 = (x) * 4 + upY;
            int c = color.ToArgb();

            //nativeFunc.RowApplyColor(canvas.Data, upX1, width, r, g, b, a);
            nativeFunc.NewRowApplyColor64(canvas.Data, upX1, width, c, a);
            //if (upX1 + width <= canvas.Data.Length)
            //{
            //    int i = 0;
            //}
            //RowApplyColorSafe(canvas.Data, upX1, width, r, g, b, a);

            for (int dy = 1; dy <= hh; dy++)
            {
                double tmpx = Math.Sqrt(hwSquare * (1.0 - (dy * dy) / (fhhSquare)));
                  
                upY =  (y + hh - dy)*canvas.Width;
                upX1 = ((int)(x + hw - tmpx)) * 4 + upY;
                int count = (int)(hw + tmpx);

                //nativeFunc.RowApplyColor(canvas.Data, upX1, count,r,g,b,a);
                nativeFunc.NewRowApplyColor64(canvas.Data, upX1, count, c, a);
                //RowApplyColorSafe(canvas.Data, upX1, count, r, g, b, a);

                upY =  (y + hh + dy) * canvas.Width;
                upX1 = ((int)(x + hw - tmpx)) * 4 + upY;
             

                //nativeFunc.NewRowApplyColor64(canvas.Data, upX1, upX2, color.ToArgb(), a);
                //nativeFunc.RowApplyColor(canvas.Data, upX1, count, r, g, b, a);
                nativeFunc.NewRowApplyColor64(canvas.Data, upX1, count, c, a);
                //RowApplyColorSafe(canvas.Data, upX1, count, r, g, b, a);

            }
        }

        private void FillElipseOdd(CanvasBGRA canvas, int x, int y, int width, int height, Color color)
        {
            float hw = width / 2.0f;
            int hh = (height-1) / 2;
            float fhh = height / 2.0f;

            TestPoint(canvas, x , y+hh);
            TestPoint(canvas, x + width-1, y+hh);

            for (int dy = 1; dy <= hh; dy++)
            {
                double tmpx = Math.Sqrt(hw * hw * (1.0 - (dy * dy) / ((float)fhh * fhh)));

                TestPoint(canvas, (int)(x + hw - tmpx), y+hh - dy);
                TestPoint(canvas, (int)(x + hw + tmpx), y + hh - dy);
                TestPoint(canvas, (int)(x + hw - tmpx), y + hh + dy);
                TestPoint(canvas, (int)(x + hw + tmpx), y + hh + dy);

            }
        }

        private void FillElipseEven(CanvasBGRA canvas, int x, int y, int width, int height, Color color)
        {
            float hw = width / 2.0f;
            float hh = height / 2.0f;


            
            int highMax = (int)hh;
            for (int dy = 0; dy < highMax; dy++)
            {
                int tmpx = (int)Math.Ceiling(Math.Sqrt(hw * hw * (1.0 - (dy * dy) / ((float)hh * hh))));

                TestPoint(canvas, (int)(x+hw - tmpx), (int)(y+hh-dy-0.5));
                TestPoint(canvas, (int)(x+hw + tmpx), (int)(y+hh-dy-0.5));
                TestPoint(canvas, (int)(x+hw - tmpx), (int)(y + hh + dy + 0.5));
                TestPoint(canvas, (int)(x+hw + tmpx), (int)(y + hh + dy + 0.5));

            }
        }

        //        surface.set(x0, y0+b, c)
        //surface.set(x0, y0-b, c)
        //surface.set(x0+a, y0, c)
        //surface.set(x0-a, y0, c)

        //y = 0
        //for (x in range(1, a):
        //    y = int(math.sqrt(b*b * (1.0 - float(x*x)/(float(a*a))))
        //    if -b * x / (a * math.sqrt(a*a - x*x)) < -1:
        //        break

        //    surface.set(x0+x, y0+y, c)
        //    surface.set(x0-x, y0+y, c)
        //    surface.set(x0+x, y0-y, c)
        //    surface.set(x0-x, y0-y, c)

        //for dy in range(1, y+1):
        //    dx = int(math.sqrt(a*a*(1.0 - float(dy*dy)/float(b*b))))
        //    surface.set(x0+dx, y0+dy, c)
        //    surface.set(x0-dx, y0+dy, c)
        //    surface.set(x0+dx, y0-dy, c)
        //    surface.set(x0-dx, y0-dy, c)
        //    }
        //}

        private void TestPoint(CanvasBGRA canvas, int x, int y)
        {
            int index = canvas.Width * y + x * CanvasBGRA.CONST_PixelSize;

            canvas.Data[index] = 255;
            canvas.Data[index+1] = 0;
            canvas.Data[index+2] = 0;

        }

        private void RowApplyColorSafe(byte[] data, int startIndex, int countPixel, int r, int g, int b, int alpha)
        {
            alpha = (alpha * 256) / 255;

            int invAlpha = 256 - alpha;

            int cb = b * alpha;
            int cg = g * alpha;
            int cr = r * alpha;

            while (countPixel > 0)
            {
                int tb = data[startIndex];
                int tg = data[startIndex + 1];
                int tr = data[startIndex + 2];


                tb = (cb + (tb * invAlpha)) >> 8;
                tg = (cg + (tg * invAlpha)) >> 8;
                tr = (cr + (tr * invAlpha)) >> 8;

                /*tb = tb + (((b-tb)*alpha)>>8);
                tg=tg + (((g-tg)*alpha)>>8);
                tr=tr + (((r-tr)*alpha)>>8);*/

                data[startIndex] = (byte)tb;
                data[startIndex + 1] = (byte)tg;
                data[startIndex + 2] = (byte)tr;



                startIndex += 4;
                countPixel--;
            }

        }
    }
}
