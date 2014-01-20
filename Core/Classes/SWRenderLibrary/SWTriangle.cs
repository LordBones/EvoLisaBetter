using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;
using GenArtCoreNative;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public class SWTriangle
    {

        private static NativeFunctions nativeFunc = new NativeFunctions();
        private short [] rangePoints  = new short[0];

        public SWTriangle()
        {
        }


        public void RenderTriangle(DnaPoint[] points, CanvasBGRA canvas, Color color)
        {
            short x1 = points[0].X;
            short y1 = points[0].Y;
            short x2 = points[1].X;
            short y2 = points[1].Y;
            short x3 = points[2].X;
            short y3 = points[2].Y;

            //FillTriangleSimple(canvas.Data, canvas.WidthPixel, x1, y1, x2, y2, x3, y3,color);
            //FillTriangleMy(canvas, x1, y1, x2, y2, x3, y3, color);
            //FillTriangleMyBetter2(canvas, x1, y1, x2, y2, x3, y3, color);
            //FillTriangleTile(canvas, x1, y1, x2, y2, x3, y3, color);
            FillTriangleTilePokus(canvas, x1, y1, x2, y2, x3, y3, color);


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
            // convert alpha value from range 0-255 to 0-256
            int alpha = (color.R * 256) / 255;

            int invAlpha = 256 - alpha;

            int b = color.B * alpha;
            int g = color.G * alpha;
            int r = color.R * alpha;

            int tb = canvas[index];
            int tg = canvas[index + 1];
            int tr = canvas[index + 2];


            canvas[index] = (byte)((b + (tb * invAlpha)) >> 8);
            canvas[index + 1] = (byte)((g + (tg * invAlpha)) >> 8);
            canvas[index + 2] = (byte)((r + (tr * invAlpha)) >> 8);


        }

        private static void FillTriangleTilePokus(CanvasBGRA canvas, short px0, short py0, short px1, short py1, short px2, short py2, Color color)
        {
            nativeFunc.RenderTrianglePokus(canvas.Data, canvas.Width, canvas.HeightPixel,
                px0, py0, px1, py1, px2, py2, color.ToArgb(), color.A);

            return;

            int alpha = (color.A * 256) / 255;

            /*int invAlpha = 256 - alpha;

            int b = color.B * alpha;
            int g = color.G * alpha;
            int r = color.R * alpha;*/

            int rgba = color.ToArgb();

            int v0x,v1x,v2x,v0y,v1y,v2y;

            v0x = px1 - px0;
            v0y = py1 - py0;

            v1x = px2 - px1;
            v1y = py2 - py1;

            v2x = px0 - px2;
            v2y = py0 - py2;

            // process all points
            int rowIndex = 0;

            int sz1 =  (0 - px0) * v0y - (0 - py0) * v0x;
            int sz2 =  (0 - px1) * v1y - (0 - py1) * v1x;
            int sz3 =  (0 - px2) * v2y - (0 - py2) * v2x;
            for (int y = 0; y < canvas.HeightPixel; y++)
            {
                int z1 = sz1;
                int z2 = sz2;
                int z3 = sz3;


                int currIndex = rowIndex;

                int x  = 0;
                int start = -1;
                
                while (x < canvas.WidthPixel)
                {
                    if ((z1 * z2 > 0) && (z1 * z3 > 0))
                    {
                        start = currIndex;
                        break;
                    }

                    z1 += v0y;
                    z2 += v1y;
                    z3 += v2y;
                    currIndex += 4;
                    x++;
                }

                if (start >= 0)
                {
                    int tmpx = x;
                    
                    while (x < canvas.WidthPixel)
                    {
                        if (!((z1 * z2 > 0) && (z1 * z3 > 0)))
                        {
                            //end = currIndex;
                            break;
                        }

                        
                        z1 += v0y;
                        z2 += v1y;
                        z3 += v2y;
                        
                        x++;
                    }

                    //if (end < 0) throw new Exception();

                    nativeFunc.NewRowApplyColor64(canvas.Data, start, x-tmpx + 1, rgba, alpha);

                    /*for(int i = 0;i<=count;i++)
                    {
                        int tb = canvas.Data[start];
                        int tg = canvas.Data[start + 1];
                        int tr = canvas.Data[start + 2];


                        canvas.Data[start] = (byte)((b + (tb * invAlpha)) >> 8);
                        canvas.Data[start + 1] = (byte)((g + (tg * invAlpha)) >> 8);
                        canvas.Data[start + 2] = (byte)((r + (tr * invAlpha)) >> 8);

                        start += 4;

                    }*/

                    /*while (start <= end)
                    {
                        int tb = canvas.Data[start];
                        int tg = canvas.Data[start + 1];
                        int tr = canvas.Data[start + 2];


                        canvas.Data[start] = (byte)((b + (tb * invAlpha)) >> 8);
                        canvas.Data[start + 1] = (byte)((g + (tg * invAlpha)) >> 8);
                        canvas.Data[start + 2] = (byte)((r + (tr * invAlpha)) >> 8);

                        start += 4;

                    }*/

                    
                }

                sz1 -= v0x;
                sz2 -= v1x;
                sz3 -= v2x;

                rowIndex += canvas.Width;
            }
        }

        private static void FillTriangleTile(CanvasBGRA canvas, short px0, short py0, short px1, short py1, short px2, short py2, Color color)
        {
            nativeFunc.RenderTriangle(canvas.Data, canvas.Width, canvas.HeightPixel,
                px0, py0, px1, py1, px2, py2, color.ToArgb(), color.A);

            return;

            int alpha = (color.A * 256) / 255;

            int invAlpha = 256 - alpha;

            int b = color.B * alpha;
            int g = color.G * alpha;
            int r = color.R * alpha;

            int v0x,v1x,v2x,v0y,v1y,v2y;

            v0x = px1 - px0;
            v0y = py1 - py0;

            v1x = px2 - px1;
            v1y = py2 - py1;

            v2x = px0 - px2;
            v2y = py0 - py2;

            // process all points
            int rowIndex = 0;

            int sz1 =  (0 - px0) * v0y - (0 - py0) * v0x;
            int sz2 =  (0 - px1) * v1y - (0 - py1) * v1x;
            int sz3 =  (0 - px2) * v2y - (0 - py2) * v2x;
            for (int y = 0; y < canvas.HeightPixel; y++)
            {
                int z1 = sz1;
                int z2 = sz2;
                int z3 = sz3;
                

                int currIndex = rowIndex;
                for (int x = 0; x < canvas.WidthPixel; x++)
                {
                    //if((((z1^z2)|(z1^z3))>>31) == 0)


                    if ((z1 * z2 > 0) && (z1 * z3 > 0))
                    {
                        //int index = (canvas.Width * y) + x * 4;

                        //ApplyColor(canvas.Data, currIndex, color);

                        

                        int tb = canvas.Data[currIndex];
                        int tg = canvas.Data[currIndex + 1];
                        int tr = canvas.Data[currIndex + 2];


                        canvas.Data[currIndex] = (byte)((b + (tb * invAlpha)) >> 8);
                        canvas.Data[currIndex+1] = (byte)((g + (tg * invAlpha)) >> 8);
                        canvas.Data[currIndex+2] = (byte)((r + (tr * invAlpha)) >> 8);

                    }

                    z1 += v0y;
                    z2 += v1y;
                    z3 += v2y;
                    currIndex += 4;
                }

                sz1 -= v0x;
                sz2 -= v1x;
                sz3 -= v2x;

                rowIndex += canvas.Width;
            }
        }

        private static void FillTriangleTile2(CanvasBGRA canvas, short px0, short py0, short px1, short py1, short px2, short py2, Color color)
        {
            int v0x,v1x,v2x,v0y,v1y,v2y;

            v0x = px1 - px0;
            v0y = py1 - py0;

            v1x = px2 - px1;
            v1y = py2 - py1;

            v2x = px0 - px2;
            v2y = py0 - py2;

            // process all points
            int rowIndex = 0;
            for (int y = 0; y < canvas.HeightPixel; y++)
            {


                int currIndex = rowIndex;
                for (int x = 0; x < canvas.WidthPixel; x++)
                {
                    int z1 = (x - px0) * v0y - (y - py0) * v0x;
                    int z2 = (x - px1) * v1y - (y - py1) * v1x;
                    int z3 = (x - px2) * v2y - (y - py2) * v2x;

                    if ((z1 * z2 > 0) && (z1 * z3 > 0))
                    {
                        //int index = (canvas.Width * y) + x * 4;

                        ApplyColor(canvas.Data, currIndex, color);

                    }

                    currIndex += 4;
                }

                rowIndex += canvas.Width;
            }
        }

        private static void FillTriangleTile2(byte[] canvas, int canvasWidth, short px0, short py0, short px1, short py1, short px2, short py2, Color color)
        {
            // 28.4 fixed-point coordinates
            int Y1 = py0;
            int Y2 = py1;
            int Y3 = py2;

            int X1 = px0;
            int X2 = px1;
            int X3 = px2;

            // Deltas
            int DX12 = X1 - X2;
            int DX23 = X2 - X3;
            int DX31 = X3 - X1;

            int DY12 = Y1 - Y2;
            int DY23 = Y2 - Y3;
            int DY31 = Y3 - Y1;

            // Fixed-point deltas
            int FDX12 = DX12 << 4;
            int FDX23 = DX23 << 4;
            int FDX31 = DX31 << 4;

            int FDY12 = DY12 << 4;
            int FDY23 = DY23 << 4;
            int FDY31 = DY31 << 4;

            // Bounding rectangle
            int minx = (Math.Min(Math.Min(X1, X2), X3) + 0xF) >> 4;
            int maxx = (Math.Max(Math.Max(X1, X2), X3) + 0xF) >> 4;
            int miny = (Math.Min(Math.Min(Y1, Y2), Y3) + 0xF) >> 4;
            int maxy = (Math.Max(Math.Max(Y1, Y2), Y3) + 0xF) >> 4;

            // Block size, standard 8x8 (must be power of two)
            const int q = 8;

            // Start in corner of 8x8 block
            minx &= ~(q - 1);
            miny &= ~(q - 1);

            int canvasIndex = miny * canvasWidth;

            // Half-edge constants
            int C1 = DY12 * X1 - DX12 * Y1;
            int C2 = DY23 * X2 - DX23 * Y2;
            int C3 = DY31 * X3 - DX31 * Y3;

            // Correct for fill convention
            if (DY12 < 0 || (DY12 == 0 && DX12 > 0)) C1++;
            if (DY23 < 0 || (DY23 == 0 && DX23 > 0)) C2++;
            if (DY31 < 0 || (DY31 == 0 && DX31 > 0)) C3++;

            // Loop through blocks
            for (int y = miny; y < maxy; y += q)
            {
                for (int x = minx; x < maxx; x += q)
                {
                    // Corners of block
                    int x0 = x << 4;
                    int x1 = (x + q - 1) << 4;
                    int y0 = y << 4;
                    int y1 = (y + q - 1) << 4;

                    // Evaluate half-space functions
                    bool a00 = C1 + DX12 * y0 - DY12 * x0 > 0;
                    bool a10 = C1 + DX12 * y0 - DY12 * x1 > 0;
                    bool a01 = C1 + DX12 * y1 - DY12 * x0 > 0;
                    bool a11 = C1 + DX12 * y1 - DY12 * x1 > 0;
                    bool a = a00 & a01 & a10 & a11;
                    //int a = (a00 << 0) | (a10 << 1) | (a01 << 2) | (a11 << 3);

                    bool b00 = C2 + DX23 * y0 - DY23 * x0 > 0;
                    bool b10 = C2 + DX23 * y0 - DY23 * x1 > 0;
                    bool b01 = C2 + DX23 * y1 - DY23 * x0 > 0;
                    bool b11 = C2 + DX23 * y1 - DY23 * x1 > 0;
                    //int b = (b00 << 0) | (b10 << 1) | (b01 << 2) | (b11 << 3);
                    bool b = b00 & b01 & b10 & b11;

                    bool c00 = C3 + DX31 * y0 - DY31 * x0 > 0;
                    bool c10 = C3 + DX31 * y0 - DY31 * x1 > 0;
                    bool c01 = C3 + DX31 * y1 - DY31 * x0 > 0;
                    bool c11 = C3 + DX31 * y1 - DY31 * x1 > 0;
                    //int c = (c00 << 0) | (c10 << 1) | (c01 << 2) | (c11 << 3);
                    bool c = c00 & c01 & c10 & c11;

                    // Skip block when outside an edge
                    //if(a == 0x0 || b == 0x0 || c == 0x0) continue;
                    if (!a || !b || !c) continue;

                    int tmpCanvasIndex = canvasIndex;

                    // Accept whole block when totally covered
                    //if(a == 0xF && b == 0xF && c == 0xF)
                    if (a && b && c)
                    {
                        for (int iy = 0; iy < q; iy++)
                        {
                            for (int ix = x; ix < x + q; ix++)
                            {
                                canvas[tmpCanvasIndex + ix * 4] = 128; // Green
                                canvas[tmpCanvasIndex + ix * 4 + 1] = 0xFF; // Green
                                canvas[tmpCanvasIndex + ix * 4 + 2] = 0xFF; // Green
                                canvas[tmpCanvasIndex + ix * 4 + 3] = 128; // Green
                            }

                            tmpCanvasIndex += canvasWidth;
                        }
                    }
                    else // Partially covered block
                    {
                        int CY1 = C1 + DX12 * y0 - DY12 * x0;
                        int CY2 = C2 + DX23 * y0 - DY23 * x0;
                        int CY3 = C3 + DX31 * y0 - DY31 * x0;

                        for (int iy = y; iy < y + q; iy++)
                        {
                            int CX1 = CY1;
                            int CX2 = CY2;
                            int CX3 = CY3;

                            for (int ix = x; ix < x + q; ix++)
                            {
                                if (CX1 > 0 && CX2 > 0 && CX3 > 0)
                                {
                                    canvas[tmpCanvasIndex + ix * 4] = 128; // Green
                                    canvas[tmpCanvasIndex + ix * 4 + 1] = 0xFF; // Green
                                    canvas[tmpCanvasIndex + ix * 4 + 2] = 0xFF; // Green
                                    canvas[tmpCanvasIndex + ix * 4 + 2] = 128;
                                }

                                CX1 -= FDY12;
                                CX2 -= FDY23;
                                CX3 -= FDY31;
                            }

                            CY1 += FDX12;
                            CY2 += FDX23;
                            CY3 += FDX31;

                            tmpCanvasIndex += canvasWidth;
                        }
                    }
                }

                canvasIndex += q * canvasWidth;
            }
        }

        private static void FillTriangleSimple(byte[] canvas, int canvasWidth, short x0, short y0, short x1, short y1, short x2, short y2, Color color)
        {
            int colorRem = GetREM(color.A);
            int colorABRrem = GetAXREM(color.A, color.B);
            int colorARRrem = GetAXREM(color.A, color.R);
            int colorAGRrem = GetAXREM(color.A, color.G);


            int width = canvasWidth;
            int height = canvas.Length / canvasWidth;
            // sort the points vertically
            if (y1 > y2)
            {
                Tools.swap<short>(ref x1, ref x2);
                Tools.swap<short>(ref y1, ref y2);
            }
            if (y0 > y1)
            {
                Tools.swap<short>(ref x0, ref x1);
                Tools.swap<short>(ref y0, ref y1);
            }
            if (y1 > y2)
            {
                Tools.swap<short>(ref x1, ref x2);
                Tools.swap<short>(ref y1, ref y2);
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
                {
                    int canvasY = y * width;
                    //int xfi = (int)xf;
                    double xForMax = (xt < width ? xt : width - 1);
                    for (int x = (xf > 0 ? (int)xf : 0); x <= xForMax; x++)
                    {
                        int index = (x + canvasY) * 4;
                        //ApplyColor(canvas, index, color);
                        ApplyColor(canvas, index, color);
                    
                    }

                    xForMax = (xt > 0 ? xt : 0);

                    int xx = (xf < width ? (int)xf : width - 1);
                    int index2 = ((int)xForMax + canvasY) * 4;
                    int endIndex = ((int)xx + canvasY) * 4;

                    //nativeFunc.RowApplyColor(canvas, index2, endIndex, colorABRrem, colorAGRrem, colorARRrem, colorRem);

                    /*for (int x = (xf < width ? (int)xf : width -1); x >= xForMax; x--)
                    {
                        int index = (x + canvasY) * 4;
                        //ApplyColor(canvas, index, color);
                        ApplyColor(canvas, index, colorABRrem, colorRem);
                        ApplyColor(canvas, index + 1, colorAGRrem, colorRem);
                        ApplyColor(canvas, index + 2, colorARRrem, colorRem);

                    }*/
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

            DrawLine(rangePoints, minY, x0, y0, x1, y1);
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
            for (int y  = 0; y < rangePoints.Length; y++)//, indexY += this._canvasWidth)
            {

                pointRange points = rangePoints[y];

                //if (points == null) continue;



                int index = rowStartIndex + points.Start * 4;
                int endIndex = rowStartIndex + points.End * 4;

                rangePoints[y] = null;
                //int endPoint = points.End;




                //if (points.End - points.Start + 1 > 2)
                {
                    //nativeFunc.RowApplyColor(canvas, index, endIndex, colorABRrem, colorAGRrem, colorARRrem, colorRem);
                }
                //else
                {
                    //while (index <= endIndex)
                    //{

                    //    canvas[index] = ApplyColor(canvas[index], colorABRrem, colorRem);
                    //    canvas[index + 1] = ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    //    canvas[index + 2] = ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                    //    index += 4;
                    //}
                }


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

            int longest = Tools.fastAbs(w);
            int shortest = Tools.fastAbs(h);
            if (!(longest > shortest))
            {
                Tools.swap<int>(ref longest, ref shortest);

                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            for (int i=0; i <= longest; i++)
            {
                #region set pixel
                short tmpY = (short)(y - minY);
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


        private void FillTriangleMyBetter(CanvasBGRA drawCanvas, short x0, short y0, short x1, short y1, short x2, short y2, Color color)
        {


            short minY = y0;
            short maxY = y0;

            if (minY > y1) minY = y1;
            if (minY > y2) minY = y2;
            if (maxY < y1) maxY = y1;
            if (maxY < y2) maxY = y2;

            int triangleHigh = maxY - minY + 1;

            // key is Y, value is List x values min 1 max 2
            short [] rangePoints = new short[triangleHigh * 2];

            for (int index=0; index < rangePoints.Length; index += 2)
                rangePoints[index] = -1;

            DrawLineBetter(rangePoints, minY, x0, y0, x1, y1);
            DrawLineBetter(rangePoints, minY, x1, y1, x2, y2);
            DrawLineBetter(rangePoints, minY, x2, y2, x0, y0);

            byte [] canvas = drawCanvas.Data;


            nativeFunc.RowApplyColorBetter(canvas, drawCanvas.Width, rangePoints, minY, color.R, color.G, color.B, color.A);

        }

        private void InitRangePoints(CanvasBGRA drawCanvas)
        {
            if (drawCanvas.HeightPixel != (rangePoints.Length / 2))
            {
                rangePoints = new short[drawCanvas.HeightPixel * 2];
            }

            for (int index=0; index < rangePoints.Length; index += 2)
                rangePoints[index] = -1;
        }

        private void FillTriangleMyBetter2(CanvasBGRA drawCanvas, short x0, short y0, short x1, short y1, short x2, short y2, Color color)
        {
            InitRangePoints(drawCanvas);

            short minY = y0;
            short maxY = y0;

            if (minY > y1) minY = y1;
            if (minY > y2) minY = y2;
            if (maxY < y1) maxY = y1;
            if (maxY < y2) maxY = y2;



            DrawLineBetter(rangePoints, x0, y0, x1, y1);
            DrawLineBetter(rangePoints, x1, y1, x2, y2);
            DrawLineBetter(rangePoints, x2, y2, x0, y0);

            byte [] canvas = drawCanvas.Data;

            //nativeFunc.RenderTriangleByRanges(canvas, drawCanvas.Width, rangePoints, minY, maxY, color.ToArgb(), color.A);
            //nativeFunc.RowApplyColorBetter(canvas, drawCanvas.Width, rangePoints, minY, maxY, color.R, color.G, color.B, color.A);

            //FastRowsApplyColor(canvas, drawCanvas.Width, rangePoints, minY, maxY, color.R, color.G, color.B, color.A);
            FastRowsApplyColorNative(canvas, drawCanvas.Width, rangePoints, minY, maxY, color.R, color.G, color.B, color.A);
        }

        void FastRowsApplyColorNative(byte[] canvas, int canvasWidth, short[] ranges, int rangeStartY, int rangeEndY, int r, int g, int b, int alpha)
        {
            //if(alpha> 0) alpha++;
            // convert alpha value from range 0-255 to 0-256

            int color = Color.FromArgb(r, g, b).ToArgb();

            int rowStartIndex = (rangeStartY) * canvasWidth;
            rangeStartY *= 2;
            rangeEndY *= 2;
            for (int i = rangeStartY; i < rangeEndY; i += 2)
            {
                int index = rowStartIndex + (ranges[i]) * 4;

                // 
                int count = ranges[i + 1] - ranges[i] + 1;

                //nativeFunc.RowApplyColorSSE64(canvas,  r,g,b, alpha);

                //if(count > 100)
                //nativeFunc.NewRowApplyColor128(canvas, index, count, color, alpha);
                //else
                nativeFunc.NewRowApplyColor64(canvas, index, count, color, alpha);

                //nativeFunc.NewRowApplyColor64(canvas, index, end, color, alpha);



                rowStartIndex += canvasWidth;
            }
        }

        void FastRowsApplyColor(byte[] canvas, int canvasWidth, short[] ranges, int rangeStartY, int rangeEndY, int r, int g, int b, int alpha)
        {
            //if(alpha> 0) alpha++;
            // convert alpha value from range 0-255 to 0-256
            alpha = (alpha * 256) / 255;



            int invAlpha = 256 - alpha;
            int rowStartIndex = (rangeStartY) * canvasWidth;

            int cb = b * alpha;
            int cg = g * alpha;
            int cr = r * alpha;


            for (int i = rangeStartY * 2; i < rangeEndY * 2; i += 2)
            {
                int index = rowStartIndex + (ranges[i]) * 4;

                //
                int end = rowStartIndex + (ranges[i + 1]) * 4;

                while (end >= index)
                {
                    int tb = canvas[index];
                    int tg = canvas[index + 1];
                    int tr = canvas[index + 2];


                    tb = (cb + (tb * invAlpha)) >> 8;
                    tg = (cg + (tg * invAlpha)) >> 8;
                    tr = (cr + (tr * invAlpha)) >> 8;

                    /*tb = tb + (((b-tb)*alpha)>>8);
                    tg=tg + (((g-tg)*alpha)>>8);
                    tr=tr + (((r-tr)*alpha)>>8);*/

                    canvas[index] = (byte)tb;
                    canvas[index + 1] = (byte)tg;
                    canvas[index + 2] = (byte)tr;


                    index += 4;
                }

                rowStartIndex += canvasWidth;
            }
        }

        private static void DrawLineBetter(short[] rangePoints, short minY,
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

            int longest = Tools.fastAbs(w);
            int shortest = Tools.fastAbs(h);
            if (!(longest > shortest))
            {
                Tools.swap<int>(ref longest, ref shortest);

                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            for (int i=0; i <= longest; i++)
            {
                #region set pixel
                short tmpY = (short)((y - minY) * 2);
                short tmpX = (short)x;

                if (rangePoints[tmpY] < 0)
                {
                    rangePoints[tmpY] = tmpX;
                    rangePoints[tmpY + 1] = tmpX;
                }
                else
                {

                    if (rangePoints[tmpY] > tmpX) rangePoints[tmpY] = tmpX;
                    else if (rangePoints[tmpY + 1] < tmpX) rangePoints[tmpY + 1] = tmpX;

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

        private static void DrawLineBetter(short[] rangePoints,
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

            int longest = Tools.fastAbs(w);
            int shortest = Tools.fastAbs(h);
            if (!(longest > shortest))
            {
                Tools.swap<int>(ref longest, ref shortest);

                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            for (int i=0; i <= longest; i++)
            {
                #region set pixel
                short tmpY = (short)(y * 2);
                short tmpX = (short)x;

                if (rangePoints[tmpY] < 0)
                {
                    rangePoints[tmpY] = tmpX;
                    rangePoints[tmpY + 1] = tmpX;
                }
                else
                {

                    if (rangePoints[tmpY] > tmpX) rangePoints[tmpY] = tmpX;
                    else if (rangePoints[tmpY + 1] < tmpX) rangePoints[tmpY + 1] = tmpX;

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
