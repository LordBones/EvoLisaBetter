using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.Classes;

namespace GenArt.Core.AST
{
    public class DnaElipse : DnaPrimitive
    {
        private DnaPoint _startPoint;
        private short _width;
        private short _height;

        public DnaPoint StartPoint { get { return _startPoint; } set { _startPoint = value; } }
        public DnaPoint EndPoint{get{return new DnaPoint((short)(_startPoint.X+_width-1),(short)(_startPoint.Y+_height-1));}}
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
            elipse.StartPoint = _startPoint;
            elipse.Brush = Brush;
            elipse.UniqueId = UniqueId;

            return elipse;
        }

        public override int GetCountPoints()
        {
            return 1;
        }

        public override void Init(byte mutationRate, Classes.ErrorMatrix errorMatrix, Classes.ImageEdges edgePoints = null)
        {
            var origin = new DnaPoint();
            origin.Init();

            Rectangle tile = new Rectangle(0, 0, 1, 1);
            if (errorMatrix != null)
            {
                int matrixIndex = errorMatrix.GetRNDMatrixRouleteIndex();
                tile = errorMatrix.GetTileByErrorMatrixIndex(matrixIndex);

                origin.X = (short)(tile.X + Tools.GetRandomNumber(0, tile.Width));
                origin.Y = (short)(tile.Y + Tools.GetRandomNumber(0, tile.Height));
            }


            this._startPoint = origin;


            //int mutationMaxy = Math.Max(5, ( (Math.Min(Tools.MaxHeight- origin.Y - 1,20) )) );
            int mutationMaxy = Math.Max(5, ((mutationRate + 1) * (Math.Min(Tools.MaxHeight - origin.Y - 1, 20))) / (256));
            //int mutationMaxy = Math.Max(5, ((mutationRate + 1) * (Tools.MaxHeight - origin.Y - 1)) / (256));
            //int mutationMiddley = mutationMaxy / 2;

            //int mutationMaxx = Math.Max(5, ( (Math.Min(Tools.MaxWidth - origin.X - 1,20))) );
            int mutationMaxx = Math.Max(5, ((mutationRate + 1) * (Math.Min(Tools.MaxWidth - origin.X - 1, 20))) / (256));
            //int mutationMaxx = Math.Max(5, ((mutationRate + 1) * (Tools.MaxWidth - origin.X - 1)) / (256));
            //int mutationMiddlex = mutationMaxx / 2;

            var point = new DnaPoint();

            

            int tmp = Tools.GetRandomNumber(4, mutationMaxx);

            point.X = (short)Math.Min(origin.X + tmp, Tools.MaxWidth - 1);
            if (point.X == origin.X)
                tmp = Tools.GetRandomNumber(1, mutationMaxy);
            else
                tmp = Tools.GetRandomNumber(4, mutationMaxy);

            point.Y = (short)Math.Min(origin.Y + tmp, Tools.MaxHeight - 1);

            _width = (short)(point.X - origin.X + 1);
            _height = (short)(point.Y - origin.Y + 1);

            Brush = new DnaBrush(0, 255, 0, 0);
            CreateNewUniqueId();
        }

        public override bool IsPointInside(GenArt.AST.DnaPoint point)
        {
            //return true;

            float halfWidth = this._width / 2.0f;
            float halfHeight = this._height / 2.0f;

            float centerX = this._startPoint.X + halfWidth;
            float centerY = this._startPoint.Y + halfHeight;
             
            float normPointX = point.X - centerX;
            float normPointY = point.Y- centerY;


            return ((double)(normPointX * normPointX)
                     / (halfWidth * halfWidth)) + ((double)(normPointY * normPointY) / (halfHeight * halfHeight))
                <= 1.0;
        }

        public override bool IsLineCrossed(GenArt.AST.DnaPoint startLine, GenArt.AST.DnaPoint endLine)
        {
            //return true;
            DnaPoint endPoint = this.EndPoint;
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                this.StartPoint, new DnaPoint(endPoint.X, this.StartPoint.Y))) return true;
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                new DnaPoint(endPoint.X, this.StartPoint.Y), endPoint)) return true;
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                this.StartPoint, new DnaPoint(this.StartPoint.X, endPoint.Y))) return true;
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                new DnaPoint(this.StartPoint.X, endPoint.Y), endPoint)) return true;

            return false;
        }

        public override void Mutate(byte MutationRate, GenArt.AST.DnaDrawing drawing, Classes.CanvasBGRA destImage = null, Classes.ImageEdges edgePoints = null)
        {
            DnaPoint endPoint = new DnaPoint((short)(this._startPoint.X + this._width - 1),(short)( this._startPoint.Y + this._height - 1));
            DnaPoint point = new DnaPoint();

            int newValue = Tools.GetRandomChangeValue(endPoint.X, this.StartPoint.X, Tools.MaxWidth - 1, MutationRate);

            point.X = (short)Math.Max(this._startPoint.X, Math.Min(newValue, Tools.MaxWidth - 1));


            if (point.X == endPoint.X)
                newValue = Tools.GetRandomChangeValueGuaranted(endPoint.Y, this.StartPoint.Y, Tools.MaxHeight - 1, MutationRate);
            else
                newValue = Tools.GetRandomChangeValue(endPoint.Y, this.StartPoint.Y, Tools.MaxHeight - 1, MutationRate);

            point.Y = (short)Math.Max(this.StartPoint.Y, Math.Min(newValue, Tools.MaxHeight - 1));


            this._width = (short)(point.X - this._startPoint.X + 1);
            this._height = (short)(point.Y - this._startPoint.Y + 1);
            endPoint = point;

            point = new DnaPoint();


            newValue = Tools.GetRandomChangeValue(this.StartPoint.X, 0, endPoint.X, MutationRate);


            point.X = (short)Math.Max(0, Math.Min(newValue, endPoint.X));
            if (point.X == this.StartPoint.X)
                newValue = Tools.GetRandomChangeValueGuaranted(this.StartPoint.Y, 0, endPoint.Y, MutationRate);
            else
                newValue = Tools.GetRandomChangeValue(this.StartPoint.Y, 0, endPoint.Y, MutationRate);


            point.Y = (short)Math.Max(0, Math.Min(newValue, endPoint.Y));

            this.StartPoint = point;
            this._width = (short)(endPoint.X - point.X + 1);
            this._height = (short)(endPoint.Y - point.Y + 1);



            drawing.SetDirty();
            CreateNewUniqueId();
        }

        public override void MutateTranspozite(GenArt.AST.DnaDrawing drawing, Classes.CanvasBGRA destImage = null)
        {
            //throw new NotImplementedException();
        }
    }
}
