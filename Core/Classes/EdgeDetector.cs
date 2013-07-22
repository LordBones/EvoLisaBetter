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
    public class EdgeDetector
    {
        private Bitmap _originalBitmap = null;
        private byte [] _edgesPoints;

        public EdgeDetector(Bitmap bmp)
        {
            _originalBitmap = bmp;
            _edgesPoints = new byte[bmp.Width * bmp.Height];
        }

        public void SaveBitmapHSL(string filename, bool h, bool s, bool l)
        {
            Bitmap bmp = new Bitmap(_originalBitmap.Width, _originalBitmap.Height, PixelFormat.Format32bppPArgb);
            var lockBmp2 = bmp.LockBits(new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            var lockBmpOrig =_originalBitmap.LockBits(new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            unsafe
            {
                byte * ll = (byte*)lockBmp2.Scan0.ToPointer();
                byte * origPtr = (byte*)lockBmpOrig.Scan0.ToPointer();
                int countPoints = _originalBitmap.Width * _originalBitmap.Height;
                int bmpIndex = 0;
                for (int index = 0; index < countPoints; index++)
                {

                    HSLColor hlsColor = new HSLColor(
                        origPtr[bmpIndex+2],origPtr[bmpIndex+1],origPtr[bmpIndex]);
                    
                    if(h)  ll[bmpIndex] = (byte)hlsColor.Hue;
                    else if (s) ll[bmpIndex] = (byte)hlsColor.Saturation;
                    else if (l) ll[bmpIndex] = (byte)hlsColor.Luminosity;

                        ll[bmpIndex + 1] = 0;
                        ll[bmpIndex + 2] = 0;

                    bmpIndex += 4;
                }
            }

            bmp.UnlockBits(lockBmp2);
            _originalBitmap.UnlockBits(lockBmpOrig);
            bmp.Save(filename, ImageFormat.Bmp);
        }

        public void SaveEdgesAsBitmap(string filename)
        {
            Bitmap bmp = new Bitmap(_originalBitmap.Width, _originalBitmap.Height, PixelFormat.Format32bppPArgb);
            var lockBmp2 = bmp.LockBits(new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

            unsafe
            {
                byte * ll = (byte*)lockBmp2.Scan0.ToPointer();
                int bmpIndex = 0;
                for (int index = 0; index <_edgesPoints.Length; index++)
                {
                    if(_edgesPoints[index] != 0)
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

        public void DetectEdges()
        {
            Array.Clear(_edgesPoints, 0, _edgesPoints.Length);
            SetEdgesFrame();
            leftRunFindEdgesByHSL();
            DownRunFindEdgesByHSL();
            //leftRunFindEdges();

            //DownRunFindEdges();
        }

        public DnaPoint [] GetAllEdgesPoints()
        {
            List<DnaPoint> result = new List<DnaPoint>();
            for (int index = 0; index < _edgesPoints.Length; index++)
            {
                if (_edgesPoints[index] != 0)
                {
                    result.Add(new DnaPoint((short) (index % _originalBitmap.Width),(short) (index / _originalBitmap.Width)));
                }
            }

            if (result.Count == 0)
                return null;
            else
              return result.ToArray() ;
        }

        /// <summary>
        /// all points around image are set as edge points
        /// </summary>
        private void SetEdgesFrame()
        {
            int indexLastLine = _edgesPoints.Length - _originalBitmap.Width ; 
            for (int i = 0; i < _originalBitmap.Width; i++)
            {
                _edgesPoints[i] = 1;
                _edgesPoints[indexLastLine+i] = 1;
            }

            int indexleftLine = 0;
            int indexrightLine = _originalBitmap.Width - 1;
            for (int i = 0; i < _originalBitmap.Height; i++)
            {
                _edgesPoints[indexleftLine] = 1;
                _edgesPoints[indexrightLine] = 1;

                indexleftLine += _originalBitmap.Width;
                indexrightLine += _originalBitmap.Width;
            }

        }

        private void leftRunFindEdges()
        {
            BitmapData bmdSRC = _originalBitmap.LockBits(
                new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height), 
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            try
            {

                unsafe
                {

                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();
                    int origIndex = 0;
                    int edgeIndex = 0;
                    const int threshold = 32;
                    while (edgeIndex < (_edgesPoints.Length - 1))
                    {
                        int br = origPtr[origIndex] - origPtr[origIndex+4];
                        int bg = origPtr[origIndex+1] - origPtr[origIndex + 5];
                        int bb = origPtr[origIndex+2] - origPtr[origIndex + 6];

                        if (!(Tools.fastAbs(br) < threshold &&
                            Tools.fastAbs(bg) < threshold &&
                            Tools.fastAbs(bb) < threshold))
                        {
                            _edgesPoints[edgeIndex] = 1;
                        }

                        origIndex += 4;
                        edgeIndex++;
                    }

                }
            }
            finally
            {
                _originalBitmap.UnlockBits(bmdSRC);
            }
        }

        private void leftRunFindEdgesByHSL()
        {
            BitmapData bmdSRC = _originalBitmap.LockBits(
                new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            try
            {

                unsafe
                {

                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();
                    int origIndex = 0;
                    int edgeIndex = 0;
                    const double threshold = 20.0;
                    while (edgeIndex < (_edgesPoints.Length - 1))
                    {
                        HSLColor hlsColor = new HSLColor(
                            origPtr[origIndex+2], origPtr[origIndex + 1], origPtr[origIndex]);
                        HSLColor hlsColor2 = new HSLColor(
                            origPtr[origIndex + 6], origPtr[origIndex + 5], origPtr[origIndex+4]);

                       
                        if ((Math.Abs(hlsColor.Luminosity - hlsColor2.Luminosity) > threshold))
                        {
                            _edgesPoints[edgeIndex] = 1;
                        }

                        origIndex += 4;
                        edgeIndex++;
                    }

                }
            }
            finally
            {
                _originalBitmap.UnlockBits(bmdSRC);
            }
        }

         private void DownRunFindEdgesByHSL()
        {
            BitmapData bmdSRC = _originalBitmap.LockBits(
                new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            try
            {

                unsafe
                {

                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();
                    int origIndex = 0;
                    int edgeIndex = 0;
                  
                    int bmpRowLength = _originalBitmap.Width * 4;
                    const double threshold = 20.0;
                    for (int x = 0; x < _originalBitmap.Width; x++)
                    {
                        origIndex = x * 4;
                        edgeIndex = x;
                        for (int y = 0; y < _originalBitmap.Height-1; y++)
                        {
                             HSLColor hlsColor = new HSLColor(
                            origPtr[origIndex+2], origPtr[origIndex + 1], origPtr[origIndex]);
                        HSLColor hlsColor2 = new HSLColor(
                            origPtr[origIndex + bmpRowLength], origPtr[origIndex + bmpRowLength + 1], origPtr[origIndex + bmpRowLength + 2]);

                          
                            if ((Math.Abs(hlsColor.Luminosity - hlsColor2.Luminosity) > threshold))
                            {
                                _edgesPoints[edgeIndex] = 1;
                            }

                            edgeIndex += _originalBitmap.Width;
                            origIndex += bmpRowLength;
                        }
                    }
                }
            }
            finally
            {
                _originalBitmap.UnlockBits(bmdSRC);
            }
        }
    

        private void DownRunFindEdges()
        {
            BitmapData bmdSRC = _originalBitmap.LockBits(
                new Rectangle(0, 0, _originalBitmap.Width, _originalBitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            try
            {

                unsafe
                {

                    byte * origPtr = (byte*)bmdSRC.Scan0.ToPointer();
                    int origIndex = 0;
                    int edgeIndex = 0;
                    const int threshold = 32;
                    int bmpRowLength = _originalBitmap.Width * 4;

                    for (int x = 0; x < _originalBitmap.Width; x++)
                    {
                        origIndex = x * 4;
                        edgeIndex = x;
                        for (int y = 0; y < _originalBitmap.Height-1; y++)
                        {
                            int br = origPtr[origIndex] - origPtr[origIndex + bmpRowLength];
                            int bg = origPtr[origIndex + 1] - origPtr[origIndex + bmpRowLength +1];
                            int bb = origPtr[origIndex + 2] - origPtr[origIndex + bmpRowLength +2];

                            if (!(Tools.fastAbs(br) < threshold &&
                                Tools.fastAbs(bg) < threshold &&
                                Tools.fastAbs(bb) < threshold))
                            {
                                _edgesPoints[edgeIndex] = 1;
                            }

                            edgeIndex += _originalBitmap.Width;
                            origIndex += bmpRowLength;
                        }
                    }
                }
            }
            finally
            {
                _originalBitmap.UnlockBits(bmdSRC);
            }
        }
    }



    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double hue = 1.0;
        private double saturation = 1.0;
        private double luminosity = 1.0;

        private const double scale = 240.0;

        public double Hue
        {
            get { return hue * scale; }
            set { hue = CheckRange(value / scale); }
        }
        public double Saturation
        {
            get { return saturation * scale; }
            set { saturation = CheckRange(value / scale); }
        }
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            Color color = (Color)this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor.luminosity != 0)
            {
                if (hslColor.saturation == 0)
                    r = g = b = hslColor.luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.hue);
                    b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor.luminosity < 0.5)  //<=??
                temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
            else
                temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            HSLColor hslColor = new HSLColor();
            hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.luminosity = color.GetBrightness();
            hslColor.saturation = color.GetSaturation();
            return hslColor;
        }
        #endregion

        public void SetRGB(int red, int green, int blue)
        {
            HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
            this.hue = hslColor.hue;
            this.saturation = hslColor.saturation;
            this.luminosity = hslColor.luminosity;
        }

        public HSLColor() { }
        public HSLColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }
        public HSLColor(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }
        public HSLColor(double hue, double saturation, double luminosity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

    }
}
