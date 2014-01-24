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

            //FillTriangleMyBetter2(canvas, x1, y1, x2, y2, x3, y3, color);
            FillTrianglePokus(canvas, x1, y1, x2, y2, x3, y3, color);

        }

        public void RenderTriangleStrip(DnaPoint[] points, CanvasBGRA canvas, Color color)
        {
            for (int i = 0; i <= points.Length - 3; i++)
            {
                short x1 = points[i].X;
                short y1 = points[i].Y;
                short x2 = points[i+1].X;
                short y2 = points[i+1].Y;
                short x3 = points[i+2].X;
                short y3 = points[i+2].Y;

                //FillTriangleSimple(canvas.Data, canvas.WidthPixel, x1, y1, x2, y2, x3, y3,color);
                //FillTriangleMy(canvas, x1, y1, x2, y2, x3, y3, color);
                //FillTriangleMyBetter2(canvas, x1, y1, x2, y2, x3, y3, color);
                //FillTriangleTile(canvas, x1, y1, x2, y2, x3, y3, color);
                //FillTriangleTilePokus(canvas, x1, y1, x2, y2, x3, y3, color);
                FillTrianglePokus(canvas, x1, y1, x2, y2, x3, y3, color);
            }

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

        private static void FillTrianglePokus(CanvasBGRA canvas, short px0, short py0, short px1, short py1, short px2, short py2, Color color)
        {

            nativeFunc.RenderTriangleNew(canvas.Data, canvas.Width, canvas.HeightPixel,
                px0, py0, px1, py1, px2, py2, color.ToArgb(), color.A);

            return;

            int alpha = (color.A * 256) / 255;

            int invAlpha = 256 - alpha;

            int b = color.B * alpha;
            int g = color.G * alpha;
            int r = color.R * alpha;

            int rgba = color.ToArgb();

            int v0x,v1x,v2x,v0y,v1y,v2y,v0c,v1c,v2c;

            v0x = px1 - px0;
            v0y = py1 - py0;

            v1x = px2 - px1;
            v1y = py2 - py1;

            v2x = px0 - px2;
            v2y = py0 - py2;

            Tools.swap<int>(ref v0x, ref v0y);
            Tools.swap<int>(ref v1x, ref v1y);
            Tools.swap<int>(ref v2x, ref v2y);

            if (v0x < 0) { v0x = -v0x; } else { v0y = -v0y; }
            if (v1x < 0) { v1x = -v1x; } else { v1y = -v1y; }
            if (v2x < 0) { v2x = -v2x; } else { v2y = -v2y; }

            v0c = -(v0x*px0+v0y*py0);
            v1c = -(v1x*px1+v1y*py1);
            v2c = -(v2x*px2+v2y*py2);


            // process all points
            int minY = (py0 < py1) ? py0 : py1;
            minY = (minY < py2) ? minY : py2;
            int maxY = (py0 > py1) ? py0 : py1;
            maxY = (maxY > py2) ? maxY : py2;

            int rowIndex = minY * canvas.Width;
            
            for (int y = minY; y <= maxY; y++)
            {
                // compute ax+by+c =0, v(a,b) , u(k,l)=A-B, u(k,l) => v(l,-k)
                //int tmpx0 = (v0x == 0)? px0 : (-v0y * y - v0c) / v0x;
                //int tmpx1 = (v1x == 0) ? px2 : (-v1y * y - v1c) / v1x;
                //int tmpx2 = (v2x == 0) ? px2 : (-v2y * y - v2c) / v2x;

                int start = 0;
                int end = 0;

                int isCrossLine0 = (py0 == py1)? -1 : (y - py0) * (py1 - y);
                //int isCrossLine1 = (py1 == py2) ? -1 : (y - py1) * (py2 - y);
                //int isCrossLine2 = (py2 == py0) ? -1 : (y - py2) * (py0 - y);

                if (isCrossLine0 >= 0)
                {
                    
                    int isCrossLine1 = (py1 == py2) ? -1 : (y - py1) * (py2 - y);

                    if (isCrossLine1 >= 0)
                    {
                        int tmpx0 = (v0x == 0) ? px0 : (-v0y * y - v0c) / v0x;
                        int tmpx1 = (v1x == 0) ? px2 : (-v1y * y - v1c) / v1x;
                    
                        start = tmpx0;
                        end = tmpx1;
                    }
                    else
                    {
                        int tmpx0 = (v0x == 0) ? px0 : (-v0y * y - v0c) / v0x;
                        int tmpx2 = (v2x == 0) ? px2 : (-v2y * y - v2c) / v2x;

                        start = tmpx0;
                        end = tmpx2;

                    }
                }
                else
                {
                    int tmpx1 = (v1x == 0) ? px2 : (-v1y * y - v1c) / v1x;
                    int tmpx2 = (v2x == 0) ? px2 : (-v2y * y - v2c) / v2x;
                    start = tmpx1;
                    end = tmpx2;
                }

                if (start > end) Tools.swap<int>(ref start, ref end);

                

                int currIndex = rowIndex+start*4;

                    
                    nativeFunc.NewRowApplyColor64(canvas.Data, currIndex, end- start + 1, rgba, alpha);

                    

                    /*while (start <= end )// && start < canvas.WidthPixel)
                    {
                        int tb = canvas.Data[currIndex];
                        int tg = canvas.Data[currIndex + 1];
                        int tr = canvas.Data[currIndex + 2];


                        canvas.Data[currIndex] = (byte)((b + (tb * invAlpha)) >> 8);
                        canvas.Data[currIndex + 1] = (byte)((g + (tg * invAlpha)) >> 8);
                        canvas.Data[currIndex + 2] = (byte)((r + (tr * invAlpha)) >> 8);

                        start ++;
                        currIndex+= 4;
                    }*/


                
                
                rowIndex += canvas.Width;
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
