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
        public DnaBrush Brush;

        public abstract DnaPoint[] Points
        {
            get;
        }

        public abstract DnaPrimitive Clone();
        public abstract int GetCountPoints();

        public abstract void Init(byte mutationRate, ErrorMatrix errorMatrix, ImageEdges edgePoints = null);

        public abstract void GetRangeHighSize(ref int startY, ref int endY);
        public abstract bool GetRangeWidthByRow(int y, ref int startx, ref int endx);

        public abstract bool IsPointInside(DnaPoint point);
        public abstract bool IsLineCrossed(DnaPoint startLine, DnaPoint endLine);

        public abstract void Mutate(byte MutationRate, DnaDrawing drawing, CanvasARGB destImage = null, ImageEdges edgePoints = null);
        public abstract void MutateTranspozite(DnaDrawing drawing, CanvasARGB destImage = null);

        
    }
}
