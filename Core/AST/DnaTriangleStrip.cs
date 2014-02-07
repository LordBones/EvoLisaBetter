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
    public class DnaTriangleStrip : DnaPrimitive
    {
        public const int CONST_MinPoints = 3;
        public const int CONST_MaxPoints = 10;

        public DnaPoint [] _Points;

        public override GenArt.AST.DnaPoint[] Points
        {
            get { return _Points; }
        }

        public override object Clone()
        {
            var newTrinagleStrip = new DnaTriangleStrip();
            newTrinagleStrip._Points = new DnaPoint[_Points.Length];
            newTrinagleStrip.Brush = Brush;
            newTrinagleStrip.UniqueId = UniqueId;

            Array.Copy(this._Points, newTrinagleStrip._Points, _Points.Length);
            //for (int index = 0; index < Points.Length; index++)
            //    newPolygon.Points[index] = Points[index];

            return newTrinagleStrip;
        }

        public override int GetCountPoints()
        {
            return this._Points.Length;
        }

        public override void Init(byte mutationRate, Classes.ErrorMatrix errorMatrix, Classes.ImageEdges edgePoints = null)
        {
            DnaPoint [] points = new DnaPoint[CONST_MinPoints];

            //if (edgePoints == null)
            {

                var origin = new DnaPoint();


                while (true)
                {
                    origin.Init();

                    Rectangle tile = new Rectangle(0, 0, 1, 1);
                    if (errorMatrix != null)
                    {
                        int matrixIndex = errorMatrix.GetRNDMatrixRouleteIndex();
                        tile = errorMatrix.GetTileByErrorMatrixIndex(matrixIndex);

                        origin.X = (short)(tile.X + Tools.GetRandomNumber(0, tile.Width));
                        origin.Y = (short)(tile.Y + Tools.GetRandomNumber(0, tile.Height));
                    }

                    DnaPoint lastPoint = origin;
                    //int addX = 0;
                    //int addY = 0;

                    for (int i = 0; i < CONST_MinPoints; i++)
                    {
                        int mutationMaxy = Math.Max(2, ((mutationRate + 1) * Tools.MaxHeight - 1) / (256));
                        int mutationMiddley = mutationMaxy / 2;

                        int mutationMaxx = Math.Max(2, ((mutationRate + 1) * Tools.MaxWidth - 1) / (256));
                        int mutationMiddlex = mutationMaxx / 2;

                        var point = new DnaPoint();
                        int tmp = Tools.GetRandomNumber(0, mutationMaxx);

                        point.X = (short)Math.Min(Math.Max(0, lastPoint.X + tmp - mutationMiddlex), Tools.MaxWidth - 1);
                        if (point.X == lastPoint.X)
                            tmp = Tools.GetRandomNumber(0, mutationMaxy, mutationMiddley);
                        else
                            tmp = Tools.GetRandomNumber(0, mutationMaxy);

                        point.Y = (short)Math.Min(Math.Max(0, lastPoint.Y + tmp - mutationMiddley), Tools.MaxHeight - 1);




                        points[i] = point;
                        lastPoint = point;
                    }

                    
                    if (!IsIntersect(points) 
                        //&& IsNotSmallAngles(points)
                        )
                    {
                        break;
                    }
                }
            
           
            }

            this._Points = points;

            Brush = new DnaBrush(255, 255, 0, 0);
            CreateNewUniqueId();
        }

        public bool IsIntersect(DnaPoint[] points)
        {
            //return false;
            //Tuple<DnaPoint,DnaPoint> [] lines = new Tuple<DnaPoint, DnaPoint>[points.Length];


            //lines[0] = new Tuple<DnaPoint, DnaPoint>(points[points.Length - 1], points[0]);

            //for (int i = 1; i < points.Length; i++)
            //{
            //    lines[i] = new Tuple<DnaPoint, DnaPoint>(points[i - 1], points[i]);
            //}

            for (int index = 0; index < points.Length; index++)
            {
                DnaPoint i1item1;
                DnaPoint i1item2;
                if (index == 0)
                {
                    i1item1 = points[points.Length - 1];
                    i1item2 = points[0];
                }
                else
                {
                    i1item1 = points[index - 1];
                    i1item2 = points[index];

                }

                for (int index2 = 0; index2 < points.Length; index2++)
                {
                    DnaPoint i2item1;
                    DnaPoint i2item2;

                    if (index2 == 0)
                    {
                        i2item1 = points[points.Length - 1];
                        i2item2 = points[0];
                    }
                    else
                    {
                        i2item1 = points[index2 - 1];
                        i2item2 = points[index2];
                    }

                    if (DnaPoint.Compare(i1item1, i2item1) ||
                        DnaPoint.Compare(i1item1, i2item2) ||
                        DnaPoint.Compare(i1item2, i2item1) ||
                        DnaPoint.Compare(i1item2, i2item2)
                        )
                    {
                        continue;
                    }

                    if (GraphicFunctions.LineIntersect(i2item1, i2item2,
                        i1item1, i1item2))
                    {
                        return true;
                    }

                    //if (DnaPoint.Compare(lines[index].Item1, lines[index2].Item1) ||
                    //  DnaPoint.Compare(lines[index].Item1, lines[index2].Item2) ||
                    //  DnaPoint.Compare(lines[index].Item2, lines[index2].Item1) ||
                    //  DnaPoint.Compare(lines[index].Item2, lines[index2].Item2)
                    //  )
                    //{
                    //    continue;
                    //}

                    //if (LineIntersect(lines[index2].Item1, lines[index2].Item2,
                    //    lines[index].Item1, lines[index].Item2))
                    //{
                    //    return true;
                    //}
                }
            }

            return false;
        }

        public override void GetRangeHighSize(ref int startY, ref int endY)
        {
            startY = this._Points[0].Y;
            endY = startY;
            for (int i = 1; i < this.Points.Length; i++)
            {
                int tmp = this._Points[i].Y;
                if (startY > tmp) startY = tmp;
                if (endY < tmp) endY = tmp;
            }
        }

        public override bool GetRangeWidthByRow(int y, ref int startx, ref int endx)
        {
            return false;
        }

        public override bool IsPointInside(GenArt.AST.DnaPoint point)
        {
            for(int i = 0;i<= this.Points.Length-3;i++)
            {
                if (GraphicFunctions.IsPointInTriangle(this._Points[i], this._Points[i + 1], this._Points[i + 2], point))
                    return true;
            }

            return false;
        }

        public override bool IsLineCrossed(GenArt.AST.DnaPoint startLine, GenArt.AST.DnaPoint endLine)
        {
#warning neoptimalni opravit

            for (int i = 0; i <= this.Points.Length - 3; i++)
            {

                if (GraphicFunctions.LineIntersect(startLine, endLine,
                    this._Points[i], this._Points[i+1])) return true;
                if (GraphicFunctions.LineIntersect(startLine, endLine,
                    this._Points[i+1], this._Points[i+2])) return true;
                if (GraphicFunctions.LineIntersect(startLine, endLine,
                    this._Points[i+2], this._Points[i])) return true;
            }

            return false;
        }

        public override void Mutate(byte MutationRate, GenArt.AST.DnaDrawing drawing, Classes.CanvasBGRA destImage = null, Classes.ImageEdges edgePoints = null)
        {
            DnaPoint [] points = this._Points;

            int action = -1;
            while(action < 0)
            {
                int tmp = Tools.GetRandomNumber(0, 4);
                if(points.Length < CONST_MaxPoints && tmp == 0) action = 0;
                else if(points.Length > CONST_MinPoints && tmp == 1) action = 1;
                // dvakrat casteji se hybe s bodem nez se vklada a maze
                else if(tmp >= 2) action = 2;
            }

            if (action == 0)
            {
                int count = 10;
                while (true && count > 0)
                {
                    DnaPoint newPoint = new DnaPoint();
                    newPoint.Init();

                    List<DnaPoint> newPoints = new List<DnaPoint>(points);
                    //int index = Tools.GetRandomNumber(0, points.Length);
                    int index = Tools.GetRandomNumber(0, 2);
                    if (index == 1) index = points.Length - 1; 

                    int mutationMaxy = Math.Max(2, ((MutationRate + 1) * Tools.MaxHeight - 1) / (256));
                    int mutationMiddley = mutationMaxy / 2;

                    int mutationMaxx = Math.Max(2, ((MutationRate + 1) * Tools.MaxWidth - 1) / (256));
                    int mutationMiddlex = mutationMaxx / 2;

                    
                    int tmp = Tools.GetRandomNumber(0, mutationMaxx);

                    newPoint.X = (short)Math.Min(Math.Max(0, points[index].X + tmp - mutationMiddlex), Tools.MaxWidth - 1);
                    if (newPoint.X == points[index].X)
                        tmp = Tools.GetRandomNumber(0, mutationMaxy, mutationMiddley);
                    else
                        tmp = Tools.GetRandomNumber(0, mutationMaxy);

                    newPoint.Y = (short)Math.Min(Math.Max(0, points[index].Y + tmp - mutationMiddley), Tools.MaxHeight - 1);

                    newPoints.Insert(index, newPoint);
                    DnaPoint [] tmpp = newPoints.ToArray();

                    if (!IsIntersect(points)
                        //&& IsNotSmallAngles(points) 
                        )
                    {
                        this._Points = tmpp;
                        drawing.SetDirty();
                        break;
                    }

                    count--;
                }
            }
            else if (action == 1)
            {
                //int index = Tools.GetRandomNumber(0, points.Length);
                int index = Tools.GetRandomNumber(0, 2);
                if (index == 1) index = points.Length - 1;

                DnaPoint [] newPoints = new DnaPoint[points.Length - 1];

                //if (index > 0)
                //    Array.Copy(Polygons, polygons, index);
                for (int i = 0; i < index; i++)
                    newPoints[i] = points[i];

                for (int i = index; i < newPoints.Length; i++)
                    newPoints[i] = points[i + 1];

                this._Points = newPoints;
                drawing.SetDirty();
            }
            else if (action == 2)
            {
                //if (Tools.GetRandomNumber(0, 10) < 5 && drawing.Polygons.Length > 1)
                //{
                //    int polyIndex = Tools.GetRandomNumber(0, drawing.Polygons.Length);
                //    int pointIndex = Tools.GetRandomNumber(0, drawing.Polygons[polyIndex].Points.Length);

                //    int pointNewIndex = Tools.GetRandomNumber(0, points.Length);
                //    points[pointIndex] = drawing.Polygons[polyIndex].Points[pointIndex];
                //}
                //else
                {
                    while (true)
                    {
                        int pointIndex = Tools.GetRandomNumber(0, points.Length);
                        DnaPoint oldPoint = points[pointIndex];

                        //get random end line on border canvas
                        //DnaPoint ? resultPoint = edgePoints.GetRandomCloserEdgePoint(oldPoint, 10);


                        DnaPoint newPoint = new DnaPoint();

                        int newValue = Tools.GetRandomNumberNoLinear_MinMoreOften(oldPoint.X,
                            0, Tools.MaxWidth - 1, MutationRate);

                        newPoint.X = (short)Math.Min(Math.Max(0, newValue), Tools.MaxWidth - 1);

                        newValue = Tools.GetRandomNumberNoLinear_MinMoreOften(oldPoint.Y,
                            0, Tools.MaxHeight - 1, MutationRate);

                        newPoint.Y = (short)Math.Min(Math.Max(0, newValue), Tools.MaxHeight - 1);

                        if (newPoint.X == oldPoint.X && newPoint.Y == oldPoint.Y)
                            break;



                        bool p1 = DnaPoint.Compare(newPoint, points[0]);
                        bool p2 = DnaPoint.Compare(newPoint, points[1]);
                        bool p3 = DnaPoint.Compare(newPoint, points[2]);

                        if ((p1 && pointIndex != 0) || (p2 && pointIndex != 1) || (p2 && pointIndex != 2))
                        {
                            points[pointIndex] = oldPoint;
                            continue;
                        }

                        bool has = false;

                        if (pointIndex == points.Length - 1)
                            has = GraphicFunctions.TriangleHasNotSmallAngle(
                                points[pointIndex].X, points[pointIndex].Y, points[pointIndex - 1].X, points[pointIndex - 1].Y, points[pointIndex - 2].X, points[pointIndex - 2].Y);
                        else if(pointIndex == 0)
                            has = GraphicFunctions.TriangleHasNotSmallAngle(
                        points[0].X, points[0].Y, points[1].X, points[1].Y, points[2].X, points[2].Y);
                        else
                            has = GraphicFunctions.TriangleHasNotSmallAngle(
                           points[pointIndex].X, points[pointIndex].Y, points[pointIndex - 1].X, points[pointIndex - 1].Y, points[pointIndex +1].X, points[pointIndex +1].Y);
                       
                     
                        if (!has)
                        {
                            points[pointIndex] = oldPoint;
                            continue;
                        }

                        if (!IsIntersect(points)
                            //&& IsNotSmallAngles(points)
                        )
                        {
                            points[pointIndex] = newPoint;

                            drawing.SetDirty();
                            CreateNewUniqueId();
                            break;
                        }
                    }
                }
            }
        }

        public override void MutateTranspozite(GenArt.AST.DnaDrawing drawing, Classes.CanvasBGRA destImage = null)
        {
            
        }
    }
}
