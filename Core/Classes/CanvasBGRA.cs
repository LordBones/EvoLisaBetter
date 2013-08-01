using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenArt.Core.Classes
{
    public class CanvasBGRA : Array2D
    {
        public const int CONST_PixelSize = 4; 

        public CanvasBGRA(int pixelWidth, int pixelHeight):
            base(pixelWidth * CONST_PixelSize, pixelHeight * CONST_PixelSize)

        {

        }

        int WidthPixel
        {
            get { return this._width / 4; }
        }

        int HeightPixel
        {
            get { return this._height / 4; }
        }
    }
}
