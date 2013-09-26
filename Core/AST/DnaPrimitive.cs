using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Core.Classes;

namespace GenArt.Core.AST
{
    public abstract class DnaPrimitive
    {
        private static long _globalUniqueId = 0;

        public int UniqueId;

        public DnaBrush Brush;

        public void CreateNewUniqueId()
        {
            this.UniqueId = (int)(_globalUniqueId % int.MaxValue);
            _globalUniqueId++;
        }

        public abstract DnaPoint[] Points
        {
            get;
        }

        public abstract object Clone();
        public abstract int GetCountPoints();

        public abstract void Init(ErrorMatrix errorMatrix, ImageEdges edgePoints = null);
        public abstract void Mutate(byte MutationRate, DnaDrawing drawing, CanvasBGRA destImage = null, ImageEdges edgePoints = null);
        public abstract void MutateTranspozite(DnaDrawing drawing, CanvasBGRA destImage = null);
        
    }
}
