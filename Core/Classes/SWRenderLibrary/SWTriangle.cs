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
            short x1 = p1.X;
            short y1 = p1.Y;
            short x2 = p2.X;
            short y2 = p2.Y;
            short x3 = p3.X;
            short y3 = p3.Y;

            //FillTriangleSimple(canvas, this._canvasWidth, x1, y1, x2, y2, x3, y3,color);
            FillTriangleMy(canvas, this._canvasWidth, x1, y1, x2, y2, x3, y3, color);

        }

        private static void swap<T>(ref T p1, ref T p2)
        {
            T tmp =  p1;
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

        private static void FillTriangleSimple(byte[] canvas, int canvasWidth, short x0, short y0, short x1, short y1, short x2, short y2, Color color)
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
                swap<short>(ref x1, ref x2);
                swap<short>(ref y1, ref y2);
            }
            if (y0 > y1)
            {
                swap<short>(ref x0, ref x1);
                swap<short>(ref y0, ref y1);
            }
            if (y1 > y2)
            {
                swap<short>(ref x1, ref x2);
                swap<short>(ref y1, ref y2);
            }

            double dx_far = Convert.ToDouble(x2 - x0) / (y2 - y0 + 1);
            double dx_upper = Convert.ToDouble(x1 - x0) / (y1 - y0 + 1);
            double dx_low = Convert.ToDouble(x2 - x1) / (y2 - y1 + 1);
            double xf = x0;
            double xt = x0 + dx_upper; // if y0 == y1, special case
            int ymax = (y2 > height - 1 ? height - 1 : y2);
            for (int y = y0; y <= ymax; y++)
            {
                if (y >= 0)
                {   int canvasY = y * width;
                    double xForMax = (xt < width ? xt : width - 1);
                    for (int x = (xf > 0 ? (int)xf : 0); x <= xForMax; x++)
                    {
                        int index = (x + canvasY) * 4;
                        //ApplyColor(canvas, index, color);
                        ApplyColor(canvas, index, colorABRrem, colorRem);
                        ApplyColor(canvas, index+1, colorAGRrem, colorRem);
                        ApplyColor(canvas, index+2, colorARRrem, colorRem);

                    }

                    xForMax = (xt > 0 ? xt : 0);

                    for (int x = (xf < width ? (int)xf : width - 1); x >= xForMax; x--)
                    {
                        int index = (x + canvasY) * 4;
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

        private static void FillTriangleMy(byte[] canvas, int canvasWidth, short x0, short y0, short x1, short y1, short x2, short y2, Color color)
        {
            // key is Y, value is List x values min 1 max 2
            Dictionary<short,List<short>> rangePoints = new Dictionary<short, List<short>>();

            DrawLine(rangePoints, canvasWidth, x0,y0,x1,y1);
            DrawLine(rangePoints, canvasWidth, x1, y1, x2, y2);
            DrawLine(rangePoints, canvasWidth, x2, y2, x0, y0);

            int colorRem = GetREM(color.A);
            int colorABRrem = GetAXREM(color.A, color.B);
            int colorARRrem = GetAXREM(color.A, color.R);
            int colorAGRrem = GetAXREM(color.A, color.G);

            foreach (var item in rangePoints)
            {
                int y = item.Key;
                List<short> points = item.Value;
                if (points.Count == 1)
                {
                    int index = (y * canvasWidth + points[0]) * 4;
                    ApplyColor(canvas, index, colorABRrem, colorRem);
                    ApplyColor(canvas, index + 1, colorAGRrem, colorRem);
                    ApplyColor(canvas, index + 2, colorARRrem, colorRem);
                }
                else
                {
                    int index = (y * canvasWidth + points[0]) * 4;
                    int endPoint = points[1] ;
                    for (int i = points[0]; i <= endPoint ; i++)
                    {

                        ApplyColor(canvas, index, colorABRrem, colorRem);
                        ApplyColor(canvas, index + 1, colorAGRrem, colorRem);
                        ApplyColor(canvas, index + 2, colorARRrem, colorRem);

                        index += 4;
                    }
                }
            }
        
        }

        public static void DrawLine(Dictionary<short,List<short>> rangePoints, int width, 
            short x1, short y1, short x2, short y2)
        {
            //if (x1 > x2)
            //{
            //    short tmp = x1; x1 = x2; x2 = tmp;
            //    tmp = y1; y1 = y2; y2 = tmp;

            //}


            int x = x1;
            int y = y1;
            int w = x2 - x1;
            int h = y2 - y1;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            for (int i=0; i <= longest; i++)
            {
                #region set pixel
                short tmpY = (short)y;
                short tmpX = (short)x;

                if (!rangePoints.ContainsKey(tmpY))
                {
                    List<short> points = new List<short>();
                    points.Add(tmpX);
                    rangePoints.Add(tmpY, points);
                }
                else
                {
                    List<short> points = rangePoints[tmpY];
                    short startP = points[0];
                    

                    if (points.Count == 1)
                    {
                        if (startP != tmpX)
                        {
                            if (startP < tmpX) points.Add(tmpX);
                            else points.Insert(0, tmpX);
                        }

                    }
                    else
                    { // in list are 2 points, min and max point range
                      // now update theese points
                        short endP = points[1];
                        if (startP != tmpX && endP != tmpX)
                        {
                            if (startP > tmpX) points[0] = tmpX;
                            if (endP < tmpX) points[1] = tmpX;
                        }
                    }
                }


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


    }
}
