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
                int endIndex = rowStartIndex + (x+height-1) * 4;


                nativeFunc.RowApplyColor(canvas.Data, index, endIndex, color.R, color.G, color.B, color.A);

                rowStartIndex += canvas.Width;
            }
        }
    }
}
