using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenArt.Core.Classes
{
    class AreaVO
    {

    }

    public struct AreaSizeVO<T>
    {
        public T Width;
        public T Height;

        public AreaSizeVO(T width, T height)
        {
            this.Height = height;
            this.Width = width;
        }
    }
}
