﻿using System;
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
    public class DnaRectangle : DnaPrimitive
    {
        public DnaPoint StartPoint;
        public DnaPoint EndPoint;
      
        public DnaRectangle()
        {

        }

        public int Width
        {
            get { return this.EndPoint.X - this.StartPoint.X + 1; }
        }

        public int Height
        {
            get { return this.EndPoint.Y - this.StartPoint.Y + 1; }
        }


        public override DnaPoint[] Points
        {
            get{
                DnaPoint [] points = new DnaPoint[2];
                points[0] = this.StartPoint;
                points[1] = this.EndPoint;

                return points;
            }
        }


        public override object Clone()
        {
            DnaRectangle newObject = new DnaRectangle();
            newObject.Brush = this.Brush;
            newObject.EndPoint = this.EndPoint;
            newObject.StartPoint = this.StartPoint;
            newObject.UniqueId = this.UniqueId;

            return newObject;
        }

        public override int GetCountPoints()
        {
            return 2;
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


            this.StartPoint = origin;


            int mutationMaxy = Math.Max(2, ((mutationRate + 1) * (Tools.MaxHeight - origin.Y - 1)) / (256));
            //int mutationMiddley = mutationMaxy / 2;

            int mutationMaxx = Math.Max(2, ((mutationRate + 1) * (Tools.MaxWidth - origin.X - 1)) / (256));
            //int mutationMiddlex = mutationMaxx / 2;

            var point = new DnaPoint();



            int tmp = Tools.GetRandomNumber(0, mutationMaxx);

            point.X = (short)Math.Min(origin.X + tmp, Tools.MaxWidth - 1);
            if (tmp == 0)
                tmp = Tools.GetRandomNumber(1, mutationMaxy);
            else
                tmp = Tools.GetRandomNumber(0, mutationMaxy);

            point.Y = (short)Math.Min(origin.Y + tmp, Tools.MaxHeight - 1);

            this.EndPoint = point;

            Brush = new DnaBrush(0, 255, 0, 0);
            CreateNewUniqueId();
        }

        public override void Mutate(byte MutationRate, DnaDrawing drawing, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {
            
            DnaPoint point = new DnaPoint();
           
            int newValue = Tools.GetRandomChangeValue(this.EndPoint.X, this.StartPoint.X, Tools.MaxWidth - 1,MutationRate);

            point.X = (short)Math.Max(this.StartPoint.X, Math.Min(newValue, Tools.MaxWidth - 1));


            if (point.X == this.EndPoint.X)
                newValue = Tools.GetRandomChangeValueGuaranted(this.EndPoint.Y, this.StartPoint.Y, Tools.MaxHeight - 1,MutationRate);
            else
                newValue = Tools.GetRandomChangeValue(this.EndPoint.Y, this.StartPoint.Y, Tools.MaxHeight - 1, MutationRate);
            
            point.Y = (short)Math.Max(this.StartPoint.Y, Math.Min(newValue, Tools.MaxHeight - 1));

            this.EndPoint = point;

            point = new DnaPoint();
          

            newValue = Tools.GetRandomChangeValue(this.StartPoint.X, 0,this.EndPoint.X, MutationRate);


            point.X = (short)Math.Max(0, Math.Min(newValue, this.EndPoint.X));
            if (point.X == this.StartPoint.X)
                newValue = Tools.GetRandomChangeValueGuaranted(this.StartPoint.Y, 0, this.EndPoint.Y, MutationRate);
            else
                newValue = Tools.GetRandomChangeValue(this.StartPoint.Y,0, this.EndPoint.Y,MutationRate);
          

            point.Y = (short)Math.Max(0, Math.Min(newValue, this.EndPoint.Y));

            this.StartPoint = point;

            drawing.SetDirty();
            CreateNewUniqueId();
        }

        //public override void Mutate(DnaDrawing drawing, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        //{
        //    var point = new DnaPoint();
        //    int tmp = Tools.GetRandomNumber(0, 41);

        //    point.X = (short)Math.Min(this.StartPoint.X + tmp, Tools.MaxWidth - 1);
        //    if (tmp == 0)
        //        tmp = Tools.GetRandomNumber(1, 41);
        //    else
        //        tmp = Tools.GetRandomNumber(0, 41);

        //    point.Y = (short)Math.Min(this.StartPoint.Y + tmp, Tools.MaxHeight - 1);

        //    this.EndPoint = point;

        //    drawing.SetDirty();
        //    CreateNewUniqueId();
        //}

        public override void MutateTranspozite(DnaDrawing drawing, CanvasBGRA destImage = null)
        {

            Rectangle polygonArea = new Rectangle(this.StartPoint.X, this.StartPoint.Y, 
                this.Width, this.Height);

            const int defaultStepSize = 40;
            int leftMaxStep =  (polygonArea.X > defaultStepSize) ? defaultStepSize : polygonArea.X;
            int topMaxStep =  (polygonArea.Y > defaultStepSize) ? defaultStepSize : polygonArea.Y;
            int tmp = destImage.WidthPixel - polygonArea.X - polygonArea.Width;
            int rightMaxStep =  (tmp > defaultStepSize) ? defaultStepSize : tmp;
            tmp = destImage.HeightPixel - polygonArea.Y - polygonArea.Height;
            int downMaxStep =  (tmp > defaultStepSize) ? defaultStepSize : tmp;


            int maxWidhtForRND = leftMaxStep + rightMaxStep;
            int maxHeightForRND = topMaxStep + downMaxStep;

            int widthDelta = 0;
            int heightDelta = 0;

            if (maxWidhtForRND > 0)
                widthDelta = Tools.GetRandomNumber(1, maxWidhtForRND + 1) - leftMaxStep;

            if (maxHeightForRND > 0)
                heightDelta = Tools.GetRandomNumber(1, maxHeightForRND + 1) - topMaxStep;


            // apply move on all points

            this.StartPoint.X += (short)widthDelta;
            this.StartPoint.Y += (short)heightDelta;
            this.EndPoint.X += (short)widthDelta;
            this.EndPoint.Y += (short)heightDelta;

            drawing.SetDirty();
        }
    }
}
