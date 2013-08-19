using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;

namespace GenArt.Core.Classes
{
    public class ImageEdges
    {
        public byte CONST_EdgesPoints2D_Edge = 1;

        public int Width;
        public int Height;
        public DnaPoint [] [] EdgePointsByX;
        public DnaPoint [][] EdgePointsByY;

        public DnaPoint [] EdgePoints;
        public Array2D EdgesPoints2D;

        public ImageEdges(int imageWidth, int imageHeight)
        {
            this.Width = imageWidth;
            this.Height = imageHeight;


            EdgePoints = new DnaPoint[imageHeight * imageWidth];
            EdgesPoints2D = new Array2D(imageWidth, imageHeight);

            EdgePointsByY = new DnaPoint[imageHeight][];
            EdgePointsByX = new DnaPoint[imageWidth][];

        }

        public DnaPoint GetRandomBorderPoint()
        {
            //get random end line on border canvas

            int pointX = 0;
            int pointY = 0;

            int tmpRnd = Tools.GetRandomNumber(1, 5);
            if (tmpRnd == 1)
            {
                pointX = Tools.GetRandomNumber(0, this.Width);
                pointY = 0;
            }
            else if (tmpRnd == 2)
            {
                pointX = this.Width - 1;
                pointY = Tools.GetRandomNumber(0, this.Height);
            }
            else if (tmpRnd == 3)
            {
                pointX = Tools.GetRandomNumber(0, this.Width);
                pointY = this.Height - 1;
            }
            else if (tmpRnd == 4)
            {
                pointX = 0;
                pointY = Tools.GetRandomNumber(0, this.Height);
            }

            return new DnaPoint((short)pointX, (short)pointY);
        }

        /// <summary>
        /// vraci prvni nalezenou hranu na definovane usecce mimo prvniho bodu
        /// pokud nic nenajde vraci null
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <returns></returns>
        public DnaPoint? GetFirstEdgeOnLineDirection(int startX, int startY, int endX, int endY)
        {
            int x = startX;
            int y = startY;
            int w = endX - startX;
            int h = endY - startY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) { dx1 = -1; dx2 = -1; } else if (w > 0) { dx1 = 1; dx2 = 1; }
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                int tmp = longest;
                longest = shortest;
                shortest = tmp;

                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;

            int lastPointX = x;
            int lastPointY = y;

            for (int i=0; i <= longest; i++)
            {
                if (y != startY && x != startX)
                {
                    #region test first edge
                    int index = (y * this.Width + x);

                    if (this.EdgesPoints2D.Data[index] == CONST_EdgesPoints2D_Edge)
                        return new Nullable<DnaPoint>(new DnaPoint((short)x, (short)y));

                    #region test edge
                    // dojdeli soucasne k posunu po ose x a y, test okolnich dvou bodu zdali nejsou hranove
                    if (lastPointX != x && lastPointY != y)
                    {
                        index = (y * this.Width + lastPointX);
                        if(this.EdgesPoints2D.Data[index] == CONST_EdgesPoints2D_Edge)
                            return new Nullable<DnaPoint>(new DnaPoint((short)lastPointX, (short)y));

                        index = (lastPointY * this.Width + x);
                        if (this.EdgesPoints2D.Data[index] == CONST_EdgesPoints2D_Edge)
                            return new Nullable<DnaPoint>(new DnaPoint((short)x, (short)lastPointY));

                    }

                    #endregion

                    #endregion
                }

                numerator += shortest;

                // zapamatovani predchoziho bodu
                lastPointX = x;
                lastPointY = y;

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

            return null;

        }

        public DnaPoint ? GetRandomCloserEdgePoint(DnaPoint startPoint, int countTrys)
        {
            int shorttest = int.MaxValue;
            DnaPoint ? result = null;

            for (; countTrys > 0; countTrys--)
            {
                DnaPoint endPoint = GetRandomBorderPoint();

                DnaPoint ? resultPoint = GetFirstEdgeOnLineDirection(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                if (resultPoint.HasValue)
                {
                    int tmpNew = Tools.fastAbs(endPoint.X - startPoint.X) + Tools.fastAbs(endPoint.Y - startPoint.Y) - 1;

                    if (shorttest > tmpNew)
                    {
                        shorttest = tmpNew;
                        result = endPoint;
                    }
                }
            }

            return result;
        }
    }

    

    public class EdgeDetector
    {
        private CanvasBGRA _originalImage = null;
        private Array2D _edgesPoints;

        private int _threshold = 25;
        
        public EdgeDetector(CanvasBGRA bmp)
        {
            _originalImage = bmp;
            _edgesPoints = new Array2D(bmp.WidthPixel , bmp.HeightPixel);
        }

        public void SaveBitmapHSL(string filename, bool h, bool s, bool l)
        {
            Bitmap bmp = new Bitmap(_originalImage.WidthPixel, _originalImage.HeightPixel, PixelFormat.Format32bppPArgb);
            var lockBmp2 = bmp.LockBits(new Rectangle(0, 0, _originalImage.WidthPixel, _originalImage.HeightPixel), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            byte [] origData = this._originalImage.Data;

            unsafe
            {
                byte * ll = (byte*)lockBmp2.Scan0.ToPointer();
                int countPoints = _originalImage.CountPixels;
                int bmpIndex = 0;
                for (int index = 0; index < countPoints; index++)
                {

                    HSLColor hlsColor = new HSLColor(
                        origData[bmpIndex+2],origData[bmpIndex+1],origData[bmpIndex]);
                    
                    if(h)  ll[bmpIndex] = (byte)hlsColor.Hue;
                    else if (s) ll[bmpIndex] = (byte)hlsColor.Saturation;
                    else if (l) ll[bmpIndex] = (byte)hlsColor.Luminosity;

                        ll[bmpIndex + 1] = 0;
                        ll[bmpIndex + 2] = 0;

                    bmpIndex += 4;
                }
            }

            bmp.UnlockBits(lockBmp2);
           
            bmp.Save(filename, ImageFormat.Bmp);
        }

        public void SaveEdgesAsBitmap(string filename)
        {
            Bitmap bmp = new Bitmap(_originalImage.WidthPixel, _originalImage.HeightPixel, PixelFormat.Format32bppPArgb);
            var lockBmp2 = bmp.LockBits(new Rectangle(0, 0, _originalImage.WidthPixel, _originalImage.HeightPixel), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            unsafe
            {
                byte * ll = (byte*)lockBmp2.Scan0.ToPointer();
                int bmpIndex = 0;
                for (int index = 0; index <_edgesPoints.Length; index++)
                {
                    if(_edgesPoints.Data[index] != 0)
                    {
                        ll[bmpIndex] = 255;
                        ll[bmpIndex+1] = 0;
                        ll[bmpIndex+2] = 0;

                    }

                    bmpIndex += 4;
                }
            }

            bmp.UnlockBits(lockBmp2);
            bmp.Save(filename, ImageFormat.Bmp);
        }

        public void DetectEdges(int threshold)
        {
            _threshold = threshold;

            Array.Clear(_edgesPoints.Data, 0, _edgesPoints.Length);
            //leftRunFindEdgesByHSLBetter();
            //DownRunFindEdgesByHSLBetter();


            //leftRunFindEdgesRGB();
            //DownRunFindEdgesRGB();

            leftRunFindEdgesRGBAdvance();
            DownRunFindEdgesRGBAdvance();


            ReduceOnePointNoise();
            ReduceTwoPointNoise();
            SetEdgesFrame();
            //SetCornerEdgesFrame();
            
        }

        public ImageEdges GetAllEdgesPoints()
        {
            ImageEdges result = new ImageEdges(this._edgesPoints.Width, this._edgesPoints.Height); 

            List<DnaPoint> epoints = new List<DnaPoint>();
            List<DnaPoint> epointsByY = new List<DnaPoint>();


            int indexRow = 0;
            for (int y = 0; y < _edgesPoints.Height; y++)
            {
                for (int x = 0; x < _edgesPoints.Width; x++)
                {
                    int index = indexRow + x;
                    if (_edgesPoints.Data[index] != 0)
                    {
                        epoints.Add(new DnaPoint((short)(index % _edgesPoints.Width), (short)(index / _edgesPoints.Width)));
                        epointsByY.Add(new DnaPoint((short)(index % _edgesPoints.Width), (short)(index / _edgesPoints.Width)));
                    }
                }

                result.EdgePointsByY[y] = epointsByY.ToArray();
                epointsByY.Clear();

                indexRow += this._edgesPoints.Width;
            }

            result.EdgePoints = epoints.ToArray();

            List<DnaPoint> epointsByX = new List<DnaPoint>();

            for (int x = 0; x < _edgesPoints.Width; x++)
            {
                int index = x;
                for (int y = 0; y < _edgesPoints.Height; y++)
                {
                    if (_edgesPoints.Data[index] != 0)
                    {
                        epointsByX.Add(new DnaPoint((short)(index % _edgesPoints.Width), (short)(index / _edgesPoints.Width)));
                    }

                    index += _edgesPoints.Width;
                }

                result.EdgePointsByX[x] = epointsByX.ToArray();
                epointsByX.Clear();
            }

            Array.Copy(this._edgesPoints.Data, result.EdgesPoints2D.Data, result.EdgesPoints2D.Length);

            if (epoints.Count == 0)
                return null;
            else
              return result ;
        }

        /// <summary>
        /// all points around image are set as edge points
        /// </summary>
        private void SetEdgesFrame()
        {
            byte [] ep = this._edgesPoints.Data;

            int indexLastLine = _edgesPoints.Length - _edgesPoints.Width;
            for (int i = 0; i < _edgesPoints.Width; i++)
            {
                ep[i] = 1;
                ep[indexLastLine + i] = 1;
            }

            int indexleftLine = 0;
            int indexrightLine = _edgesPoints.Width - 1;
            for (int i = 0; i < _edgesPoints.Height; i++)
            {
                ep[indexleftLine] = 1;
                ep[indexrightLine] = 1;

                indexleftLine += _edgesPoints.Width;
                indexrightLine += _edgesPoints.Width;
            }

        }

        /// <summary>
        /// all points around image are set as edge points
        /// </summary>
        private void SetCornerEdgesFrame()
        {
            int indexLastLine = _edgesPoints.Length - _edgesPoints.Width;
            _edgesPoints.Data[0] = 1;
            _edgesPoints.Data[_edgesPoints.Width - 1] = 1;
            _edgesPoints.Data[indexLastLine] = 1;
            _edgesPoints.Data[_edgesPoints.Length - 1] = 1;
            
        }

        private void ReduceOnePointNoise()
        {
            int upRowIndex = 0;
            int midRowIndex = this._edgesPoints.Width;
            int downRowIndex = this._edgesPoints.Width * 2;

            byte [] ep = this._edgesPoints.Data;

            for (int y = 1; y < this._edgesPoints.Height - 1; y++)
            {
                int upIndex = upRowIndex+1;
                int midIndex = midRowIndex+1;
                int downIndex = downRowIndex+1;

                for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                {
                    if (ep[upIndex - 1] == 0 && ep[upIndex] == 0 && ep[upIndex + 1] == 0 &&
                        ep[midIndex - 1] == 0 && ep[midIndex] == 1 && ep[midIndex + 1] == 0 &&
                        ep[downIndex - 1] == 0 && ep[downIndex] == 0 && ep[downIndex + 1] == 0
                        )
                    {
                        ep[midIndex] = 0;
                    }


                    upIndex++;
                    midIndex++;
                    downIndex++;
                }

                upRowIndex += this._edgesPoints.Width;
                midRowIndex += this._edgesPoints.Width;
                downRowIndex += this._edgesPoints.Width;



            }
        }

        private void ReduceTwoPointNoise()
        {
            int upRowIndex = 0;
            int midRowIndex = this._edgesPoints.Width;
            int midRowIndex2 = this._edgesPoints.Width * 2;
            int downRowIndex = this._edgesPoints.Width * 3;

            byte [] ep = this._edgesPoints.Data;

            for (int y = 1; y < this._edgesPoints.Height - 2; y++)
            {
                int upIndex = upRowIndex + 1;
                int midIndex = midRowIndex + 1;
                int midIndex2 = midRowIndex2 + 1;
                int downIndex = downRowIndex + 1;

                for (int x = 1; x < this._edgesPoints.Width - 2; x++)
                {
                    if (ep[upIndex - 1] == 0 && ep[upIndex] == 0 && ep[upIndex + 1] == 0 && ep[upIndex + 2] == 0 &&
                        ep[midIndex - 1] == 0 && 
                        (ep[midIndex] == 1 || ep[midIndex + 1] == 1 || ep[midIndex2] == 1 || ep[midIndex2 + 1] == 1) && 
                        ep[midIndex + 2] == 0 &&
                        ep[midIndex2 - 1] == 0 &&  ep[midIndex2 + 2] == 0 &&
                        ep[downIndex - 1] == 0 && ep[downIndex] == 0 && ep[downIndex + 1] == 0 && ep[downIndex + 2] == 0
                        )
                    {
                        ep[midIndex] = 0; ep[midIndex+1] = 0;
                        ep[midIndex2] = 0; ep[midIndex2 + 1] = 0;
                    }


                    upIndex++;
                    midIndex++;
                    midIndex2++;

                    downIndex++;
                }

                upRowIndex += this._edgesPoints.Width;
                midRowIndex += this._edgesPoints.Width;
                midRowIndex2 += this._edgesPoints.Width;
                downRowIndex += this._edgesPoints.Width;



            }
        }

        private void leftRunFindEdgesRGB()
        {
            byte [] origData = this._originalImage.Data;


            for (int yIndex = 0; yIndex < _edgesPoints.Length; yIndex += this._edgesPoints.Width)
            {
                int endLineIndex = yIndex + this._edgesPoints.Width;
                int origIndex = yIndex * 4;

                int startR = origData[origIndex] ;
                int startG = origData[origIndex + 1] ;
                int startB = origData[origIndex + 2] ;

                origIndex += 4;

                for (int edgeIndex = yIndex + 1; edgeIndex < endLineIndex; edgeIndex++)
                {
                    int br = origData[origIndex];
                    int bg = origData[origIndex + 1];
                    int bb = origData[origIndex + 2];

                    if (!(Tools.fastAbs(br - startR) < _threshold &&
                   Tools.fastAbs(bg - startG) < _threshold &&
                   Tools.fastAbs(bb - startB) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                    }

                    startB = bb;
                    startG = bg;
                    startR = br;


                    origIndex += 4;

                }
            }
        }

        private void leftRunFindEdgesRGBAdvance()
        {
            byte [] origData = this._originalImage.Data;


            for (int yIndex = 0; yIndex < _edgesPoints.Length; yIndex += this._edgesPoints.Width)
            {
                int endLineIndex = yIndex + this._edgesPoints.Width;
                int origIndex = yIndex * 4;

                int startR = origData[origIndex];
                int startG = origData[origIndex + 1];
                int startB = origData[origIndex + 2];

                origIndex += 4;

                for (int edgeIndex = yIndex + 1; edgeIndex < endLineIndex; edgeIndex++)
                {
                    int br = origData[origIndex];
                    int bg = origData[origIndex + 1];
                    int bb = origData[origIndex + 2];

                    if (!(Tools.fastAbs(br - startR) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        //startR = br;
                        startR = origData[origIndex-4];
                    }

                    if (!(Tools.fastAbs(bg - startG) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        startG = bg;
                        startG = origData[origIndex-4 + 1];
                    }

                    if (!(Tools.fastAbs(bb - startB) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        startB = bb;
                        startB = origData[origIndex-4 + 2];
                    }


                   // if (!(Tools.fastAbs(br - startR) < _threshold &&
                   //Tools.fastAbs(bg - startG) < _threshold &&
                   //Tools.fastAbs(bb - startB) < _threshold))
                   // {
                   //     _edgesPoints.Data[edgeIndex] = 1;

                   //     startB = bb;
                   //     startG = bg;
                   //     startR = br;
                   // }

                   

                    origIndex += 4;

                }
            }
        }

        private void DownRunFindEdgesRGB()
        {
            byte [] origData = this._originalImage.Data;
            int bmpRowLength = _originalImage.Width;

            int origIndex = 0;
            int edgeIndex = 0;

            for (int x = 0; x < _originalImage.WidthPixel; x++)
            {
                origIndex = x * 4;
                edgeIndex = x;

                int startR = origData[origIndex];
                int startG = origData[origIndex + 1];
                int startB = origData[origIndex + 2];

                edgeIndex += this._edgesPoints.Width;
                origIndex += bmpRowLength;

                for (int y = 1; y < _originalImage.HeightPixel - 1; y++)
                {
                    int br = origData[origIndex];
                    int bg = origData[origIndex + 1];
                    int bb = origData[origIndex + 2];

                    if (!(Tools.fastAbs(br - startR) < _threshold &&
              Tools.fastAbs(bg - startG) < _threshold &&
              Tools.fastAbs(bb - startB) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                    }

                    startB = bb;
                    startG = bg;
                    startR = br;

                    edgeIndex += this._edgesPoints.Width;
                    origIndex += bmpRowLength;
                }
            }
        }

        private void DownRunFindEdgesRGBAdvance()
        {
            byte [] origData = this._originalImage.Data;
            int bmpRowLength = _originalImage.Width;

            int origIndex = 0;
            int edgeIndex = 0;

            for (int x = 0; x < _originalImage.WidthPixel; x++)
            {
                origIndex = x * 4;
                edgeIndex = x;

                int startR = origData[origIndex];
                int startG = origData[origIndex + 1];
                int startB = origData[origIndex + 2];

                edgeIndex += this._edgesPoints.Width;
                origIndex += bmpRowLength;

                for (int y = 1; y < _originalImage.HeightPixel - 1; y++)
                {
                    int br = origData[origIndex];
                    int bg = origData[origIndex + 1];
                    int bb = origData[origIndex + 2];

                    if (!(Tools.fastAbs(br - startR) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        //startR = br;
                        startR = origData[origIndex - bmpRowLength];
                    }

                    if (!(Tools.fastAbs(bg - startG) < _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        //startG = bg;
                        startG = origData[origIndex - bmpRowLength+1];
                 
                    }

                    if (!(Tools.fastAbs(bb - startB) < _threshold)) 
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        //startB = bb;
                        startB = origData[origIndex - bmpRowLength+2];
                 
                    }

              //      if (!(Tools.fastAbs(br - startR) < _threshold &&
              //Tools.fastAbs(bg - startG) < _threshold &&
              //Tools.fastAbs(bb - startB) < _threshold))
              //      {
              //          _edgesPoints.Data[edgeIndex] = 1;

              //          startB = bb;
              //          startG = bg;
              //          startR = br;
              //      }

                   

                    edgeIndex += this._edgesPoints.Width;
                    origIndex += bmpRowLength;
                }
            }
        }


        private void leftRunFindEdgesByHSL()
        {


            byte [] origData = this._originalImage.Data;
            int origIndex = 0;
            int edgeIndex = 0;
            
            while (edgeIndex < (_edgesPoints.Length - 1))
            {
                HSLColor hlsColor = new HSLColor(
                    origData[origIndex + 2], origData[origIndex + 1], origData[origIndex]);
                HSLColor hlsColor2 = new HSLColor(
                    origData[origIndex + 6], origData[origIndex + 5], origData[origIndex + 4]);


                if ((Math.Abs(hlsColor.Luminosity - hlsColor2.Luminosity) > _threshold))
                {
                    _edgesPoints.Data[edgeIndex] = 1;
                }

                origIndex += 4;
                edgeIndex++;
            }


        }

        private void leftRunFindEdgesByHSLBetter()
        {


            byte [] origData = this._originalImage.Data;
           

            for (int yIndex = 0; yIndex < _edgesPoints.Length; yIndex += this._edgesPoints.Width)
            {
                int endLineIndex = yIndex + this._edgesPoints.Width;
                int origIndex = yIndex * 4;

                HSLColor startBlockColor = new HSLColor(
                    origData[origIndex + 2], origData[origIndex + 1], origData[origIndex]);
                origIndex += 4;

                for (int edgeIndex = yIndex + 1; edgeIndex < endLineIndex; edgeIndex++)
                {
                    HSLColor hlsColor = new HSLColor(
                        origData[origIndex + 2], origData[origIndex + 1], origData[origIndex]);


                    if ((Math.Abs(startBlockColor.Luminosity - hlsColor.Luminosity) > _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        startBlockColor = hlsColor;
                    }

                    origIndex += 4;

                }
            }
        }

        private void DownRunFindEdgesByHSL()
        {


            byte [] origData = this._originalImage.Data;
            int origIndex = 0;
            int edgeIndex = 0;

            int bmpRowLength = _originalImage.Width;
            
            for (int x = 0; x < _originalImage.WidthPixel; x++)
            {
                origIndex = x * 4;
                edgeIndex = x;
                for (int y = 0; y < _originalImage.HeightPixel - 1; y++)
                {
                    HSLColor hlsColor = new HSLColor(
                   origData[origIndex + 2], origData[origIndex + 1], origData[origIndex]);
                    HSLColor hlsColor2 = new HSLColor(
                        origData[origIndex + bmpRowLength], origData[origIndex + bmpRowLength + 1], origData[origIndex + bmpRowLength + 2]);


                    if ((Math.Abs(hlsColor.Luminosity - hlsColor2.Luminosity) > _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                    }

                    edgeIndex += _edgesPoints.Width;
                    origIndex += bmpRowLength;
                }
            }
        }

        private void DownRunFindEdgesByHSLBetter()
        {

            byte [] origData = this._originalImage.Data;
            int bmpRowLength = _originalImage.Width;

            int origIndex = 0;
            int edgeIndex = 0;

            for (int x = 0; x < _originalImage.WidthPixel; x++)
            {
                origIndex = x * 4;
                edgeIndex = x;

                HSLColor startBlockColor = new HSLColor(
                   origData[origIndex + 2], origData[origIndex + 1], origData[origIndex]);

                edgeIndex += this._edgesPoints.Width;
                origIndex += bmpRowLength;

                for (int y = 1; y < _originalImage.HeightPixel - 1; y++)
                {
                    HSLColor hlsColor = new HSLColor(
                   origData[origIndex + 2], origData[origIndex + 1], origData[origIndex]);


                    if ((Math.Abs(startBlockColor.Luminosity - hlsColor.Luminosity) > _threshold))
                    {
                        _edgesPoints.Data[edgeIndex] = 1;
                        startBlockColor = hlsColor;
                    }

                    edgeIndex += this._edgesPoints.Width;
                    origIndex += bmpRowLength;
                }
            }

        }
    }



    
}
