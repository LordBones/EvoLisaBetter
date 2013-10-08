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

            FillElipse(canvas, elipse.Middle.X, elipse.Middle.Y, elipse.Width, elipse.Height, elipse.Brush.BrushColor);
        }

        private void FillElipse(CanvasBGRA canvas, int x, int y, int width, int height, Color color)
        {
            int a = width;
            int b = height;

            for (int dy = 1; dy < height; dy++)
            {
                int tmpx = (int)Math.Sqrt(height * height * (1.0 - dy * dy / (float)width * width));

                TestPoint(canvas, x - tmpx, dy);
                TestPoint(canvas, x + tmpx, dy);

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
    }
}
