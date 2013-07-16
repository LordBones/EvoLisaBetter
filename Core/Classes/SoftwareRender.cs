using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Core.Classes.SWRenderLibrary;

namespace GenArt.Core.Classes
{
    public class SoftwareRender
    {
        private Polygon _drawPolygon;
        private Polygon _drawPolygonCorrect;
        private SWTriangle _drawTriangle;

        // format ukladani barev do pole je bgra

        public SoftwareRender(int canvasWidth, int canvasHeight)
        {
            _drawTriangle = new SWTriangle(canvasWidth, canvasHeight);
            _drawPolygon = new Polygon(canvasWidth, canvasHeight);
            _drawPolygon.SetStartBufferSize(canvasWidth, canvasHeight);
            _drawPolygonCorrect = new Polygon(canvasWidth, canvasHeight);
            _drawPolygonCorrect.SetStartBufferSize(canvasWidth, canvasHeight);
        }

        public static void FastColorFill2(byte[] canvas, Color color)
        {
            unsafe
            {
                fixed (byte * tmpcurrentPtr = canvas)
                {
                    uint * currentPtr = (uint*)tmpcurrentPtr;
                    uint colorForFill = (uint)((color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R));


                    *currentPtr = colorForFill;
                    currentPtr += (canvas.Length >> 2) & 1;

                    //int totalLength = (Tools.MaxWidth * Tools.MaxHeight * 4);
                    uint * end = currentPtr + ((canvas.Length >> 2) & (~((uint)1)));


                    while (currentPtr < end)
                    {
                        *currentPtr = colorForFill;
                        currentPtr++;
                        *(currentPtr) = colorForFill;
                        currentPtr++;
                    }


                }
            }
        }

        public static void FastColorFill(byte[] canvas, Color color)
        {
            if (canvas.Length < 4)
                return;

            unsafe
            {
                fixed (byte * tmpcurrentPtr = canvas)
                {
                    int canvasPixelCount = canvas.Length >> 2;


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

        [DllImport("msvcrt")]
        public static extern IntPtr memset(int[] dest, int c, IntPtr count);

        // or this
        [DllImport("msvcrt")]
        public static unsafe extern void* memset(void* dest, int c, IntPtr count);







        //int[] x = new int[4];
        //memset(x, 0, new IntPtr(4)); // first version


        public static void FastColorFill4(byte[] canvas, Color color)
        {
            int colorForFill = (int)((color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R));

            unsafe
            {
                fixed (byte * x_ptr = canvas)
                {
                    int * ptr = (int*)x_ptr;

                    memset((void*)ptr, colorForFill, new IntPtr(4)); // second version
                }
            }

        }

        public static void DrawLine(byte[] canvas, int width, Point startP, Point endP, Color color)
        {
            int widthMull = width << 2;

            int x = startP.X;
            int y = startP.Y * widthMull;
            int w = endP.X - startP.X;
            int h = endP.Y - startP.Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -widthMull; else if (h > 0) dy1 = widthMull;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -widthMull; else if (h > 0) dy2 = widthMull;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            //int mullY = dy1 * widthMull;
            //int mully2 = dy2 * widthMull;
            //int compY = y * widthMull;

            byte alpha = color.A;
            byte red = color.R;
            byte blue = color.B;
            byte green = color.G;

            for (int i=0; i <= longest; i++)
            {
                #region set pixel


                int index = y + (x << 2);
                canvas[index] = SoftwareRenderLibrary.MixColorChanelWithAlpha(canvas[index], green, alpha);
                canvas[index + 1] = SoftwareRenderLibrary.MixColorChanelWithAlpha(canvas[index + 1], blue, alpha);
                canvas[index + 2] = SoftwareRenderLibrary.MixColorChanelWithAlpha(canvas[index + 2], red, alpha);


                //canvas[index ] = color.G;
                //canvas[index + 1] = color.B;
                //canvas[index+2] = color.R;

                //canvas[index+3] = 255;//color.R;
                //canvas[index + 3] = color.A;



                #endregion


                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                    //compY += mullY;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                    //compY += mully2;
                }
            }

        }

        public static void DrawLine(byte[] canvas, int width, Point[] points, Color color)
        {
            for (int index = 1; index < points.Length; index++)
            {
                DrawLine(canvas, width, points[index - 1], points[index], color);
            }

            DrawLine(canvas, width, points[points.Length - 1], points[0], color);
        }

        //Render a Drawing
        public void Render(DnaDrawing drawing, byte[] data, int width, int scale, Color background)
        {
            FastColorFill(data, Color.FromArgb(255, 0, 0, 0));

            
            DnaPolygon [] dnaPolygons = drawing.Polygons;
            int polyCount = dnaPolygons.Length;
            for (int i = 0; i < polyCount; i++)
            {
                DnaPolygon polygon = dnaPolygons[i];
               
                this._drawPolygon.FillPolygon(polygon.Points, data, polygon.Brush.BrushColor);

                //this._drawTriangle.RenderTriangle(polygon.Points[0],polygon.Points[1],polygon.Points[2],                 data, polygon.Brush.BrushColor);

                //this._drawPolygonCorrect.FillPolygon(polygon.Points, data, polygon.Brush.BrushColor);

                //DrawLine(data, width, points, Color.FromArgb(polygon.Brush.Alpha, polygon.Brush.Red, polygon.Brush.Green, polygon.Brush.Blue));
            }
        }

        //Render a Drawing
        public void RenderNative(DnaDrawing drawing, byte[] data, int width, int scale, Color background)
        {
            FastColorFill(data, Color.FromArgb(255, 0, 0, 0));


            DnaPolygon [] dnaPolygons = drawing.Polygons;
            int polyCount = dnaPolygons.Length;
            for (int i = 0; i < polyCount; i++)
            {
                DnaPolygon polygon = dnaPolygons[i];
                //Point [] points = GetGdiPoints(polygon.Points, 1);

                //Color color = Color.FromArgb(polygon.Brush.Alpha, polygon.Brush.Red, polygon.Brush.Green, polygon.Brush.Blue);
                //this._drawPolygon.FillPolygon(points, data, color);
                this._drawPolygon.FillPolygonNative(polygon.Points, data, polygon.Brush.BrushColor);
                //  this._drawPolygonCorrect.FillPolygon(polygon.Points, data, polygon.Brush.BrushColor);

                //DrawLine(data, width, points, Color.FromArgb(polygon.Brush.Alpha, polygon.Brush.Red, polygon.Brush.Green, polygon.Brush.Blue));
            }
        }


        //Convert a list of DnaPoint to a list of System.Drawing.Point's
        public static Point[] GetGdiPoints(IList<DnaPoint> points, int scale)
        {
            Point[] pts = new Point[points.Count];
            int i = 0;
            foreach (DnaPoint pt in points)
            {
                pts[i++] = new Point(pt.X * scale, pt.Y * scale);
            }
            return pts;
        }

        //Convert a DnaBrush to a System.Drawing.Brush
        private static Brush GetGdiBrush(DnaBrush b)
        {
            return new SolidBrush(Color.FromArgb(b.Alpha, b.Red, b.Green, b.Blue));
        }
    }
}
