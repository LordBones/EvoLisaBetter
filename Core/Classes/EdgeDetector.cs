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
            leftRunFindEdges();
        }

        public DnaPoint [] GetAllEdgesPoints()
        {
            List<DnaPoint> result = new List<DnaPoint>();
            for (int index = 0; index < _edgesPoints.Length; index++)
            {
                if (_edgesPoints[index] != 0)
                {
                    result.Add(new DnaPoint((short) (index % _originalBitmap.Width),(short) (index / _originalBitmap.Height)));
                }
            }
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
    }
}
