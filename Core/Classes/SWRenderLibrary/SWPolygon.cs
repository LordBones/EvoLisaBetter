using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    internal class SoftwareRenderLibrary
    {
        //private static byte [] CreateBufferForPolygon(ref int width, ref )

        public static void DrawFillPolygon(byte[] canvas, int width, Point[] points, Color color)
        {
            
        }

        /// <summary>
        /// alpha 255 opaque
        /// </summary>
        /// <param name="oldC"></param>
        /// <param name="newC"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte MixColorChanelWithAlpha(byte oldC,byte newC,byte alpha )
        {
            return (byte)(oldC + (((newC - oldC) * alpha) >> 8));
        }
    }

    public class Polygon
    {

        const int CONST_MinArea_Edge = 2;
        const int CONST_MinArea_OutFill = 3;
        const int CONST_MinArea_AreaBound = 1;
        const int CONST_MinArea_Empty = 0;

        const byte CONST_ScanLineDirection_up = 1;
        const byte CONST_ScanLineDirection_down = 2;
        const byte CONST_ScanLineDirection_both = 0;

        //const ushort CONST_OutFill_double = (ushort)((CONST_MinArea_Edge << 8) | CONST_MinArea_Edge);
        const ushort CONST_OutFill_double = 0x0303;
        const uint CONST_MinAreaOutFill_DoubleWord = 0x03030303;
        const uint CONST_MinAreaEmpty_DoubleWord = 0x00000000;
        const ulong CONST_MinAreaOutFill_QuadWord = 0x0303030303030303;
        const ulong CONST_MinAreaEmpty_QuadWord = 0x0000000000000000;

        /// <summary>
        /// pocet pixelu o kolik je minimalni oblast vetsi oproti
        /// </summary>
        const int CONST_MinAreaPadingCount = 1;

        private int _maxWidth, _maxHeight;

        private Point _minAreaOrigStart;
        private Size _minAreaOrigSize;

        private byte [] _minArea;
        private int _minAreaForSeedWidth;
        private int _minAreaForSeedHeight;

        Stack<General_RangeItem> stackUp = new Stack<General_RangeItem>();
        Stack<General_RangeItem> stackDown = new Stack<General_RangeItem>();

        //MyStack stackBetterUp = new MyStack();
        //MyStack stackBetterDown = new MyStack();




        // true size of alloced min area
        // can be higher
        // reduce repetable alocation of min_area
        private int _minAreaForSeedWidthAlocated;
        private int _minAreaForSeedHeightAlocated;


        #region public properties

        public byte[] MinArea
        {
            get { return _minArea; }
        }

        public int MinAreaWidthAlocated
        {
            get { return this._minAreaForSeedWidthAlocated; }
        }

        public int MinAreaWidth
        {
            get { return this._minAreaForSeedWidth; }
        }

        public int MinAreaHeight
        {
            get { return this._minAreaForSeedHeight; }
        }

        #endregion

        public Polygon(int maxWidth, int maxHeight)
        {
            this._maxHeight = maxHeight;
            this._maxWidth = maxWidth;
        }

        #region public mehtods

        /// <summary>
        /// setting on max size reduce realocing data for software render
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetStartBufferSize(int width, int height)
        {
            this._minAreaForSeedHeightAlocated = width + CONST_MinAreaPadingCount * 2;
            this._minAreaForSeedWidthAlocated = height + CONST_MinAreaPadingCount * 2;

            CreateMinAreaWithTopLeftEdge();
        }

        public void FillPolygon(DnaPoint[] points, byte[] canvas, Color color)
        {
            FindMinimalArea(points, out _minAreaOrigStart, out _minAreaOrigSize);

            Init();

            DrawPolygonBounds(points);

            //Point [] points2 = new Point[] { new Point(0, 0), new Point(this._maxWidth - 1, this._maxHeight - 1) };
            //Init(points2);

            //DrawPatternFill();

          

            FillAroundPolygon();

            //PolygonPointTable ppt = new PolygonPointTable(this._minAreaForSeedHeight);

            //GetPolygonRasterPoints(ppt, points);

            //FillPolygonIntoCanvas(this._minArea, this._minAreaForSeedWidthAlocated, ppt);

            //FillAroundPolygonBasicSeed();
            //CopyFillPolygonIntoCanvasFast(canvas, this._maxWidth, color);
            //CopyFillPolygonIntoCanvas(canvas, this._maxWidth, color);
            CopyFillPolygonIntoCanvasNewRender(canvas, this._maxWidth, color);

        }

        public void FillPolygonNative(DnaPoint[] points, byte[] canvas, Color color)
        {
            FindMinimalArea(points, out _minAreaOrigStart, out _minAreaOrigSize);

            Init();

            DrawPolygonBounds(points);

            //DrawPatternFill();

            FillAroundPolygon();

            //PolygonPointTable ppt = new PolygonPointTable(this._minAreaForSeedHeight);

            //GetPolygonRasterPoints(ppt, points);

            //FillPolygonIntoCanvas(this._minArea, this._minAreaForSeedWidthAlocated, ppt);

            //FillAroundPolygonBasicSeed();
            //CopyFillPolygonIntoCanvasFast(canvas, this._maxWidth, color);
            //CopyFillPolygonIntoCanvas(canvas, this._maxWidth, color);
            CopyFillPolygonIntoCanvasNewRender(canvas, this._maxWidth, color);

        }

        public void FillPolygon(Point[] points, byte[] canvas, Color color)
        {
            //Point [] points2 = new Point[] { new Point(0, 0), new Point(this._maxWidth - 1, this._maxHeight - 1) };
            //FindMinimalArea(points2, out _minAreaOrigStart, out _minAreaOrigSize);
            //Init();

            FindMinimalArea(points, out _minAreaOrigStart, out _minAreaOrigSize);

            Init();

            DrawPolygonBounds(points);

           
            //DrawPatternFill();

            //FillAroundPolygon();

            //PolygonPointTable ppt = new PolygonPointTable(this._minAreaForSeedHeight);

            //GetPolygonRasterPoints(ppt, points);
            //FillPolygonIntoCanvas(this._minArea, this._minAreaForSeedWidthAlocated, ppt);

            FillAroundPolygon();

            //FillAroundPolygonBasicSeed();
            //CopyFillPolygonIntoCanvasFast(canvas, this._maxWidth, color);
            //CopyFillPolygonIntoCanvas(canvas, this._maxWidth, color);
            CopyFillPolygonIntoCanvasNewRender(canvas, this._maxWidth, color);

        }

        public void FillPolygonCorrectSlow(Point[] points, byte[] canvas, Color color)
        {

            //Point [] points2 = new Point[] { new Point(0, 0), new Point(this._maxWidth - 1, this._maxHeight - 1) };
            //FindMinimalArea(points2, out _minAreaOrigStart, out _minAreaOrigSize);
            //Init();

            FindMinimalArea(points, out _minAreaOrigStart, out _minAreaOrigSize);
            Init();

            DrawPolygonBounds(points);

            //DrawPatternFill();

            //PolygonPointTable ppt = new PolygonPointTable(this._minAreaForSeedHeight);

            //GetPolygonRasterPoints(ppt, points);
            //FillPolygonIntoCanvas(this._minArea, this._minAreaForSeedWidthAlocated, ppt);

            FillAroundPolygonBasicSeed();
            
            //CopyFillPolygonIntoCanvasFast(canvas, this._maxWidth, color);
            CopyFillPolygonIntoCanvas(canvas, this._maxWidth, color);

        }

        public void FillPolygonBenchmark(byte[] canvas, Color color)
        {
            //FindMinimalArea(points, out _minAreaOrigStart, out _minAreaOrigSize);

            Point [] points = new Point[] { new Point(0, 0), new Point(this._maxWidth - 1, this._maxHeight - 1) };
            FindMinimalArea(points, out _minAreaOrigStart, out _minAreaOrigSize);
            Init();

            //DrawPatternFill();
            //DrawPolygonBounds(points);

            //FillAroundPolygon();

            PolygonPointTable ppt = new PolygonPointTable(this._minAreaForSeedHeight);

            GetPolygonRasterPoints(ppt, points);
            FillPolygonIntoCanvas(this._minArea, this._minAreaForSeedWidthAlocated, ppt);

            //General_FillFieldByUnboundSeedScanLineBasic(CONST_MinAreaPadingCount - 1, CONST_MinAreaPadingCount - 1);
            //General_FillFieldByUnboundSeedScanLineBetter(CONST_MinAreaPadingCount - 1, CONST_MinAreaPadingCount - 1);

            //CopyFillPolygonIntoCanvasFast(canvas, this._maxWidth, color);
            //CopyFillPolygonIntoCanvas(canvas, this._maxWidth, color);
            CopyFillPolygonIntoCanvasNewRender(canvas, this._maxWidth, color);

        }

        private void FillAroundPolygon()
        {
            int lineIndexUp = (CONST_MinAreaPadingCount) * this._minAreaForSeedWidthAlocated + CONST_MinAreaPadingCount ;

            int lineIndexDown = (_minAreaForSeedHeight - 2) * _minAreaForSeedWidthAlocated+1;

            for (int index = 0; index < this._minAreaOrigSize.Width; index++)
            {
                if (this._minArea[lineIndexUp] == CONST_MinArea_Empty)
                {
                    General_FillFieldByUnboundSeedScanLine(lineIndexUp);
                }
                if (this._minArea[lineIndexDown] == CONST_MinArea_Empty)
                {
                    General_FillFieldByUnboundSeedScanLine(lineIndexDown);
                }

                lineIndexDown++;
                lineIndexUp++;
            }

            int lineIndexLeft = this._minAreaForSeedWidthAlocated + 1;
            int lineIndexRight = this._minAreaForSeedWidthAlocated + this._minAreaOrigSize.Width;

            for (int index = 0; index < this._minAreaOrigSize.Height; index++)
            {
                if (this._minArea[lineIndexLeft] == CONST_MinArea_Empty)
                {
                    General_FillFieldByUnboundSeedScanLine(lineIndexLeft);
                }
                if (this._minArea[lineIndexRight] == CONST_MinArea_Empty)
                {
                    General_FillFieldByUnboundSeedScanLine(lineIndexRight);
                }

                lineIndexLeft += this._minAreaForSeedWidthAlocated;
                lineIndexRight += this._minAreaForSeedWidthAlocated;
            }

        }

        private void FillAroundPolygonBasicSeed()
        {
            int lineIndexUp = (CONST_MinAreaPadingCount ) * this._minAreaForSeedWidthAlocated + CONST_MinAreaPadingCount;

            int lineIndexDown = (_minAreaForSeedHeight - 2) * _minAreaForSeedWidthAlocated + 1;

            for (int index = 0; index < this._minAreaOrigSize.Width; index++)
            {
                if (this._minArea[lineIndexUp] == CONST_MinArea_Empty)
                {
                    FillFieldByUnboundSeed2(lineIndexUp);
                }
                if (this._minArea[lineIndexDown] == CONST_MinArea_Empty)
                {
                    FillFieldByUnboundSeed2(lineIndexDown);
                }

                lineIndexDown++;
                lineIndexUp++;
            }

            int lineIndexLeft = this._minAreaForSeedWidthAlocated + 1;
            int lineIndexRight = this._minAreaForSeedWidthAlocated + this._minAreaForSeedWidth-2;

            for (int index = 0; index < this._minAreaOrigSize.Height; index++)
            {
                if (this._minArea[lineIndexLeft] == CONST_MinArea_Empty)
                {
                    FillFieldByUnboundSeed2(lineIndexLeft);
                }
                if (this._minArea[lineIndexRight] == CONST_MinArea_Empty)
                {
                    FillFieldByUnboundSeed2(lineIndexRight);
                }

                lineIndexLeft += this._minAreaForSeedWidthAlocated;
                lineIndexRight += this._minAreaForSeedWidthAlocated;
            }

        }

        public bool IsMinAreaDataEqual(Polygon polygon)
        {
            if (polygon.MinAreaHeight != this.MinAreaHeight || polygon.MinAreaWidth != MinAreaWidth)
                return false;

            int startIndexFirst = polygon.MinAreaWidthAlocated*2+2;
            int startIndexSecond = this._minAreaForSeedWidthAlocated+1;
            for (int y = 0; y < _minAreaOrigSize.Height; y++)
            {
                for (int x = 0; x < _minAreaOrigSize.Width; x++)
                {
                    if (polygon.MinArea[startIndexFirst + x] != _minArea[startIndexSecond + x])
                    {
                        return false;
                    }
                }

                startIndexFirst += polygon.MinAreaWidthAlocated;
                startIndexSecond += _minAreaForSeedWidthAlocated;
            }

            return true;
        }

        public void SaveMinAreaToFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                int startIndexFirst = 0;
                for (int y = 0; y < _minAreaForSeedHeight; y++)
                {
                    for (int x = 0; x < _minAreaForSeedWidth; x++)
                    {
                        byte data = _minArea[startIndexFirst + x];
                        if (data == CONST_MinArea_AreaBound)
                        {
                            fs.WriteByte(Convert.ToByte('#'));
                        }
                        else if (data == CONST_MinArea_Edge)
                        {
                            fs.WriteByte(Convert.ToByte('*'));
                        }
                        else if (data == CONST_MinArea_OutFill)
                        {
                            fs.WriteByte(Convert.ToByte('.'));
                        }
                        else if (data == CONST_MinArea_Empty)
                        {
                            fs.WriteByte(Convert.ToByte(' '));
                        }
                        else
                        {
                            fs.WriteByte(Convert.ToByte('X'));
                        }
                    }

                    fs.WriteByte(Convert.ToByte('\n'));
                    startIndexFirst += _minAreaForSeedWidthAlocated;
                }

            }
        }

        #endregion

        private void Init()
        {
            this._minAreaForSeedWidth = this._minAreaOrigSize.Width + CONST_MinAreaPadingCount * 2;
            this._minAreaForSeedHeight = this._minAreaOrigSize.Height + CONST_MinAreaPadingCount * 2;

            if (this._minAreaForSeedWidth > this._minAreaForSeedWidthAlocated ||
                this._minAreaForSeedHeight > this._minAreaForSeedHeightAlocated)
            {
                this._minAreaForSeedHeightAlocated = this._minAreaForSeedHeight;
                this._minAreaForSeedWidthAlocated = this._minAreaForSeedWidth;

                CreateMinAreaWithTopLeftEdge();
            }
            else
            {
                /// fast clear field
                /// 

                int startIndex = _minAreaForSeedWidthAlocated + 1;
                int endindex = this._minAreaForSeedHeight * this._minAreaForSeedWidthAlocated;
                while (startIndex < endindex)
                {
                    Array.Clear(this._minArea, startIndex, this._minAreaForSeedWidth - 1);
                    startIndex += this._minAreaForSeedWidthAlocated;
                }
            }



            // init seed bounds


            int indexSecondLine = (_minAreaForSeedHeight - 1) * _minAreaForSeedWidthAlocated;

            for (int index = 0; index < _minAreaForSeedWidth; index++)
            {
                this._minArea[indexSecondLine + index] = 1;
            }

            indexSecondLine = _minAreaForSeedWidth - 1;

            for (int index = 0; index < _minAreaForSeedHeight; index++)
            {
                this._minArea[indexSecondLine] = 1;

                indexSecondLine += this._minAreaForSeedWidthAlocated;
            }

        }

        private void CreateMinAreaWithTopLeftEdge()
        {
            _minArea = new byte[this._minAreaForSeedWidthAlocated * this._minAreaForSeedHeightAlocated];

            for (int index = 0; index < _minAreaForSeedWidthAlocated; index++)
            {
                this._minArea[index] = CONST_MinArea_AreaBound;
            }

            int rowIndex = 0;

            for (int index = 0; index < _minAreaForSeedHeightAlocated; index++)
            {
                this._minArea[rowIndex] = CONST_MinArea_AreaBound;

                rowIndex += this._minAreaForSeedWidthAlocated;
            }
        }

        #region copy fill polygon into canvas

        // may be fastess blend aplpha without test saturation
//        R = TopR + (SourceR * alpha) >> 8;
//G = TopG + (SourceG * alpha) >> 8;
//B = TopB + (SourceB * alpha) >> 8;


        private void CopyFillPolygonIntoCanvas(byte[] canvas, int width, Color color)
        {
            int rowIndex = this._minAreaForSeedWidthAlocated * CONST_MinAreaPadingCount + CONST_MinAreaPadingCount;

            int origRowIndex = this._minAreaOrigStart.Y * this._maxWidth + this._minAreaOrigStart.X;

            int invAlpha = 256 - color.A;
            int aMult = 0x10000 * color.A / 255;
            int rem = 0x10000 - aMult; // Remaining fraction

            byte red = color.R;
            byte blue = color.B;
            byte green = color.G;

            int ar = rem * red;
            int ab = rem * blue;
            int ag = rem * green;
            int arrem = aMult * red;
            int abrem = aMult * blue;
            int agrem = aMult * green;


            int minHeight = this._minAreaOrigSize.Height;
            int minWidth = this._minAreaOrigSize.Width;
            for (int indexY = 0; indexY < minHeight; indexY++)
            {
                int canvasIndex = (origRowIndex) << 2;

                int indexX = rowIndex;
                int indexXEnd = rowIndex + minWidth;
                while (indexX < indexXEnd)
                {
                    byte value = this._minArea[indexX];
                    if (value == CONST_MinArea_Edge || value == 0)
                    {
                        //canvas[canvasIndex] = (byte)((alpha * blue + invAlpha * canvas[canvasIndex]) >> 8);
                        //canvas[canvasIndex + 1] = (byte)((alpha * green + invAlpha * canvas[canvasIndex + 1]) >> 8);
                        //canvas[canvasIndex + 2] = (byte)((alpha * red + invAlpha * canvas[canvasIndex + 2]) >> 8);

                        //canvas[canvasIndex] = (byte)((ab + invAlpha * canvas[canvasIndex]) >> 8);
                        //canvas[canvasIndex + 1] = (byte)((ag + invAlpha * canvas[canvasIndex + 1]) >> 8);
                        //canvas[canvasIndex + 2] = (byte)((ar + invAlpha * canvas[canvasIndex + 2]) >> 8);

                        canvas[canvasIndex] = (byte)((abrem + rem * canvas[canvasIndex]) >> 16);
                        canvas[canvasIndex + 1] = (byte)((agrem + rem * canvas[canvasIndex + 1]) >> 16);
                        canvas[canvasIndex + 2] = (byte)((arrem + rem * canvas[canvasIndex + 2]) >> 16);
                    }

                    canvasIndex += 4;
                    indexX++;
                }

                rowIndex += this._minAreaForSeedWidthAlocated;
                origRowIndex += this._maxWidth;
            }

        }

        //        int blend(unsigned char result[4], unsigned char fg[4], unsigned char bg[4])
        //{
        //    unsigned int alpha = fg[3] + 1;
        //    unsigned int inv_alpha = 256 - fg[3];
        //    result[0] = (unsigned char)((alpha * fg[0] + inv_alpha * bg[0]) >> 8);
        //    result[1] = (unsigned char)((alpha * fg[1] + inv_alpha * bg[1]) >> 8);
        //    result[2] = (unsigned char)((alpha * fg[2] + inv_alpha * bg[2]) >> 8);
        //    result[3] = 0xff;
        //    }

        private void CopyFillPolygonIntoCanvasNewRender(byte[] canvas, int width, Color color)
        {
            int rowIndex = this._minAreaForSeedWidthAlocated * CONST_MinAreaPadingCount + CONST_MinAreaPadingCount;

            int origRowIndex = this._minAreaOrigStart.Y * this._maxWidth + this._minAreaOrigStart.X;

            int invAlpha = 256 - color.A;
            int aMult = 0x10000 * color.A / 255;
            int rem = 0x10000 - aMult; // Remaining fraction

            byte red = color.R;
            byte blue = color.B;
            byte green = color.G;

            int ar = rem * red;
            int ab = rem * blue;
            int ag = rem * green;
            int arrem = aMult * red;
            int abrem = aMult * blue;
            int agrem = aMult * green;


            int minHeight = this._minAreaOrigSize.Height;
            int minWidth = this._minAreaOrigSize.Width;
            for (int indexY = 0; indexY < minHeight; indexY++)
            {
                int canvasIndex = (origRowIndex) << 2;

                int indexX = rowIndex;
                int indexXEnd = rowIndex + minWidth;
                while (indexX < indexXEnd)
                {
                    byte value = this._minArea[indexX];
                    if (value == CONST_MinArea_Edge || value == CONST_MinArea_Empty)
                    {
                        //canvas[canvasIndex] = (byte)((alpha * blue + invAlpha * canvas[canvasIndex]) >> 8);
                        //canvas[canvasIndex + 1] = (byte)((alpha * green + invAlpha * canvas[canvasIndex + 1]) >> 8);
                        //canvas[canvasIndex + 2] = (byte)((alpha * red + invAlpha * canvas[canvasIndex + 2]) >> 8);

                        //canvas[canvasIndex] = (byte)((ab + invAlpha * canvas[canvasIndex]) >> 8);
                        //canvas[canvasIndex + 1] = (byte)((ag + invAlpha * canvas[canvasIndex + 1]) >> 8);
                        //canvas[canvasIndex + 2] = (byte)((ar + invAlpha * canvas[canvasIndex + 2]) >> 8);

                        canvas[canvasIndex] = (byte)((abrem + rem * canvas[canvasIndex]) >> 16);
                        canvas[canvasIndex + 1] = (byte)((agrem + rem * canvas[canvasIndex + 1]) >> 16);
                        canvas[canvasIndex + 2] = (byte)((arrem + rem * canvas[canvasIndex + 2]) >> 16);
                    }

                    canvasIndex += 4;
                    indexX++;
                }

                rowIndex += this._minAreaForSeedWidthAlocated;
                origRowIndex += this._maxWidth;
            }

        }

        #endregion


        #region better flood scanfill



        private void General_FillFieldByUnboundSeedScanLine(int startIndex)
        {
            //Stack<General_RangeItemBetter> stack = new Stack<General_RangeItemBetter>(this._minAreaForSeedHeight);

            //while (this._minArea[minAreaIndex] == CONST_MinArea_Empty)
            //{
            //    this._minArea[minAreaIndex] = CONST_MinArea_OutFill;
            //    minAreaIndex++;
            //}

            int endIndex = FindEndOfEmptyLineAndFill(startIndex);

            //if (this.MinArea[startIndex] == CONST_MinArea_Empty)
            if (startIndex != endIndex)
            {
                int width = this._minAreaForSeedWidthAlocated;

                stackUp.Push(new General_RangeItem(startIndex-width, endIndex-width-1));
                stackDown.Push(new General_RangeItem(startIndex+width, endIndex+width-1));

            }

            //int width = this._minAreaForSeedWidthAlocated;
            while (stackDown.Count > 0 || stackUp.Count > 0)
            {
                while (stackDown.Count > 0)
                {
                    General_RangeItem ri = stackDown.Pop();

                    //General_FindLinesForFillBetter3(ri.x +width, ri.x2+width, CONST_ScanLineDirection_down);
                    General_FindLinesForFill2Down(ri.x, ri.x2);

                }

                while (stackUp.Count > 0)
                {
                    General_RangeItem ri = stackUp.Pop();

                    //General_FindLinesForFillBetter3(ri.x - width, ri.x2 - width, CONST_ScanLineDirection_up);
                    General_FindLinesForFillUp(ri.x, ri.x2);

                }
            }
        }

        private void General_FindLinesForFill2Down(int x1, int x2)
        {



            int width = this._minAreaForSeedWidthAlocated;

            int startRangeX = x1;

            // add left overflow line
            if (_minArea[startRangeX] == CONST_MinArea_Empty)
            {
                startRangeX--;
                while (_minArea[startRangeX] == CONST_MinArea_Empty)
                {
                    this._minArea[startRangeX] = CONST_MinArea_OutFill;
                    startRangeX--;
                }

                startRangeX++;

                if (startRangeX < x1)
                {
                    int testx = startRangeX - width;
                    int testy = x1 - width - 2;
                    bool isNotEmpty = TestIfIsAllNotEmpty(testx, testy);

                    if (!isNotEmpty)
                        stackUp.Push(new General_RangeItem(testx, testy));

                }
            }

            // add all lines in middle of last interval
            //int startRangeX = startRangeX;
            int endRangeX = x1;
            while (startRangeX <= x2)
            {
                // find end line

                while (_minArea[endRangeX] == CONST_MinArea_Empty)
                {
                    this._minArea[endRangeX] = CONST_MinArea_OutFill;
                    endRangeX++;
                }



                //tmpx2 = FindEndOfEmptyLineAndFill(tmpx1);

                //endRangeX = FindEndOfEmptyLineAndFillFaster(endRangeX);
                //tmpx2 = FindEndOfEmptyLineAndFill(tmpx1);


                if (endRangeX != startRangeX)
                {
                    if (endRangeX - 1 > x2)
                    {

                        int testx = x2 + 2 - width;
                        int testy = endRangeX - 1 - width;
                        bool isNotEmpty = TestIfIsAllNotEmpty(testx, testy);

                        if (!isNotEmpty)
                            stackUp.Push(new General_RangeItem(testx, testy));

                    }
                    //General_RangeItemBetter gri;
                    //gri = new General_RangeItemBetter(tmpx1, tmpx2 - 1);
                    stackDown.Push(new General_RangeItem(startRangeX + width, endRangeX - 1 + width));
                }

                startRangeX = endRangeX;
                startRangeX++;

                // find start line
                while (startRangeX <= x2 && _minArea[startRangeX] != CONST_MinArea_Empty)
                {
                    startRangeX++;
                }

                endRangeX = startRangeX;
            }
        }

        private void General_FindLinesForFillDown(int x1, int x2)
        {
            int width = this._minAreaForSeedWidthAlocated;

            int startRangeX = x1;

            // add left overflow line
            if (_minArea[startRangeX] == CONST_MinArea_Empty)
            {
                startRangeX--;
                while (_minArea[startRangeX] == CONST_MinArea_Empty)
                {
                    startRangeX--;
                }

                startRangeX++;

                if (startRangeX < x1)
                {
                    int testx = startRangeX - width;
                    int testy = x1 - width - 2;
                    bool isNotEmpty = TestIfIsAllNotEmpty(testx, testy);

                    if (!isNotEmpty)
                        stackUp.Push(new General_RangeItem(testx, testy));

                }
            }

            // add all lines in middle of last interval
            //int startRangeX = startRangeX;

            while (startRangeX <= x2)
            {
                // find end line
                int endRangeX = startRangeX;
                while (this._minArea[endRangeX] == CONST_MinArea_Empty)
                {
                    this._minArea[endRangeX] = CONST_MinArea_OutFill;
                    endRangeX++;
                }

                //tmpx2 = FindEndOfEmptyLineAndFill(tmpx1);

                //tmpx2 = FindEndOfEmptyLineAndFillFaster(tmpx1);
                //tmpx2 = FindEndOfEmptyLineAndFill(tmpx1);


                if (endRangeX != startRangeX)
                {
                    if (endRangeX - 1 > x2)
                    {

                        int testx = x2 + 2 - width;
                        int testy = endRangeX - 1 - width;
                        bool isNotEmpty = TestIfIsAllNotEmpty(testx, testy);

                        if (!isNotEmpty)
                            stackUp.Push(new General_RangeItem(testx, testy));

                    }
                    //General_RangeItemBetter gri;
                    //gri = new General_RangeItemBetter(tmpx1, tmpx2 - 1);
                    stackDown.Push(new General_RangeItem(startRangeX + width, endRangeX - 1 + width));
                }

                startRangeX = endRangeX;
                startRangeX++;

                // find start line
                while (this._minArea[startRangeX] != CONST_MinArea_Empty && startRangeX <= x2)
                {
                    startRangeX++;
                }
            }

        }

        private void General_FindLinesForFillUp(int x1, int x2)
        {
            int width = this._minAreaForSeedWidthAlocated;

            int startRangeX = x1;

            // add left overflow line
            if (_minArea[startRangeX] == CONST_MinArea_Empty)
            {
                startRangeX--;
                while (_minArea[startRangeX] == CONST_MinArea_Empty)
                {
                    startRangeX--;
                }

                startRangeX++;

                if (startRangeX < x1)
                {
                    int testx = startRangeX + width;
                    int testy = x1 + width - 2;
                    bool isNotEmpty = TestIfIsAllNotEmpty(testx, testy);

                    if (!isNotEmpty)
                        stackDown.Push(new General_RangeItem(testx, testy));

                }
            }



            // add all lines in middle of last interval

            while (startRangeX <= x2)
            {

                // find end line
                int endRangeX = startRangeX;
                while (this._minArea[endRangeX] == CONST_MinArea_Empty)
                {
                    this._minArea[endRangeX] = CONST_MinArea_OutFill;
                    endRangeX++;
                }

                //tmpx2 = FindEndOfEmptyLineAndFill(tmpx1);

                //tmpx2 = FindEndOfEmptyLineAndFillFaster(tmpx1);
                //tmpx2 = FindEndOfEmptyLineAndFill(tmpx1);


                if (endRangeX != startRangeX)
                {
                    //byte tmpDirection = CONST_ScanLineDirection_both;

                    if (endRangeX - 1 > x2)
                    {
                        int testx = x2 + 2 + width;
                        int testy = endRangeX - 1 + width;


                        bool isNotEmpty = TestIfIsAllNotEmpty(testx, testy);

                        if (!isNotEmpty)

                            stackDown.Push(new General_RangeItem(testx, testy));
                    }
                    //General_RangeItemBetter gri;
                    //gri = new General_RangeItemBetter(tmpx1, tmpx2 - 1);
                    stackUp.Push(new General_RangeItem(startRangeX - width, endRangeX - width - 1));
                }

                startRangeX = endRangeX;
                startRangeX++;

                // find start line
                while (startRangeX <= x2 && this._minArea[startRangeX] != CONST_MinArea_Empty)
                {
                    startRangeX++;
                }
            }

        }

     
        private int FindEndOfEmptyLineAndFill(int startX)
        {
            while (this._minArea[startX] == CONST_MinArea_Empty)
            {
                this._minArea[startX] = CONST_MinArea_OutFill;
                startX++;
            }

            return startX;
        }

        private int FindEndOfEmptyLineAndFillFaster(int startX)
        {
            //unsafe
            //{
            //    fixed (byte * ptrMinA = this._minArea)
            //    {
            //        uint * ptrA = (uint*)(ptrMinA + startX);
            //        int index = 0;
            //        while (ptrA[index] == CONST_MinAreaEmpty_DoubleWord)
            //        {
            //            ptrA[index] = CONST_MinAreaOutFill_DoubleWord;
            //            index++;
            //            startX += 4;
            //        }
            //    }
            //}

            //unsafe
            //{
            //    fixed (byte * ptrMinA = this._minArea)
            //    {
            //        ulong * ptrA = (ulong*)(ptrMinA + startX);

            //        int index = 0;
            //        while (ptrA[index] == CONST_MinAreaEmpty_QuadWord)
            //        {
            //            ptrA[index] = CONST_MinAreaOutFill_QuadWord;
            //            index++;
            //            startX += 8;
            //        }

            //        startX += index << 3;
            //    }
            //}

            unsafe
            {
                fixed (byte * ptrMinA = this._minArea)
                {
                    //byte * ptrA = (byte*)(ptrMinA + startX);

                    while (ptrMinA[startX] == CONST_MinArea_Empty)
                    {
                        ptrMinA[startX] = CONST_MinArea_OutFill;
                        startX++;
                    }
                }
            }

            //while (this._minArea[startX] == CONST_MinArea_Empty)
            //{
            //    this._minArea[startX] = CONST_MinArea_OutFill;
            //    startX++;
            //}

            return startX;
        }

        private bool TestIfIsAllNotEmpty(int x1, int x2)
        {
            while (x1 <= x2)
            {
                if (this._minArea[x1] == CONST_MinArea_Empty)
                {
                    return false;
                }
                x1++;
            }

            return true;
        }


        #endregion


        #region reference flood scanfill

        struct General_RangeItem
        {
            public int x;
            public int x2;

            public General_RangeItem(int px, int px2)
            {
                x = px;
                x2 = px2;

            }
        }


        #endregion

        #region General basic seed fill

        private void FillFieldByUnboundSeed2(int startIndex)
        {
            Queue<int> pointsForFill = new Queue<int>();

            pointsForFill.Enqueue(startIndex);

            while (pointsForFill.Count > 0)
            {
                int index = pointsForFill.Dequeue();

                if (this._minArea[index] != 0)
                    continue;

                this._minArea[index] = 3;

                int indexTmp = index + this._minAreaForSeedWidthAlocated;
                if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);
                indexTmp = index - this._minAreaForSeedWidthAlocated;
                if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);

                indexTmp = index + 1;

                if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);
                indexTmp = index - 1;
                if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);


                if (pointsForFill.Count > 200)
                {
                    int i = 0;
                    i += 464 * i;
                }
            }
        }

        private void FillFieldByUnboundSeed(int startX, int startY)
        {
            Queue<int> pointsForFill = new Queue<int>();

            int startIndex = startY * this._minAreaForSeedWidthAlocated + startX;
            pointsForFill.Enqueue(startIndex);

            while (pointsForFill.Count > 0)
            {
                int index = pointsForFill.Dequeue();

                if (this._minArea[index] != 0)
                    continue;

                this._minArea[index] = 3;

                //int indexTmp = index + this._minAreaForSeedWidth;
                //if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);
                //indexTmp = index - this._minAreaForSeedWidth;
                //if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);

                int indexTmp = index + this._minAreaForSeedWidthAlocated;
                while (_minArea[indexTmp] == 0)
                {
                    this._minArea[indexTmp] = 3;

                    int scanIndex = indexTmp - 1;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);
                    indexTmp = indexTmp + 1;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);

                    indexTmp += this._minAreaForSeedWidthAlocated;
                }

                indexTmp = index - this._minAreaForSeedWidthAlocated;
                while (_minArea[indexTmp] == 0)
                {
                    this._minArea[indexTmp] = 3;

                    int scanIndex = indexTmp - 1;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);
                    indexTmp = indexTmp + 1;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);

                    indexTmp -= this._minAreaForSeedWidthAlocated;
                }

                indexTmp = index + 1;
                while (_minArea[indexTmp] == 0)
                {
                    this._minArea[indexTmp] = 3;

                    int scanIndex = indexTmp - this._minAreaForSeedWidthAlocated;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);
                    indexTmp = indexTmp + this._minAreaForSeedWidthAlocated;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);

                    indexTmp++;
                }

                indexTmp = index - 1;
                while (_minArea[indexTmp] == 0)
                {
                    this._minArea[indexTmp] = 3;

                    int scanIndex = indexTmp - this._minAreaForSeedWidthAlocated;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);
                    indexTmp = indexTmp + this._minAreaForSeedWidthAlocated;
                    if (_minArea[scanIndex] == 0) pointsForFill.Enqueue(scanIndex);

                    indexTmp--;
                }

                //if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);
                //indexTmp = index - 1;
                //if (_minArea[indexTmp] == 0) pointsForFill.Enqueue(indexTmp);

            }
        }

        #endregion






        private void FindMinimalArea(Point[] points, out Point start, out Size size)
        {
            start = new Point(this._maxWidth, this._maxHeight);
            Point end = new Point(1, 1);

            for (int index = 0; index < points.Length; index++)
            {
                Point point = points[index];
                if (point.X < start.X) start.X = point.X;
                if (point.Y < start.Y) start.Y = point.Y;
                if (point.X > end.X) end.X = point.X;
                if (point.Y > end.Y) end.Y = point.Y;
            }

            size = new Size(end.X - start.X + 1, end.Y - start.Y + 1);
        }

        private void FindMinimalArea(DnaPoint[] points, out Point start, out Size size)
        {
            start = new Point(this._maxWidth, this._maxHeight);
            Point end = new Point(1, 1);

            for (int index = 0; index < points.Length; index++)
            {
                DnaPoint point = points[index];
                if (point.X < start.X) start.X = point.X;
                if (point.Y < start.Y) start.Y = point.Y;
                if (point.X > end.X) end.X = point.X;
                if (point.Y > end.Y) end.Y = point.Y;
            }

            size = new Size(end.X - start.X + 1, end.Y - start.Y + 1);
        }


        private void DrawPatternFill()
        {
            int startIndexFirst =  _minAreaForSeedWidthAlocated + CONST_MinAreaPadingCount;
            int t =0;
            for (int y = 0; y < _minAreaOrigSize.Height; y++)
            {
                if (t >= 13)
                {
                    t = 0;
                }

                for (int x = t; x < _minAreaOrigSize.Width; x += 13)
                {
                    _minArea[startIndexFirst + x] = CONST_MinArea_Edge;
                }

                t++;
                startIndexFirst += _minAreaForSeedWidthAlocated;
            }
        }

        private void DrawPolygonBounds(DnaPoint[] points)
        {
            for (int index = 1; index < points.Length; index++)
            {
                DrawLine(this._minArea, this._minAreaForSeedWidthAlocated, points[index - 1], points[index]);
            }

            DrawLine(this._minArea, this._minAreaForSeedWidthAlocated, points[points.Length - 1], points[0]);
        }

        private void DrawPolygonBounds(Point[] points)
        {
            DnaPoint first = new DnaPoint();
            DnaPoint second = new DnaPoint();
            for (int index = 1; index < points.Length; index++)
            {
                Point dnp = points[index - 1];
                first.X = (short)dnp.X;
                first.Y = (short)dnp.Y;
                dnp = points[index];
                second.X = (short)dnp.X;
                second.Y = (short)dnp.Y;

                DrawLine(this._minArea, this._minAreaForSeedWidthAlocated, first, second);
            }

            first.X = (short)points[points.Length - 1].X;
            first.Y = (short)points[points.Length - 1].Y;
            second.X = (short)points[0].X;
            second.Y = (short)points[0].Y;


            DrawLine(this._minArea, this._minAreaForSeedWidthAlocated, first, second);
        }

        private void DrawLine(byte[] canvas, int width, DnaPoint startP, DnaPoint endP)
        {
            int widthMull = width;

            int modifX = startP.X - this._minAreaOrigStart.X + CONST_MinAreaPadingCount;
            int modifY = startP.Y - this._minAreaOrigStart.Y + CONST_MinAreaPadingCount;
            int modifX2 = endP.X - this._minAreaOrigStart.X + CONST_MinAreaPadingCount;
            int modifY2 = endP.Y - this._minAreaOrigStart.Y + CONST_MinAreaPadingCount;


            int x = modifX;
            int y = modifY * widthMull;
            int w = modifX2 - modifX;
            int h = modifY2 - modifY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) 
                dx1 = -1; 
            else if (w > 0) 
                dx1 = 1;
            if (h < 0) dy1 = 
                -widthMull; 
            else if (h > 0) 
                dy1 = widthMull;
            if (w < 0) 
                dx2 = -1; 
            else if (w > 0) 
                dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) 
                    dy2 = -widthMull; 
                else if (h > 0) 
                    dy2 = widthMull;

                dx2 = 0;
            }
            int numerator = longest >> 1;



            for (int i=0; i <= longest; i++)
            {
                #region set pixel


                int index = y + x;
                canvas[index] = 2;


                #endregion


                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

        }

        #region new draw polygon


        private void FillPolygonIntoCanvas(byte [] canvas, int canvasWidth, PolygonPointTable ppt)
        {
            int rowIndex = 0;
            for (int y = 0; y < ppt._table.Length; y++)
            {
                List<int> points = ppt._table[y];


                if (points.Count > 1)
                {
                    int index = 0;

                    while ((index + 1) < points.Count)
                    {
                        int xStart = points[index];
                        int xEnd = points[index + 1];

                        if (xStart != xEnd)
                        {
                            canvas[rowIndex + xStart] = CONST_MinArea_Edge;
                            canvas[rowIndex + xEnd] = CONST_MinArea_Edge;

                            xStart++;
                            while (xStart < xEnd)
                            {
                                canvas[rowIndex + xStart] = CONST_MinArea_OutFill;
                                xStart++;
                            }
                        }
                        else
                        {
                            canvas[rowIndex + xStart] = CONST_MinArea_OutFill;
                        }

                        index += 2;
                    }
                }

                rowIndex += canvasWidth;
            }
        }

        private void GetPolygonRasterPoints(PolygonPointTable ppt, Point[] points)
        {
            DnaPoint [] p = new DnaPoint[points.Length];

            for (int index = 0; index < points.Length; index++)
            {
                p[index] = new DnaPoint((short)points[index].X, (short)points[index].Y);
            }

            GetPolygonRasterPoints(ppt, p);
        }

        private void GetPolygonRasterPoints(PolygonPointTable ppt, DnaPoint[] points)
        {
            for (int index = 1; index < points.Length; index++)
            {
                DrawLine(ppt, points[index - 1], points[index]);
            }

            DrawLine(ppt, points[points.Length - 1], points[0]);
        }
        

        private void DrawLine(PolygonPointTable ppt, DnaPoint startP, DnaPoint endP)
        {
            int modifX = startP.X - this._minAreaOrigStart.X + CONST_MinAreaPadingCount;
            int modifY = startP.Y - this._minAreaOrigStart.Y + CONST_MinAreaPadingCount;
            int modifX2 = endP.X - this._minAreaOrigStart.X + CONST_MinAreaPadingCount;
            int modifY2 = endP.Y - this._minAreaOrigStart.Y + CONST_MinAreaPadingCount;


            int x = modifX;
            int y = modifY;
            int w = modifX2 - modifX;
            int h = modifY2 - modifY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0)
                dx1 = -1;
            else if (w > 0)
                dx1 = 1;
            if (h < 0) dy1 =
                -1;
            else if (h > 0)
                dy1 = 1;
            if (w < 0)
                dx2 = -1;
            else if (w > 0)
                dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0)
                    dy2 = -1;
                else if (h > 0)
                    dy2 = 1;

                dx2 = 0;
            }
            int numerator = longest >> 1;



            for (int i=0; i <= longest; i++)
            {

                #region add edge point into list
                
                ppt.AddPoint(y, x);
              
                #endregion


                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

        }


        private class PolygonPointTable
        {
            public List<int> [] _table;


            public PolygonPointTable(int maxHeight)
            {
                _table = new List<int>[maxHeight];

                for (int y = 0; y < maxHeight; y++)
                {
                    _table[y] = new List<int>(10);
                }

            }

            public void AddPoint(int y, int x)
            {
                int index = 0;
                int count = _table[y].Count;
                List<int> rowPoints = _table[y];
                while (index < count && rowPoints[index] < x)
                {
                    index++;
                }

                if (count <= index)
                {
                    

                        rowPoints.Add(x);
                }
                else
                {
                    if (rowPoints[index] != x)
                    rowPoints.Insert(index, x);
                }

            }
        }

        #endregion

        private class MyQueue
        {
            public int [] _Queue;
            private int _startIndex;
            private int _endIndex;

            public MyQueue(int initSize)
            {
                _Queue = new int[initSize];
                _startIndex = 0;
                _endIndex = 0;
            }

            public int Count()
            {
                return _endIndex - _startIndex;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enqueue(int value)
            {
                _Queue[_endIndex] = value;
                _endIndex++;
            }

            public int Dequeue()
            {
                int tmp = _Queue[_startIndex];
                _startIndex++;
                return tmp;
            }



        }

    }
}
