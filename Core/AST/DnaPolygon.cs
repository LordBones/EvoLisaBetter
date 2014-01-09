﻿using System;
using System.Collections.Generic;
using System.Drawing;
using GenArt.Classes;
using GenArt.Core.AST;
using GenArt.Core.Classes;

namespace GenArt.AST
{
    public class DnaPolygon : DnaPrimitive
    {
        public DnaPoint [] _Points; // { get; set; }
        
        public DnaPolygon()
        {
            
        }

        public override DnaPoint[] Points
        {
            get { return _Points; }
        }

        public override void Init(byte mutationRate, ErrorMatrix errorMatrix, ImageEdges edgePoints = null)
        {
            

            int countPoints = Math.Min(Settings.ActivePointsPerPolygonMax,3);
            DnaPoint [] points = new DnaPoint [countPoints];

            if (edgePoints == null)
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

                    for (int i = 0; i < countPoints; i++)
                    {
                        int mutationMaxy = Math.Max(2, ((mutationRate + 1) * Tools.MaxHeight) / (256));
                        int mutationMiddley = mutationMaxy / 2;

                        int mutationMaxx = Math.Max(2, ((mutationRate + 1) * Tools.MaxWidth) / (256));
                        int mutationMiddlex = mutationMaxx / 2;

                        var point = new DnaPoint();
                        int tmp = Tools.GetRandomNumber(0, mutationMaxx);

                        point.X = (short)Math.Min(Math.Max(0, lastPoint.X + tmp - mutationMiddlex), Tools.MaxWidth - 1);
                        if(point.X == lastPoint.X)
                            tmp = Tools.GetRandomNumber(0, mutationMaxy, mutationMiddley);
                        else
                            tmp = Tools.GetRandomNumber(0, mutationMaxy);

                        point.Y = (short)Math.Min(Math.Max(0, lastPoint.Y + tmp - mutationMiddley), Tools.MaxHeight - 1);

                        


                        points[i] = point;
                        lastPoint = point;
                    }

                    //
                    if (!IsIntersect(points) && IsNotSmallAngles(points))
                    {
                        break;
                    }
                }
            }
            else
            {
                //while (true)
                //{
                //    int lastIndex = int.MaxValue;

                //    for (int i = 0; i < countPoints; i++)
                //    {
                        
                //        int index = Tools.GetRandomNumber(0, edgePoints.EdgePoints.Length-1, lastIndex);

                //        points[i] = edgePoints.EdgePoints[index];
                //        lastIndex = index;
                //    }

                //    //
                //    if (!IsIntersect(points) && IsNotSmallAngles(points))
                //    {
                //        break;
                //    }
                //}

                /*while (true)
                {
                    int lastIndex = int.MaxValue;

                    int index = Tools.GetRandomNumber(0, edgePoints.EdgePoints.Length - 1, lastIndex);

                    DnaPoint ? result = null;
                    
                    DnaPoint startPoint = edgePoints.EdgePoints[index];
                    points[0] = startPoint;

                    while (!result.HasValue)
                    {
                        DnaPoint endPoint = edgePoints.GetRandomBorderPoint();
                        result = edgePoints.GetFirstEdgeOnLineDirection(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                        if (result.HasValue && DnaPoint.Compare(result.Value, points[0]))
                            result = null;
                    }

                    points[1] = result.Value;
                    result = null;

                    while (!result.HasValue)
                    {
                        DnaPoint endPoint = edgePoints.GetRandomBorderPoint();
                        result = edgePoints.GetFirstEdgeOnLineDirection(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

                        if (result.HasValue && (DnaPoint.Compare(result.Value, points[0]) || DnaPoint.Compare(result.Value, points[1])))
                            result = null;
                    }

                    points[2] = result.Value;

                    //
                    if (!IsIntersect(points) && IsNotSmallAngles(points))
                    {
                        break;
                    }
                }*/

                Rectangle tile = new Rectangle(0,0,1,1);
                if (errorMatrix != null)
                {
                    int matrixIndex = errorMatrix.GetRNDMatrixRouleteIndex();
                    tile = errorMatrix.GetTileByErrorMatrixIndex(matrixIndex);
                }

                while (true)
                {
                    int startX = 0;
                    int startY = 0;
                    

                    if (errorMatrix != null)
                    {

                        startX = Tools.GetRandomNumber(0, tile.Width) + tile.X;
                        startY = Tools.GetRandomNumber(0, tile.Height) + tile.Y;
                    }
                    else
                    {
                        startX = Tools.GetRandomNumber(1, edgePoints.Width);
                        startY = Tools.GetRandomNumber(1, edgePoints.Height); 
                    }

                    DnaPoint ? result = null;

                    while (!result.HasValue)
                    {
                        DnaPoint endPoint = edgePoints.GetRandomBorderPoint(startX, startY);
                        result = edgePoints.GetFirstEdgeOnLineDirection(startX, startY, endPoint.X, endPoint.Y);
                    }

                    points[0] = result.Value;
                    result = null;


                    while (!result.HasValue  )
                    {
                        DnaPoint endPoint = edgePoints.GetRandomBorderPoint(startX, startY);
                        result = edgePoints.GetFirstEdgeOnLineDirection(startX, startY, endPoint.X, endPoint.Y);
                        if (result.HasValue && DnaPoint.Compare(result.Value, points[0]))
                            result = null;
                    }

                    points[1] = result.Value;
                    result = null;

                    while (!result.HasValue)
                    {
                        DnaPoint endPoint = edgePoints.GetRandomBorderPoint(startX, startY);
                        result = edgePoints.GetFirstEdgeOnLineDirection(startX, startY, endPoint.X, endPoint.Y);

                        if (result.HasValue && (DnaPoint.Compare(result.Value, points[0]) || DnaPoint.Compare(result.Value, points[1])) )
                            result = null;
                    }

                    points[2] = result.Value;

                   

                    //
                    if (//!IsIntersect(points) &&
                        !IsTriangleEdgesCrossedSomeEdge(points[0], points[1], points[2],edgePoints) &&
                        IsNotSmallAngles(points))
                    {
                        break;
                    }
                }
            }

            this._Points = points;

            Brush = new DnaBrush(255,255,0,0);
            CreateNewUniqueId();
        }

        public void InitTestPolygon()
        {
            //this.Points.Clear();
            //this.Points.Add(new DnaPoint(){ X=0,Y=0});
            //this.Points.Add(new DnaPoint() { X = Tools.MaxWidth-1, Y = 0 });
            //this.Points.Add(new DnaPoint() { X = Tools.MaxWidth - 1, Y = Tools.MaxHeight - 1 });
            //this.Points.Add(new DnaPoint() { X = 0, Y = Tools.MaxHeight - 1 });
            //this.Brush = new DnaBrush() { Alpha = 255, Blue = 255, Green = 255, Red = 0 };
     
        }

        public override object Clone()
        {
            var newPolygon = new DnaPolygon();
            newPolygon._Points = new DnaPoint[_Points.Length];
            newPolygon.Brush = Brush;
            newPolygon.UniqueId = UniqueId;

            Array.Copy(this._Points, newPolygon._Points, _Points.Length);
            //for (int index = 0; index < Points.Length; index++)
            //    newPolygon.Points[index] = Points[index];

            return newPolygon;
        }

        public override int GetCountPoints()
        {
            return this._Points.Length;
        }

        public override void Mutate(byte MutationRate, DnaDrawing drawing, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {

                DnaPoint [] points = this._Points;

                if (edgePoints != null && false)
                {
                    while (true)
                    {
                        int pointIndex = Tools.GetRandomNumber(0, points.Length);
                        DnaPoint oldPoint = points[pointIndex];

                        //get random end line on border canvas
                        //DnaPoint ? resultPoint = edgePoints.GetRandomCloserEdgePoint(oldPoint, 10);

                        DnaPoint endPoint = edgePoints.GetRandomBorderPoint(oldPoint.X, oldPoint.Y);

                        DnaPoint ? resultPoint = edgePoints.GetFirstEdgeOnLineDirection(oldPoint.X, oldPoint.Y, endPoint.X, endPoint.Y);

                        // zamezeni generovani bodu ktery uz je vrcholem trojuhelniku
                        if (resultPoint.HasValue)
                        {
                            bool p1 = DnaPoint.Compare(resultPoint.Value, points[0]);
                            bool p2 = DnaPoint.Compare(resultPoint.Value, points[1]);
                            bool p3 = DnaPoint.Compare(resultPoint.Value, points[2]);

                            if ((p1 && pointIndex != 0) || (p2 && pointIndex != 1) || (p2 && pointIndex != 2))
                                resultPoint = null;
                        }

                        if (resultPoint.HasValue)
                        {


                            points[pointIndex] = resultPoint.Value;

                            if (//IsNotSmallAngles(points) && 
                                //!IsTriangleEdgesCrossedSomeEdge(points[0], points[1], points[2],edgePoints)&&
                                !IsIntersect(points))
                            {
                                // dojde-li k posunu trojuhelniku, snizi se jeho pruhlednost o 10procent
                                //this.Brush.Alpha = (byte)Math.Max(this.Brush.Alpha - 10, 5);
                                //DnaBrush brush = this.Brush;
                                //brush.Alpha = (byte)Math.Max(this.Brush.Alpha - 10, 5);
                                //this.Brush = brush;

                                drawing.SetDirty();
                                CreateNewUniqueId();
                                break;
                            }
                        }

                        points[pointIndex] = oldPoint;
                    }
                }
                else
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

                            points[pointIndex] = newPoint;

                            if (IsNotSmallAngles(points) &&
                                //!IsTriangleEdgesCrossedSomeEdge(points[0], points[1], points[2],edgePoints)&&
                                !IsIntersect(points))
                            {
                                // dojde-li k posunu trojuhelniku, snizi se jeho pruhlednost o 10procent
                                //this.Brush.Alpha = (byte)Math.Max(this.Brush.Alpha - 10, 5);
                                //DnaBrush brush = this.Brush;
                                //brush.Alpha = (byte)Math.Max(this.Brush.Alpha - 10, 5);
                                //this.Brush = brush;



                                drawing.SetDirty();
                                CreateNewUniqueId();
                                break;
                            }


                            //points[pointIndex] = oldPoint;
                        }
                    }
                }


                
           
        }

        public override void MutateTranspozite(DnaDrawing drawing, CanvasBGRA destImage = null)
        {

            Rectangle polygonArea = DnaPolygon.GetPolygonArea(this._Points);

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

            for (int i = 0; i < this._Points.Length; i++)
            {
                this._Points[i].X += (short)widthDelta;
                this._Points[i].Y += (short)heightDelta;
                if (this._Points[i].X >= destImage.WidthPixel || this._Points[i].Y >= destImage.HeightPixel)
                    throw new Exception("Toto nesmi nastat");
            }

            drawing.SetDirty();
            
        }

        private bool IsTriangleEdgesCrossedSomeEdge(DnaPoint p1, DnaPoint p2, DnaPoint p3, ImageEdges edgePoints )
        {
            if (edgePoints.IsSomeEdgeOnLineNoStartEndPoint(p1.X, p1.Y, p2.X, p2.Y)) return true;
            if (edgePoints.IsSomeEdgeOnLineNoStartEndPoint(p2.X, p2.Y, p3.X, p3.Y)) return true;
            if (edgePoints.IsSomeEdgeOnLineNoStartEndPoint(p3.X, p3.Y, p1.X, p1.Y)) return true;

            return false;
        }

        public void Mutateold(DnaDrawing drawing, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {

            DnaPoint [] points = this._Points;

            if (Tools.GetRandomNumber(0, 1000000) < 750000)
            {
                int pointIndex = Tools.GetRandomNumber(0, points.Length);
                DnaPoint oldPoint = points[pointIndex];


                if (Tools.GetRandomNumber(0, 1000000) < 500000 && edgePoints.EdgePointsByY.Length > 0)
                {
                    int pointYIndex = Math.Max(edgePoints.Height - 1,
                    Math.Min(0, _Points[pointIndex].Y + Tools.GetRandomNumber(0, 20, 10) - 5));

                    DnaPoint [] rowEdges = edgePoints.EdgePointsByY[pointYIndex];

                    if (rowEdges.Length == 1)
                        points[pointIndex] = rowEdges[0];
                    else
                    {
                        points[pointIndex] = rowEdges[Tools.GetRandomNumber(0, rowEdges.Length)];
                    }

                    if (IsNotSmallAngles(points) && !IsIntersect(points))
                    {
                        drawing.SetDirty();
                        //break;
                    }

                    points[pointIndex] = oldPoint;
                    //Array.Copy(this.Points, points, this.Points.Length);

                }
                else
                {
                    int pointXIndex = Math.Max(edgePoints.Width - 1,
                    Math.Min(0, _Points[pointIndex].X + Tools.GetRandomNumber(0, 20, 10) - 5));

                    DnaPoint [] rowEdges = edgePoints.EdgePointsByX[pointXIndex];

                    if (rowEdges.Length == 1)
                        points[pointIndex] = rowEdges[0];
                    else
                    {
                        points[pointIndex] = rowEdges[Tools.GetRandomNumber(0, rowEdges.Length)];
                    }

                    if (IsNotSmallAngles(points) && !IsIntersect(points))
                    {
                        drawing.SetDirty();
                        //break;
                    }

                    points[pointIndex] = oldPoint;

                }

            }
            else
            {
                while (true)
                {
                    int pointIndex = Tools.GetRandomNumber(0, points.Length);

                    DnaPoint oldPoint = points[pointIndex];

                    if (edgePoints == null)
                        points[pointIndex].MutateMiddle();
                    else
                    {
                        int edgeIndex = Tools.GetRandomNumber(0, edgePoints.EdgePoints.Length);
                        points[pointIndex] = edgePoints.EdgePoints[edgeIndex];
                    }

                    if (IsNotSmallAngles(points) && !IsIntersect(points))
                    {

                        drawing.SetDirty();
                        break;
                    }

                    points[pointIndex] = oldPoint;
                    //Array.Copy(this.Points, points, this.Points.Length);
                }
            }

        }

        public override void GetRangeHighSize(ref int startY, ref int endY)
        {
            startY = this._Points[0].Y;
            endY = startY;
            int tmp = this._Points[1].Y;
            if (startY > tmp) startY = tmp;
            if (endY < tmp) endY = tmp;

            tmp = this._Points[2].Y;
            if (startY > tmp) startY = tmp;
            if (endY < tmp) endY = tmp;
        }

        public override void GetRangeWidthByRow(int y, ref int startX, ref int endX)
        {
            startX = 0;
            endX = -1;
        }


        public override bool IsPointInside(DnaPoint point)
        {
            return GraphicFunctions.IsPointInTriangle(this._Points[0], this._Points[1], this._Points[2], point);
        }

        public override bool IsLineCrossed(DnaPoint startLine, DnaPoint endLine)
        {
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                this._Points[0], this._Points[1])) return true;
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                this._Points[1], this._Points[2])) return true;
            if (GraphicFunctions.LineIntersect(startLine, endLine,
                this._Points[2], this._Points[0])) return true;
            
            return false;
        }
        
      

        public bool IsIntersect(List<DnaPoint> points)
        {
            return IsIntersect(points.ToArray());
        }

        public bool IsIntersect(DnaPoint [] points)
        {
            //return false;
            //Tuple<DnaPoint,DnaPoint> [] lines = new Tuple<DnaPoint, DnaPoint>[points.Length];


            //lines[0] = new Tuple<DnaPoint, DnaPoint>(points[points.Length - 1], points[0]);

            //for (int i = 1; i < points.Length; i++)
            //{
            //    lines[i] = new Tuple<DnaPoint, DnaPoint>(points[i - 1], points[i]);
            //}

            for(int index = 0; index < points.Length; index++)
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
                    i1item1 = points[index-1];
                    i1item2 = points[index];
               
                }
             
                for (int index2 = 0; index2 < points.Length; index2++ )
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

        public bool IsIntersect2(DnaPoint[] points)
        {
            //return false;
            Tuple<DnaPoint,DnaPoint> [] lines = new Tuple<DnaPoint, DnaPoint>[points.Length];


            lines[0] = new Tuple<DnaPoint, DnaPoint>(points[points.Length - 1], points[0]);

            for (int i = 1; i < points.Length; i++)
            {
                lines[i] = new Tuple<DnaPoint, DnaPoint>(points[i - 1], points[i]);
            }

            for (int index = 0; index < points.Length; index++)
            {
                DnaPoint i1item1 = lines[index].Item1;
                DnaPoint i1item2 = lines[index].Item2;


                for (int index2 = 0; index2 < points.Length; index2++)
                {
                    DnaPoint i2item1 = lines[index2].Item1;
                    DnaPoint i2item2 = lines[index2].Item2;

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

        public bool IsNotSmallAngles(List<DnaPoint> points)
        {
            return IsNotSmallAngles(points.ToArray());
        }
        public bool IsNotSmallAngles(DnaPoint [] points)
        {
            //return true;
            if (points.Length < 3)
                return true;

            for (int index = 1; index < points.Length-1; index++)
            {
                DnaPoint point = points[index];
                DnaPoint point1 = points[index+1];
                DnaPoint pointm1 = points[index-1];

 
                if(!LinesHasMinimalAngle(
                    pointm1.X, pointm1.Y,
                    point1.X, point1.Y,
                    point.X,point.Y))
                {
                    return false;
                }
            }

            DnaPoint pointZero = points[0];
            DnaPoint pointEnd = points[points.Length - 1];

            if (!LinesHasMinimalAngle(
           pointEnd.X, pointEnd.Y,
           points[1].X, points[1].Y,
           pointZero.X, pointZero.Y))
            {
                return false;
            }

            if (!LinesHasMinimalAngle(
           points[points.Length - 2].X, points[points.Length - 2].Y,
           pointZero.X, pointZero.Y,
           pointEnd.X, pointEnd.Y))
            {
                return false;
            }

            
            return true;
        }

       


        


        private const double CONST_ANGLE = System.Math.PI * (10 / 180.0);
        private const double CONST_ANGLE_Inverse = System.Math.PI * 2 - CONST_ANGLE;

        private static bool LinesHasMinimalAngle(int x1, int y1, int x2, int y2, int middleX, int middleY)
        {
            //return true;
            double px1 = x1 - middleX;
            double py1 = y1 - middleY;
            double px2 = x2 - middleX;
            double py2 = y2 - middleY;

            double angle = Math.Abs(PointsAngle2(px1, py1, px2, py2));

            if (angle < 0.0d)
            {
                throw new Exception("Error");
            }

            return angle >= CONST_ANGLE && (angle <= CONST_ANGLE_Inverse);
            //return true;

        }



        public static double PointsAngle2(double px1, double py1, double px2, double py2)
        {
            Double Angle = Math.Atan2(py1 - 0, px1 - 0) - Math.Atan2(py2 - 0, px2 - 0);

            return Angle;
        }


        private static bool LinesHasMinimalAngleDegree(int x1, int y1, int x2, int y2, int middleX, int middleY)
        {
            //return true;
            double px1 = x1 - middleX;
            double py1 = y1 - middleY;
            double px2 = x2 - middleX;
            double py2 = y2 - middleY;

            double angle = Math.Abs(PointsAngle2Degree(px1, py1, px2, py2));

            if (angle < 0.0d)
            {
                throw new Exception("Error");
            }

            return angle >= 10.0d && (angle <= (360.0d - 10.0d));
            //return true;

        }

        

        public static double PointsAngle2Degree(double px1, double py1, double px2, double py2)
        {
            Double Angle = Math.Atan2(py1 - 0, px1 - 0) - Math.Atan2(py2 - 0, px2 - 0);

            return Angle * 180 / System.Math.PI ;
        }

        public long GetPixelSizePolygon()
        {
            if(this._Points.Length < 3)
                return 0;

            long minX = long.MaxValue,minY = long.MaxValue;
            long maxX = 0,maxY = 0;

            for (int i = 0; i < 3; i++)
            {
                int x = this._Points[i].X;
                int y = this._Points[i].Y;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            return ((maxX - minX + 1) * (maxY - minY + 1)) / 2;
        }

        public static Rectangle GetPolygonArea(DnaPoint [] points)
        {   
            int minX = int.MaxValue,minY = int.MaxValue;
            int maxX = 0,maxY = 0;

            for (int i = 0; i < points.Length; i++)
            {
                int x = points[i].X;
                int y = points[i].Y;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            return new Rectangle(minX, minY, maxX - minX+1, maxY - minY+1);
        }

      
    }
}