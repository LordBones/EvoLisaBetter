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

        public DnaPoint GetRandomBorderPoint(int startX, int startY)
        {
            int maxX = this.Width - 1 - startX;
            int minX = -(this.Width - maxX-1);

            int maxY = this.Height - 1 - startY;
            int minY = -(this.Height - maxY - 1);

            int angle = Tools.GetRandomNumber(0, 360);
            double pomer = Math.Tan((angle / 180.0) * Math.PI);

            int x = (int)(startY / pomer);
            //check up
            if (x >= minX && x <= maxX && angle <= 180)
            {
                return new DnaPoint((short)(startX + x), (short)0);
            }

            // check down
            x = (int)((startY- (this.Height-1)) / pomer);
            //check up
            if (x >= minX && x <= maxX && angle > 180)
            {
                return new DnaPoint((short)(startX + x), (short)(this.Height-1));
            }

            int y = (int)(startX * pomer);
            //check up
            if (y >= minY && y <= maxY && angle > 90 && angle < 270)
            {
                return new DnaPoint((short)0,(short)(startY + y));
            }

            // check down
            y = (int)((startX-(this.Width-1)) * pomer);
            //check up
            if (y >= minY && y <= maxY && (angle <= 90 || angle >=270 ))
            {
                return new DnaPoint((short)(this.Width - 1),(short)(startY + y) );
            }

            throw new Exception("Sem se to nema nikdy dostat");
            
        }

        public bool IsSomeEdgeOnLineNoStartEndPoint(int startX, int startY, int endX, int endY)
        {
            DnaPoint ? point = GetFirstEdgeOnLineDirection(startX, startY, endX, endY);

            if (!point.HasValue)
                return false;
            else
            {
                return point.Value.X != endX && point.Value.Y != endY;
            }
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

            int longest = Tools.fastAbs(w);
            int shortest = Tools.fastAbs(h);
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
        private const int CONST_EdgeDirection_NotDefine = -1;
        private const int CONST_EdgeDirection_Vertical = 0;
        private const int CONST_EdgeDirection_Horizontal = 90;
        private const int CONST_EdgeDirection_LeftDown = 45;
        private const int CONST_EdgeDirection_LeftUp = 135;

        private byte [] gauseKernel = new byte[]
        {0,1,2,1,0,
         1,4,8,4,1,
         2,8,16,8,2,
         1,4,8,4,1,
         0,1,2,1,0
        };

        private int [] gauseKernel2 = new int[]
        {0,-1,-2,-1,0,
         -1,-4,-8,-4,-1,
         -2,-8,+64,-8,-2,
         -1,-4,-8,-4,-1,
         -0,-1,-2,-1,-0
        };

        private CanvasBGRA _originalImage = null;
        private Array2D _edgesPoints;
        private Array2D _imageGreyscale = null;
        private int [] _kernelSums = null;
        private int [] _kernelDirection = null;

        private byte [] _colourArea = null;
        private byte [] _finalEdges = null;

        private int _threshold = 25;
        
        public EdgeDetector(CanvasBGRA bmp)
        {
            _originalImage = bmp;
            _edgesPoints = new Array2D(bmp.WidthPixel , bmp.HeightPixel);
            _imageGreyscale = new Array2D(bmp.WidthPixel, bmp.HeightPixel);
            _kernelSums = new int[_imageGreyscale.Length];
            _kernelDirection = new int[_imageGreyscale.Length];
            _colourArea = new byte[_imageGreyscale.Length];
            _finalEdges = new byte[_imageGreyscale.Length];

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

                        ll[bmpIndex + 1] = ll[bmpIndex];
                        ll[bmpIndex + 2] = ll[bmpIndex];

                    bmpIndex += 4;
                }
            }

            bmp.UnlockBits(lockBmp2);
           
            bmp.Save(filename, ImageFormat.Bmp);
        }

        public void SaveImageGreyscaleAsBitmap(string filename)
        {

            SaveGreyscaleAsBitmap(filename);
            
        }

        public void SaveImageEdgeDirectionsAsBitmap(string filename)
        {
            CanvasBGRA canvas = new CanvasBGRA(_imageGreyscale.Width, _imageGreyscale.Height);

            int imageIndex = 0;
            for (int index = 0; index < _imageGreyscale.Length; index++)
            {
                int data = _kernelDirection[index];
                if (data == 0) // vertical
                {
                    canvas.Data[imageIndex] = 255;
                    canvas.Data[imageIndex + 1] = 255;
                    canvas.Data[imageIndex + 2] = 255;
                }
                if (data == 45) // leftdown
                {
                    canvas.Data[imageIndex] = 0;
                    canvas.Data[imageIndex + 1] = 0;
                    canvas.Data[imageIndex + 2] = 255;
                }
                if (data == 90) // horizontal
                {
                    canvas.Data[imageIndex] = 0;
                    canvas.Data[imageIndex + 1] = 0;
                    canvas.Data[imageIndex + 2] = 0;
                }
                if (data == 135) // leftup
                {
                    canvas.Data[imageIndex] = 0;
                    canvas.Data[imageIndex + 1] = 255;
                    canvas.Data[imageIndex + 2] = 0;
                }

                canvas.Data[imageIndex + 3] = 255;
               
                imageIndex += 4;
            }

            using (Bitmap bmp = CanvasBGRA.CreateBitmpaFromCanvas(canvas))
            {
                bmp.Save(filename);
            }

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
            Array.Clear(_imageGreyscale.Data, 0, _edgesPoints.Length);
            Array.Clear(_kernelSums, 0, _kernelSums.Length);
            Array.Clear(_kernelDirection, 0, _kernelDirection.Length);
            Array.Clear(_colourArea, 0, _colourArea.Length);
            Array.Clear(_finalEdges, 0, _finalEdges.Length);



            ConvertToGreyScale();
            
            ReduceNoiseGausianGreyscale();
            NewDetectEdges((byte)threshold);

            //LeftDetectEdgeNew();
            //DetectEdgeByKernelSum();
            //MakeThinEdgesMyLeftRight();
            //MakeThinEdgesMyUpDown();

            //MakeThinEdges();
            //UpDownDetectEdgeNew();

        


            //ReduceOnePointNoise();
            //ReduceTwoPointNoise();
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
                        DnaPoint p = new DnaPoint((short)x, (short)y);
                        epoints.Add(p);
                        epointsByY.Add(p);
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
                        epointsByX.Add(new DnaPoint((short)x, (short)y));
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

        #region edge detector by area

        private void NewDetectEdges(byte colourTolerance)
        {
            Array.Clear(_finalEdges, 0, _finalEdges.Length);

            int min = 0;
            while (min < 256)
            {
                int max = (min + colourTolerance < 256) ? min + colourTolerance : 255; 
                DetectColoursInRange((byte)(min), (byte)(max));
                DetectExtractEdges();
                CopyFinalColorIntoEdges();

                min += colourTolerance+1;
            }
        }

        private void DetectColoursInRange(byte minColor, byte maxColor)
        {
            Array.Clear(_colourArea, 0, _colourArea.Length);

            for (int index = 0; index < this._imageGreyscale.Length; index++)
            {
                byte tmp = this._imageGreyscale.Data[index];
                if (minColor <= tmp && tmp <= maxColor)
                    this._colourArea[index] = 1;
            }
        }

        private void DetectExtractEdges()
        {
            int upRowIndex = 0;
            int midRowIndex = this._imageGreyscale.Width;
            int downRowIndex = this._imageGreyscale.Width * 2;

            byte [] ca = this._colourArea;


            for (int y = 1; y < this._imageGreyscale.Height - 1; y++)
            {
                int upIndex = upRowIndex + 1;
                int midIndex = midRowIndex + 1;
                int downIndex = downRowIndex + 1;

                for (int x = 1; x < this._imageGreyscale.Width - 1; x++)
                {
                    if (ca[midIndex] == 1 && !(
                        ca[upIndex] == 1 &&
                        ca[midIndex - 1] == 1 && ca[midIndex + 1] == 1 &&
                        ca[downIndex] == 1 
                        ))
                    {
                        if ((ca[upIndex] == 0 && _finalEdges[upIndex] == 0) ||
                            (ca[downIndex] == 0 && _finalEdges[downIndex] == 0) ||
                            (ca[midIndex-1] == 0 && _finalEdges[midIndex-1] == 0) ||
                            (ca[midIndex+1] == 0 && _finalEdges[midIndex+1] == 0) 
                            )

                        this._finalEdges[midIndex] = 1;
                    }

                    upIndex++;
                    midIndex++;
                    downIndex++;
                }

                upRowIndex += this._imageGreyscale.Width;
                midRowIndex += this._imageGreyscale.Width;
                downRowIndex += this._imageGreyscale.Width;
            }
        }

        private void CopyFinalColorIntoEdges()
        {
            for (int index = 0; index < this._finalEdges.Length; index++)
            {
                byte tmp = this._finalEdges[index];
                if (tmp == 1)
                    this._edgesPoints.Data[index] = tmp;
            }
        }

        #endregion

        #region better edge detection

        private void ConvertToGreyScale()
        {
            int coefBonus = 32;
            int imageIndex = 0;
            for (int index = 0; index < _imageGreyscale.Length; index++)
            {
                //_imageGreyscale.Data[index] = (byte)((_originalImage.Data[imageIndex] * 8 +
                //_originalImage.Data[imageIndex + 1] * 71 +
                //_originalImage.Data[imageIndex + 2] * 21) / 100);
                int r = _originalImage.Data[imageIndex + 2];
                int g = _originalImage.Data[imageIndex + 1];
                int b = _originalImage.Data[imageIndex];
                //r = (r < 128 ? Math.Max(r - coefBonus, 0) : Math.Min(r + coefBonus, 255));
                //g = (g < 128 ? Math.Max(g - coefBonus, 0) : Math.Min(g + coefBonus, 255));
                //b = (b < 128 ? Math.Max(b - coefBonus, 0) : Math.Min(b + coefBonus, 255));

                //r = (r < 128 ? Math.Max(r-(128- r) , 0) : Math.Min(r -128 + r, 255));
                //g = (g < 128 ? Math.Max(g-(128-g ), 0) : Math.Min(g -128 +g, 255));
                //b = (b < 128 ? Math.Max(b-(128 -b), 0) : Math.Min(b-128+b, 255));


                _imageGreyscale.Data[index] = (byte)((b * 11 +
                    g  * 59 +
               r * 30) / 100);

                imageIndex += 4;
            }
        }

        private void SaveGreyscaleAsBitmap(string filename)
        {
            ReduceNoiseGausianGreyscale();
            CanvasBGRA canvas = new CanvasBGRA(_imageGreyscale.Width, _imageGreyscale.Height);

            int imageIndex = 0;
            for (int index = 0; index < _imageGreyscale.Length; index++)
            {
                byte data = _imageGreyscale.Data[index];
                canvas.Data[imageIndex] = data;
                canvas.Data[imageIndex+1] = data;
                canvas.Data[imageIndex+2] = data;
                canvas.Data[imageIndex + 3] = 255;

                imageIndex += 4;
            }

            using (Bitmap bmp = CanvasBGRA.CreateBitmpaFromCanvas(canvas))
            {
                bmp.Save(filename);
            }
        }

        private void LeftDetectEdgeNew()
        {
            int upRowIndex = 0;
            int midRowIndex = this._edgesPoints.Width;
            int downRowIndex = this._edgesPoints.Width * 2;

            byte [] ep = this._edgesPoints.Data;
            byte [] gs = this._imageGreyscale.Data;

            for (int y = 1; y < this._edgesPoints.Height - 1; y++)
            {
                int upIndex = upRowIndex + 1;
                int midIndex = midRowIndex + 1;
                int downIndex = downRowIndex + 1;

                for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                {
                    int sumY =
                        gs[upIndex - 1] * -1 + gs[upIndex] * -2 + gs[upIndex + 1] * -1 +
                        //gs[midIndex - 1] + gs[midIndex] + gs[midIndex + 1] +
                        gs[downIndex - 1] * 1 + gs[downIndex] * 2 + gs[downIndex + 1] * 1;

                    int sumX =
                        gs[upIndex - 1] * -1 + gs[upIndex] * 0 + gs[upIndex + 1] * 1 +
                        gs[midIndex - 1] * -2 + gs[midIndex] * 0 + gs[midIndex + 1] * 2 +
                        gs[downIndex - 1] * -1 + gs[downIndex] * 0 + gs[downIndex + 1] * 1;

                    int val = (int)Math.Sqrt(sumX*sumX + sumY*sumY);
                    _kernelSums[midIndex] = val;

                    int angle = (int)(Math.Atan2(-sumY ,sumX) * 180 / System.Math.PI);
                    angle %= 180;

                    if (angle < 0) angle = 180 + angle;

                    if (angle < 23 || angle >= 157) angle = CONST_EdgeDirection_Vertical;
                    else if (angle < 68 && angle >= 23) angle = CONST_EdgeDirection_LeftDown;
                    else if (angle < 113 && angle >= 68) angle = CONST_EdgeDirection_Horizontal;
                    else angle = CONST_EdgeDirection_LeftUp;

                    _kernelDirection[midIndex] = angle;

                    //if (val > _threshold*2)
                    //{
                    //    ep[midIndex] = 1;
                    //}

                    upIndex++;
                    midIndex++;
                    downIndex++;
                }

                upRowIndex += this._edgesPoints.Width;
                midRowIndex += this._edgesPoints.Width;
                downRowIndex += this._edgesPoints.Width;
            }
        }

        private void ComputeMaxMinThreshold(ref int maxThreshold, ref int minTrheshold)
        {
            Median8bit median = new Median8bit();

            int midRowIndex = this._edgesPoints.Width;
            
            int [] ks = _kernelSums;

            int thresholdMin = _threshold;
            int thresholdMax = _threshold + (_threshold / 5);

            for (int y = 1; y < this._edgesPoints.Height - 1; y++)
            {
                int midIndex = midRowIndex + 1;
            
                for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                {
                    byte val = (byte)Math.Min(Math.Max( ks[midIndex],0),255);

                    median.InsertData(val);
         
                    midIndex++;
                }

                midRowIndex += this._edgesPoints.Width;
            }

            minTrheshold = (int)median.Median ;
            maxThreshold = (int)(minTrheshold + median.StdDev + median.StdDev/2);
        }

        private void DetectEdgeByKernelSum()
        {
            int upRowIndex = 0;
            int midRowIndex = this._edgesPoints.Width;
            int downRowIndex = this._edgesPoints.Width * 2;

            byte [] ep = this._edgesPoints.Data;
            int [] ks = _kernelSums;

            int thresholdMin = _threshold;
            int thresholdMax = _threshold+(_threshold/5);

            ComputeMaxMinThreshold(ref thresholdMax, ref thresholdMin);

            for (int y = 1; y < this._edgesPoints.Height - 1; y++)
            {
                int upIndex = upRowIndex + 1;
                int midIndex = midRowIndex + 1;
                int downIndex = downRowIndex + 1;

                for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                {
                    if (_kernelSums[midIndex] >= thresholdMax)
                    {
                        ep[midIndex] = 1;
                    }
                    else if (_kernelSums[midIndex] >= thresholdMin)
                    {
                        int thr = thresholdMax;
                        if ((ks[upIndex - 1] > thr) ||
                            (ks[upIndex] > thr ) ||
                            (ks[upIndex + 1] > thr ))
                        {
                            ep[midIndex] = 1;
                        }

                        if ((ks[midIndex - 1] > thr) ||
                            (ks[midIndex + 1] > thr ))
                        {
                            ep[midIndex] = 1;
                        }
                        if ((ks[downIndex - 1] > thr ) ||
                            (ks[downIndex] > thr ) ||
                            (ks[downIndex + 1] > thr ))
                            
                        {
                            ep[midIndex] = 1;
                        }
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

        private void UpDownDetectEdgeNew()
        {
            int upRowIndex = 0;
            int midRowIndex = this._edgesPoints.Width;
            int downRowIndex = this._edgesPoints.Width * 2;

            byte [] ep = this._edgesPoints.Data;
            byte [] gs = this._imageGreyscale.Data;

            for (int y = 1; y < this._edgesPoints.Height - 1; y++)
            {
                int upIndex = upRowIndex + 1;
                int midIndex = midRowIndex + 1;
                int downIndex = downRowIndex + 1;

                for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                {
                    int sum =
                        gs[upIndex - 1] * -1 + gs[upIndex] * 0 + gs[upIndex + 1] * 1 +
                        gs[midIndex - 1]*-2 + gs[midIndex]*0 + gs[midIndex + 1]*2 +
                        gs[downIndex - 1] * -1 + gs[downIndex]*0 + gs[downIndex + 1] * 1;

                    if (sum > _threshold)
                    {
                        ep[midIndex] = 1;
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

        private void ReduceNoiseGausianGreyscale()
        {
            byte [] gauseValues = new byte[_imageGreyscale.Length];

            int rowIndex = this._edgesPoints.Width * 2;
            int rowStartKernelIndex = 0;
            
            
            byte [] gs = this._imageGreyscale.Data;

            for (int y = 2; y < this._edgesPoints.Height - 2; y++)
            {
                int index = rowIndex + 2;
                int rowKernelIndex = rowStartKernelIndex;
                for (int x = 2; x < this._edgesPoints.Width - 2; x++)
                {
                    int sum = 0;
                    int rowInsideKernelIndex = rowKernelIndex;
                    for(int ky = 0;ky < 5;ky++)
                    {
                        int insideKernelIndex = rowInsideKernelIndex; 

                        for (int kx =0; kx < 5; kx++)
                        {
                            sum += gs[insideKernelIndex] * gauseKernel[ky * 5 + kx];

                            insideKernelIndex++;
                        }

                        rowInsideKernelIndex += this._edgesPoints.Width;
                    }

                    gauseValues[index] = (byte)(sum/80);

                    rowKernelIndex++;
                    index++;
                }

                rowIndex += this._edgesPoints.Width;
                rowStartKernelIndex += this._edgesPoints.Width;
            }

            // update pixels
            int findexRow = this._edgesPoints.Width * 2;

            for (int y = 2; y < this._edgesPoints.Height - 2; y++)
            {
                int index = findexRow + 2;
                for (int x = 2; x < this._edgesPoints.Width - 2; x++)
                {
                    gs[index] = gauseValues[index];
                    index++;
                }

                findexRow += this._edgesPoints.Width;
            }
            
        }

        private void MakeThinEdgesMyLeftRight()
        {
            int rowIndex = this._edgesPoints.Width;
            
            byte [] ep = this._edgesPoints.Data;
            int [] kd = this._kernelDirection;

            for (int y = 1; y < this._edgesPoints.Height - 1; y++)
            {
                int index = rowIndex + 1;

                int lastDirection = CONST_EdgeDirection_NotDefine;
                for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                {
                    if (ep[index] == 1)
                    {
                        if (lastDirection != CONST_EdgeDirection_NotDefine)
                        {
                            if (CONST_EdgeDirection_Vertical == kd[index] && kd[index] == lastDirection)
                            {
                                ep[index] = 0;
                            }
                            else if (CONST_EdgeDirection_LeftDown == kd[index] && kd[index] == lastDirection)
                            {
                                ep[index] = 0;
                            }
                            else if (CONST_EdgeDirection_LeftUp == kd[index] && kd[index] == lastDirection)
                            {
                                ep[index] = 0;
                            }
                            else
                            {
                                lastDirection = kd[index];
                            }
                        }
                        else
                        {
                            lastDirection = kd[index];
                        }
                    }
                    else
                    {
                        lastDirection = CONST_EdgeDirection_NotDefine;
                    }

                    index++;               
                }

                rowIndex += this._edgesPoints.Width;
            }
        }

        private void MakeThinEdgesMyUpDown()
        {
            byte [] ep = this._edgesPoints.Data;
            int [] kd = this._kernelDirection;

            for (int x = 1; x < this._edgesPoints.Width - 1; x++)
            {
                int index = x;

                int lastDirection = CONST_EdgeDirection_NotDefine;
                for (int y = 1; y < this._edgesPoints.Height - 1; y++)
                {
                    if (ep[index] == 1)
                    {
                        if (lastDirection != CONST_EdgeDirection_NotDefine)
                        {
                            if (CONST_EdgeDirection_Horizontal == kd[index] && kd[index] == lastDirection)
                            {
                                ep[index] = 0;
                            }
                            else if (CONST_EdgeDirection_LeftDown == kd[index] && kd[index] == lastDirection)
                            {
                                ep[index] = 0;
                            }
                            else if (CONST_EdgeDirection_LeftUp == kd[index] && kd[index] == lastDirection)
                            {
                                ep[index] = 0;
                            }
                            else
                            {
                                lastDirection = kd[index];
                            }
                        }
                        else
                        {
                            lastDirection = kd[index];
                        }
                    }
                    else
                    {
                        lastDirection = CONST_EdgeDirection_NotDefine;
                    }

                    index += this._edgesPoints.Width;
                }
            }
        }


        private void MakeThinEdges()
        {
            List<int> indexForDelete = new List<int>();
            byte [] ep = this._edgesPoints.Data;


            do
            {

                for (int index  = 0; index < indexForDelete.Count; index++)
                    ep[indexForDelete[index]] = 0;

                indexForDelete.Clear();


                int upRowIndex = 0;
                int midRowIndex = this._edgesPoints.Width;
                int downRowIndex = this._edgesPoints.Width * 2;

                
                for (int y = 1; y < this._edgesPoints.Height - 1; y++)
                {
                    int upIndex = upRowIndex + 1;
                    int midIndex = midRowIndex + 1;
                    int downIndex = downRowIndex + 1;

                    for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                    {
                        int p1 = ep[midIndex];
                        int p2 = ep[upIndex];
                        int p3 = ep[upIndex+1];
                        int p4 = ep[midIndex+1];
                        int p5 = ep[downIndex+1];
                        int p6 = ep[downIndex];
                        int p7 = ep[downIndex-1];
                        int p8 = ep[midIndex-1];
                        int p9 = ep[upIndex-1];


                        int sum = p2+p3+p4+p5+p6+p7+p8+p9;
                        
                        int sumChange01 = (p2 < p3 ? 1 : 0) + (p3 < p4 ? 1 : 0) +
                                          (p4 < p5 ? 1 : 0) + (p5 < p6 ? 1 : 0) +
                                          (p6 > p7 ? 1 : 0) + (p7 > p8 ? 1 : 0) +
                                          (p8 > p9 ? 1 : 0) + (p9 > p2 ? 1 : 0);


                        if (p1 == 1 && (2 <= sum && sum <= 6) && sumChange01 == 1 &&
                            (p2 * p4 * p6) == 0 && (p4 * p6 * p8) == 0 && p7 != 0)
                        {
                            indexForDelete.Add(midIndex);
                        }

                        
                        upIndex++;
                        midIndex++;
                        downIndex++;
                    }

                    upRowIndex += this._edgesPoints.Width;
                    midRowIndex += this._edgesPoints.Width;
                    downRowIndex += this._edgesPoints.Width;
                }

                for (int index  = 0; index < indexForDelete.Count; index++)
                    ep[indexForDelete[index]] = 0;

                indexForDelete.Clear();


                upRowIndex = 0;
                midRowIndex = this._edgesPoints.Width;
                downRowIndex = this._edgesPoints.Width * 2;


                for (int y = 1; y < this._edgesPoints.Height - 1; y++)
                {
                    int upIndex = upRowIndex + 1;
                    int midIndex = midRowIndex + 1;
                    int downIndex = downRowIndex + 1;

                    for (int x = 1; x < this._edgesPoints.Width - 1; x++)
                    {
                        int p1 = ep[midIndex];
                        int p2 = ep[upIndex];
                        int p3 = ep[upIndex + 1];
                        int p4 = ep[midIndex + 1];
                        int p5 = ep[downIndex + 1];
                        int p6 = ep[downIndex];
                        int p7 = ep[downIndex - 1];
                        int p8 = ep[midIndex - 1];
                        int p9 = ep[upIndex - 1];


                        int sum = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;

                        int sumChange01 = (p2 < p3 ? 1 : 0) + (p3 < p4 ? 1 : 0) +
                                          (p4 < p5 ? 1 : 0) + (p5 < p6 ? 1 : 0) +
                                          (p6 > p7 ? 1 : 0) + (p7 > p8 ? 1 : 0) +
                                          (p8 > p9 ? 1 : 0) + (p9 > p2 ? 1 : 0);


                        if (p1 == 1 && (2 <= sum && sum <= 6) && sumChange01 == 1 &&
                            (p2 * p4 * p8) == 0 && (p2 * p6 * p8) == 0 && p7 != 0)
                        {
                            indexForDelete.Add(midIndex);
                        }


                        upIndex++;
                        midIndex++;
                        downIndex++;
                    }

                    upRowIndex += this._edgesPoints.Width;
                    midRowIndex += this._edgesPoints.Width;
                    downRowIndex += this._edgesPoints.Width;
                }

            } while (indexForDelete.Count > 0);
        }


        #endregion


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
    }




    public class EdgeDetectorInspiration 
{
	private int [] image;
	private int [] gradImage;
	private int [] dirImage; 
	private int imageSize, imageWidth;
	private int [] j;
	
	
	// the constructor, computes the gradient amplitudes and directions 
	// stores them in gradImage[] and dirImage[] arrays.
	
	public void EdgeDetector(int [] inPixels, int width, int height) 
	{
		imageSize  = inPixels.Length;
		imageWidth = width;
		gradImage  = new int[imageSize];
		dirImage   = new int[imageSize];
		image      = new int[imageSize];
		image      = inPixels;
		j          = new int[4];
		j[0] = 1;
		j[1] =-imageWidth;
		j[2] =-1;
		j[3] =imageWidth;
		
		int gradient;
		double direction;
		
		for (int x = 0; x < imageSize; x++)
		{
			if (x % width > 1 && x % width < width-1 && x > width && x < imageSize - width)
			{
				// the Sobel operator
				
				double dy = (inPixels[x-1]*-2 + inPixels[x+1]*2+
							inPixels [x-width-1]*-1 + inPixels[x-width+1] +
							inPixels [x+width-1]*-1 + inPixels[x-width+1]) / 8.0;
				
				double dx = (inPixels[x-width-1] + inPixels[x-width]*2+
							inPixels[x-width+1] + inPixels[x+width-1]*-1 +
							inPixels[x+width]*-2 + inPixels[x-width+1]*-1) / 8.0;
				
				gradient = (int)Math.Sqrt(dx*dx + dy*dy);
				
				
				if (dx == 0)
				{
					if (dy == 0) direction = 0.0;
					else direction = (dy > 0) ? Math.PI/2.0 : Math.PI/-2.0;
				}
				else if (dy == 0)
				{
					direction = 0.0;
				}
				else 
				{
					direction = Math.Atan(dy/dx); 
				}
				
				gradImage[x] = gradient;
				dirImage[x] = (int)(direction * 180.0 / Math.PI);		
			}
		}
	}	
	
	
	
	// Computes edges using both direction and amplitude thresholding
	
	public int[] getEdges(int gradThreshold, int direction)
	{
		int [] outPixels = new int[imageSize]; 
		outPixels = getEdges(gradThreshold);
		
		for (int x = 0; x < imageSize; x++)
				if ((dirImage[x] < direction-10) || (dirImage[x] > direction+10))
					outPixels[x] = 255;
					
		return outPixels;
	}
	
	
	
	// Computes edges by thresholding the amplitude of the gradient operator
	
	public int[] getEdges(int gradThreshold)
	{
		int [] outPixels = new int[imageSize]; 
		
		for (int x = 0; x < imageSize; x++)
		{
				if (gradImage[x] >= gradThreshold)
					outPixels[x] = 0;
				else
					outPixels[x] = 255;
		}
		return outPixels;
	}
	
	
	
	// Returns the gradient amplitudes. Can be viewed as an image
	
	public int[] getGradientImage()
	{
		return gradImage;
	}
	
	
	public int[] thinEdges()
	{
		int x,k,i;
		for (i = 0; i < imageSize; i++)
			if (image[i] > 5)
				image[i] = 1;
			else
				image[i] = 0;
		
		bool remain = true;
		while (remain)
		{
			remain = false;
			for (x = 0; x < 4; x++) 
			{
				for (i = imageWidth+1; i < imageSize-imageWidth-2; i++)
				{
					if ((image[i] == 1) && (image[i + j[x]] == 0))
						if (!patternMatch(i))					
							image[i] = 2;
						else
						{ image[i] = 3; remain = true;}
				}
				for (k=0; k<imageSize; k++)
					if (image[k] == 3)
						image[k] = 0;
			}
		}
		for (k=0; k<imageSize; k++)
			if (image[k] > 0)
				image[k] = 255;
		return image;
	}

	
	bool patternMatch(int i)
	{
		if (((image[i-imageWidth-1] > 0) && (image[i-imageWidth+1] > 0) && (image[i-imageWidth] > 0)) 
			||((image[i+imageWidth-1] > 0) && (image[i+imageWidth+1] > 0) && (image[i+imageWidth] > 0))) 
				return true;
		
		else if (((image[i-imageWidth-1] > 0) && (image[i-1] > 0) && (image[i+imageWidth-1] > 0))
			||((image[i-imageWidth+1] > 0) && (image[i+1] > 0) && (image[i+imageWidth+1] > 0)))
				return true;
				
		else if ((image[i-imageWidth-1] > 0)
			&& (image[i-imageWidth] > 0)
			&& (image[i-1] > 0)
			&& (image[i-imageWidth+1] == 0)
			&& (image[i+imageWidth-1] == 0)
			&& (image[i+imageWidth] == 0)
			&& (image[i+1] == 0)
			&& (image[i+imageWidth+1] == 0))
				return true;
				
		else if ((image[i+imageWidth-1] > 0)
			&& (image[i+imageWidth] > 0)
			&& (image[i-1] > 0)
			&& (image[i-imageWidth-1] == 0)
			&& (image[i+imageWidth+1] == 0)
			&& (image[i-imageWidth] == 0)
			&& (image[i+1] == 0)
			&& (image[i-imageWidth+1] == 0))
				return true;
				
		else if ((image[i-imageWidth+1] == 0)
			&& (image[i+1] > 0)
			&& (image[i+imageWidth+1] > 0)
			&& (image[i+imageWidth] > 0)
			&& (image[i+imageWidth-1] == 0)
			&& (image[i-imageWidth] == 0)
			&& (image[i-1] == 0)
			&& (image[i-imageWidth-1] == 0))
				return true;
		
		else if ((image[i-imageWidth-1] == 0)
			&& (image[i-imageWidth] > 0)
			&& (image[i-imageWidth+1] > 0)
			&& (image[i+1] > 0)
			&& (image[i+imageWidth+1] == 0)
			&& (image[i+imageWidth] == 0)
			&& (image[i-1] == 0)
			&& (image[i+imageWidth-1] == 0))
				return true;
		
		return false;
	}	

	public int[] linkEdges(int amp, int angle)
	{
		int x = 0;
		bool remain = true;

		for (int i = 0; i < imageSize-2; i++)
			if (image[i] == 255)
				for (x = 0; x < 4; x++)
					if ((Math.Abs(gradImage[i] - gradImage[i+j[x]]) < amp) && (Math.Abs(dirImage[i] - dirImage[i+j[x]]) < angle))
						{image [i] = 100; image[i+j[x]] = 100; }		
		return image;
	}
	
	public int[] removeShortEdges()
	{
		int i, k, length;
		bool remain, delete;
		
		for (i = 0; i < imageSize; i++)
			if (image[i] > 5)
				image[i] = 255;
			else
				image[i] = 0;
		
		for (i = imageWidth + 1; i < imageSize - imageWidth - 1; i++)
		{
			if (image[i] == 255)
			{	
				length = 0;
				length = traverse(i);
				if (length < 8)
				{
					image[i] = 0;
					deleteEdge(i);
				}
			}
		}
		return image;
	}
	
	private int traverse(int i)
	{
		int length = 1;
		for (int x = 0; x < 4; x++)
		{
			if (image[i + j[x]] == 255)
			{
				image[i + j[x]] = 250;
				length = length + traverse(i + j[x]);
			}
		}
		return length;
	}
	
	private void deleteEdge(int i)
	{
		for (int x = 0; x < 4; x++)
			if (image[i + j[x]] == 250)
			{
				image[i + j[x]] = 0;
				deleteEdge(i + j[x]);
			}
	}
	
}

    
}
