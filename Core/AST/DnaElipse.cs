using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;

namespace GenArt.Core.AST
{
    public class DnaElipse : DnaPrimitive
    {
        private DnaPoint _middle;
        private short _width;
        private short _height;

        public DnaPoint Middle { get { return _middle; } set { _middle = value; } }
        public short Width { get { return _width; } set { _width = value; } }
        public short Height { get { return _height; } set {_height = value;} }

        public DnaElipse()
        {

        }

        public override GenArt.AST.DnaPoint[] Points
        {
            get { return new DnaPoint[0]; }
        }

        public override object Clone()
        {
            DnaElipse elipse = new DnaElipse();
            elipse.Width = _width;
            elipse.Height = _height;
            elipse.Middle = _middle;
            elipse.Brush = Brush;
            elipse.UniqueId = UniqueId;

            throw new NotImplementedException();
        }

        public override int GetCountPoints()
        {
            return 1;
        }

        public override void Init(byte mutationRate, Classes.ErrorMatrix errorMatrix, Classes.ImageEdges edgePoints = null)
        {
            throw new NotImplementedException();
        }

        public override bool IsPointInside(GenArt.AST.DnaPoint point)
        {
            throw new NotImplementedException();
        }

        public override bool IsLineCrossed(GenArt.AST.DnaPoint startLine, GenArt.AST.DnaPoint endLine)
        {
            throw new NotImplementedException();
        }

        public override void Mutate(byte MutationRate, GenArt.AST.DnaDrawing drawing, Classes.CanvasBGRA destImage = null, Classes.ImageEdges edgePoints = null)
        {
            throw new NotImplementedException();
        }

        public override void MutateTranspozite(GenArt.AST.DnaDrawing drawing, Classes.CanvasBGRA destImage = null)
        {
            throw new NotImplementedException();
        }
    }
}
