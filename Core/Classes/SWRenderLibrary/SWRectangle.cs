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


        public void Render(DnaRectangle rectangle, CanvasARGB canvas)
        {
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
                throw new Exception("Toto nesmi nastat");


            //FillRectangle(canvas, rectangle.StartPoint.X, rectangle.StartPoint.Y,
            //   rectangle.Width, rectangle.Height, rectangle.Brush.BrushColor);

            nativeFunc.RenderRectangle(canvas.Data, canvas.Width,
               rectangle.StartPoint.X, rectangle.StartPoint.Y,
               rectangle.Width, rectangle.Height,
               (int)rectangle.Brush.ColorAsUInt,
               rectangle.Brush.Alpha);
        }

        public void RenderRow(int y, int startIndexRow, int color, DnaRectangle rectangle, CanvasARGB canvas)
        {
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
                throw new Exception("Toto nesmi nastat");

            // radek neprotina obdelnik nic se nekresli
            if (y < rectangle.StartPoint.Y || rectangle.EndPoint.Y < y)
                return;
            //FillRectangle(canvas, rectangle.StartPoint.X, rectangle.StartPoint.Y,
            //   rectangle.Width, rectangle.Height, rectangle.Brush.BrushColor);

            FillRowRectangle(startIndexRow, rectangle, canvas, color);


            //nativeFunc.RenderRectangle(canvas.Data, canvas.Width,
            //   rectangle.StartPoint.X, rectangle.StartPoint.Y,
            //   rectangle.Width, rectangle.Height,
            //   rectangle.Brush.BrushColor.ToArgb(),
            //   rectangle.Brush.BrushColor.A);
        }

        private void FillRowRectangle(int startIndexRow, DnaRectangle rectangle, CanvasARGB canvas, int color)
        {
            int startIndex = startIndexRow + rectangle.StartPoint.X * 4;
            //nativeFunc.NewRowApplyColor128(canvas.Data, rowStartIndex, width, c, a);
            int alpha256 = ((((color) >> 24) & 0xff) * 256) / 255;

            nativeFunc.NewRowApplyColor64(canvas.Data, startIndex, rectangle.Width, color,alpha256);
            //RowApplyColorSafe(canvas.Data, startIndex, startIndex+(rectangle.Width-1)*4, color.R, color.G, color.B, color.A);

        }

        private void FillRectangle(CanvasARGB canvas, int x, int y, int width, int height, Color color)
        {
            int rowStartIndex = y * canvas.Width + x * 4;



           
            int c = color.ToArgb();

            //int indexY = minY * this._canvasWidth;
            for (int iy  = 0; iy < height; iy++)//, indexY += this._canvasWidth)
            {
                //int length = rowEndIndex - rowStartIndex + 1;
                //byte [] test = new byte[length];

                //Array.Copy(canvas.Data, rowStartIndex, test, 0, length);

                //nativeFunc.RowApplyColorSSE64(canvas.Data, rowStartIndex, rowEndIndex, r,g,b,a);
                //nativeFunc.NewRowApplyColor(canvas.Data, rowStartIndex, width, color.ToArgb(), a);
                nativeFunc.NewRowApplyColor128(canvas.Data, rowStartIndex, width, c);

                //nativeFunc.RowApplyColor(canvas.Data, rowStartIndex, width, color.R, color.G, color.B, color.A);
                //SWHelpers.RowApplyColorSafe(canvas.Data, rowStartIndex, rowEndIndex, color.R, color.G, color.B, color.A);



                //nativeFunc.RowApplyColor(test, 0, length-1, color.R, color.G, color.B, color.A);

                //bool equal = true;
                //for (int i = 0; i < test.Length; i++)
                //{
                //    if (test[i] != canvas.Data[rowStartIndex + i])
                //    {
                //        equal = false;
                //        throw new NotImplementedException();
                //        break;
                //    }
                //}

                rowStartIndex += canvas.Width;

            }
        }
    }
}
