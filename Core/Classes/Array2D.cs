using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenArt.Core.Classes
{
    public class Array2D
    {
        public byte [] Data;
        protected int _width;
        protected int _height;

        public Array2D(int width, int height)
        {
            this.Data = new byte[width * height];
            this._width = width;
            this._height = height;
        }

        int Width
        {
            get { return _width; }
        }

        int Height
        {
            get { return _height; }
        }
    }
}
