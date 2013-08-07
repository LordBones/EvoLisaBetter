﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArtCoreNative;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public class SWTriangle
    {
        private int _canvasWidth, _canvasHeight;
        private Class1 nativeFunc = new Class1();

        public SWTriangle(int canvasWidth, int canvasHeight)
        {
            this._canvasHeight = canvasHeight;
            this._canvasWidth = canvasWidth;
        }


        public void RenderTriangle(DnaPoint [] points, CanvasBGRA canvas, Color color)
        {
            short x1 = points[0].X;
            short y1 = points[0].Y;
            short x2 = points[1].X;
            short y2 = points[1].Y;
            short x3 = points[2].X;
            short y3 = points[2].Y;

            //FillTriangleSimple(canvas, this._canvasWidth, x1, y1, x2, y2, x3, y3,color);
            FillTriangleMy(canvas, x1, y1, x2, y2, x3, y3, color);

        }

        private static void swap<T>(ref T p1, ref T p2)
        {
            T tmp =  p1;
            p1 = p2;
            p2 = tmp;
        }

        private static int GetREM(byte alpha)
        {
            return 0x10000 - (0x10000 * alpha / 255);
        }


        private static int GetAXREM(byte alpha, byte colorChanel)
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

        private static byte ApplyColor(int colorChanel, int axrem, int rem)
        {
            return (byte)((axrem + rem * colorChanel) >> 16);
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
                //int xfi = (int)xf;
                    double xForMax = (xt < width ? xt : width -1);
                    for (int x = (xf > 0 ? (int)xf : 0); x <= xForMax; x++)
                    {
                        int index = (x + canvasY) * 4;
                        //ApplyColor(canvas, index, color);
                        ApplyColor(canvas, index, colorABRrem, colorRem);
                        ApplyColor(canvas, index+1, colorAGRrem, colorRem);
                        ApplyColor(canvas, index+2, colorARRrem, colorRem);

                    }

                    xForMax = (xt > 0 ? xt : 0);

                    for (int x = (xf < width ? (int)xf : width -1); x >= xForMax; x--)
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

        private class pointRange
        {
            public short Start;
            public short End;

            public pointRange(short start, short end)
            {
                this.End = end;
                this.Start = start;
            }
        }

        private void FillTriangleMy(CanvasBGRA drawCanvas, short x0, short y0, short x1, short y1, short x2, short y2, Color color)
        {
            

            short minY = y0;
            short maxY = y0;

            if (minY > y1) minY = y1;
            if (minY > y2) minY = y2;
            if (maxY < y1) maxY = y1;
            if (maxY < y2) maxY = y2;

            int triangleHigh = maxY - minY + 1;

            // key is Y, value is List x values min 1 max 2
            pointRange [] rangePoints = new pointRange[triangleHigh];

            DrawLine(rangePoints, minY, x0,y0,x1,y1);
            DrawLine(rangePoints, minY, x1, y1, x2, y2);
            DrawLine(rangePoints, minY, x2, y2, x0, y0);

            byte colorA = color.A;
            int colorRem = GetREM(colorA);
            int colorABRrem = GetAXREM(colorA, color.B);
            int colorARRrem = GetAXREM(colorA, color.R);
            int colorAGRrem = GetAXREM(colorA, color.G);

            byte [] canvas = drawCanvas.Data;

            //ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = 1;
            /*Parallel.For(0, rangePoints.Length-1, y =>
            {
                pointRange points = rangePoints[y];

                //if (points == null) continue;


                int prowStartIndex = (minY+y) * drawCanvas.Width;
                int index = prowStartIndex + points.Start * 4;
                int endIndex = prowStartIndex + points.End * 4;

                rangePoints[y] = null;
                //int endPoint = points.End;

                while (index <= endIndex)
                {

                    canvas[index] = ApplyColor(canvas[index], colorABRrem, colorRem);
                    canvas[index + 1] = ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    canvas[index + 2] = ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                    index += 4;
                }

                
            });
            */
            int rowStartIndex = (minY) * drawCanvas.Width;
            
                
            //int indexY = minY * this._canvasWidth;
            for (int y  = 0; y < rangePoints.Length; y++ )//, indexY += this._canvasWidth)
            {

                pointRange points = rangePoints[y];

                //if (points == null) continue;



                int index = rowStartIndex + points.Start*4;
                int endIndex = rowStartIndex + points.End * 4;

                rangePoints[y] = null;
                //int endPoint = points.End;

                

           
                //if (points.End - points.Start + 1 > 2)
                {
                    nativeFunc.RowApplyColor(canvas, index, endIndex, colorABRrem, colorAGRrem, colorARRrem, colorRem);
                }
                //else
                /*{
                    while (index <= endIndex)
                    {

                        canvas[index] = ApplyColor(canvas[index], colorABRrem, colorRem);
                        canvas[index + 1] = ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                        canvas[index + 2] = ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                        index += 4;
                    }
                }*/


                //for (int i = points.Start; i <= endPoint; i++)
                //{

                //    canvas[index] = ApplyColor(canvas[index], colorABRrem, colorRem);
                //    canvas[index + 1] = ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                //    canvas[index + 2] = ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                //    index += 4;
                //}

                
                rowStartIndex += drawCanvas.Width;
            }
        
        }

        private static void DrawLine(pointRange[] rangePoints, short minY,
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
            if (w < 0) { dx1 = -1; dx2 = -1; } else if (w > 0) { dx1 = 1; dx2 = 1; }
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                swap<int>(ref longest, ref shortest);
                
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            for (int i=0; i <= longest; i++)
            {
                #region set pixel
                short tmpY = (short)(y-minY);
                short tmpX = (short)x;

                pointRange points = rangePoints[tmpY];

                if (points == null)
                {
                    rangePoints[tmpY] = new pointRange(tmpX, tmpX);
                }
                else
                {
                   
                    if (points.Start > tmpX) points.Start = tmpX;
                    else if (points.End < tmpX) points.End = tmpX;

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
