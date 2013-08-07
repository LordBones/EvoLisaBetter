using System;
using System.Collections.Generic;
using System.Drawing;
using GenArt.Classes;
using GenArt.Core.Classes;

namespace GenArt.AST
{
    [Serializable]
    public class DnaPolygon
    {
        public DnaPoint [] Points; // { get; set; }
        public DnaBrush Brush;// { get; set; }
        
        public DnaPolygon()
        {

        }



        public void Init(ImageEdges edgePoints = null)
        {
            //87,81 195,0 199,79

            //int p1 = 0;
            //int p2 = 11;
            //int p3 = 10;
            //int p4 = 0;
            //double f = PointsAngle2(p1, p2, p3, p4);
            //double f1 = PointsAngle2(-p1, p2, -p3, p4);
            //double f2 = PointsAngle2(p1, -p2, p3, -p4);
            //double f3 = PointsAngle2(-p1, -p2, -p3, -p4);
            //double fff = f1 + f2 + f3 + f;
           

            //int count = Tools.GetRandomNumber(3, 3);

            int countPoints = Math.Min(Settings.ActivePointsPerPolygonMax,3);
            DnaPoint [] points = new DnaPoint [countPoints];

            if (edgePoints == null)
            {

                var origin = new DnaPoint();
                origin.Init();

                while (true)
                {
                    DnaPoint lastPoint = origin;
                    //int addX = 0;
                    //int addY = 0;

                    for (int i = 0; i < countPoints; i++)
                    {
                        var point = new DnaPoint();
                        int tmp = Tools.GetRandomNumber(0, 20,10);

                        point.X = (short)Math.Min(Math.Max(0, lastPoint.X + tmp -10), Tools.MaxWidth - 1);
                        tmp = Tools.GetRandomNumber(0, 20,10);
                        point.Y = (short)Math.Min(Math.Max(0, lastPoint.Y + tmp - 10), Tools.MaxHeight - 1);

                        //if ((Tools.GetRandomNumber(0, 1000) > 500))
                        //{
                        //    point.X = (short)Math.Min(Math.Max(0, lastPoint.X + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxWidth - 1);
                        //    point.Y = lastPoint.Y;
                        //}

                        //else
                        //{
                        //    point.Y = (short)Math.Min(Math.Max(0, lastPoint.Y + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxHeight - 1);
                        //    point.X = lastPoint.X;
                        //}

                        //point.X = Math.Min(Math.Max(0, origin.X + Tools.GetRandomNumber(-10, 10)), Tools.MaxWidth - 1);
                        //point.Y = Math.Min(Math.Max(0, origin.Y + Tools.GetRandomNumber(-10, 10)), Tools.MaxHeight - 1);


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
                while (true)
                {
                    int lastIndex = int.MaxValue;

                    for (int i = 0; i < countPoints; i++)
                    {
                        
                        int index = Tools.GetRandomNumber(0, edgePoints.EdgePoints.Length-1, lastIndex);

                        points[i] = edgePoints.EdgePoints[index];
                        lastIndex = index;
                    }

                    //
                    if (!IsIntersect(points) && IsNotSmallAngles(points))
                    {
                        break;
                    }
                }
            }

            this.Points = points;

            Brush = new DnaBrush(0,255,0,0);
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

        public DnaPolygon Clone()
        {
            var newPolygon = new DnaPolygon();
            newPolygon.Points = new DnaPoint[Points.Length];
            newPolygon.Brush = Brush;
           
            Array.Copy(this.Points, newPolygon.Points, Points.Length);
            //for (int index = 0; index < Points.Length; index++)
            //    newPolygon.Points[index] = Points[index];

            return newPolygon;
        }

        public DnaPoint [] ClonePoints()
        {
            DnaPoint [] result = new DnaPoint[this.Points.Length];
            
            Array.Copy(this.Points, result, Points.Length);

            return result;
        }

        public void Mutate(DnaDrawing drawing, CanvasBGRA destImage = null, ImageEdges edgePoints = null)
        {

                DnaPoint [] points = this.Points;

                if ( Tools.GetRandomNumber(0, 1000000) < 750000)
                {
                    int pointIndex = Tools.GetRandomNumber(0, points.Length - 1);
                    DnaPoint oldPoint = points[pointIndex];


                    if (Tools.GetRandomNumber(0, 1000000) < 500000 && edgePoints.EdgePointsByY.Length > 0)
                    {
                        int pointYIndex = Math.Max(edgePoints.Height - 1,
                        Math.Min(0, Points[pointIndex].Y + Tools.GetRandomNumber(0, 20, 10) - 5));

                        DnaPoint [] rowEdges = edgePoints.EdgePointsByY[pointYIndex];

                        if (rowEdges.Length == 1)
                            points[pointIndex] = rowEdges[0];
                        else
                        {
                            points[pointIndex] = rowEdges[Tools.GetRandomNumber(0, rowEdges.Length - 1)];
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
                        Math.Min(0, Points[pointIndex].X + Tools.GetRandomNumber(0, 20, 10) - 5));

                        DnaPoint [] rowEdges = edgePoints.EdgePointsByX[pointXIndex];

                        if (rowEdges.Length == 1)
                            points[pointIndex] = rowEdges[0];
                        else
                        {
                            points[pointIndex] = rowEdges[Tools.GetRandomNumber(0, rowEdges.Length - 1)];
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
                        int pointIndex = Tools.GetRandomNumber(0, points.Length - 1);
                        
                        DnaPoint oldPoint = points[pointIndex];

                        if (edgePoints == null)
                            points[pointIndex].MutateMiddle();
                        else
                        {
                            int edgeIndex = Tools.GetRandomNumber(0, edgePoints.EdgePoints.Length - 1);
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
            
           // else
            /*RemovePointByChance(drawing);



            if (Tools.WillMutate(Settings.ActiveAddPointMutationRate))
                AddPoint(drawing);

            {
                #region move point with test intersect


                DnaPoint [] points = this.ClonePoints();
                //DnaBrush brush = new DnaBrush();
                bool genNewUidID = false;
                bool pointsWasChange = false;
                bool changeColor = false;

                //if (Tools.WillMutate(Settings.ActiveMovePointMaxMutationRate))
                //{
                //    int pointIndex = Tools.GetRandomNumber(0, points.Length - 1);
                //    points[pointIndex].MutateWholeRange();
                //    pointsWasChange = true;
                //    changeColor = false;
                //}
                //else 
                if (Tools.WillMutate(Settings.ActiveMovePointMidMutationRate))
                {
                    while (true)
                    {
                        int pointIndex = Tools.GetRandomNumber(0, points.Length - 1);
                        points[pointIndex].MutateMiddle();

                        if (IsNotSmallAngles(points) && !IsIntersect(points))
                        {
                            genNewUidID = true;
                            pointsWasChange = true;
                            break;
                        }

                        Array.Copy(this.Points, points, this.Points.Length);
                    }
                }

                else if (Tools.WillMutate(Settings.ActiveMovePointMinMutationRate))
                {
                    while (true)
                    {
                        int pointIndex = Tools.GetRandomNumber(0, points.Length - 1);
                        points[pointIndex].MutateSmall();

                        if (IsNotSmallAngles(points) && !IsIntersect(points))
                        {
                            pointsWasChange = true;
                            break;
                        }

                        Array.Copy(this.Points, points, this.Points.Length);
                    }

                }



                if (pointsWasChange)
                {
                    this.Points = points;

                    if (changeColor)
                    {
                        if (_rawDestImage != null)
                        {
                            Color tmpColor = DnaDrawing.GetColorByPolygonPoints(this.Points, _rawDestImage, width);
                            this.Brush.SetByColor(tmpColor);
                        }
                    }

                    if (genNewUidID)
                        this.UniqueID = DnaPolygon.GetNewUiqueId();


                    drawing.SetDirty();

                }

                #endregion
            }
            */


        }

        public void Mutate2(DnaDrawing drawing, byte[] _rawDestImage = null, int width = 0)
        {
            RemovePointByChance(drawing);

            if (Tools.WillMutate(Settings.ActiveAddPointMutationRate))
                AddPoint(drawing);

            if (Tools.WillMutate(50000))
            {
                if (_rawDestImage != null)
                {
                    Color tmpColor = DnaDrawing.GetColorByPolygonPoints(this.Points, _rawDestImage, width);
                    this.Brush.SetByColor(tmpColor);
                }
            }
            else
            {
                Brush.MutateRGBOld(drawing);
            }

            #region move point with test intersect


            DnaPoint [] points = this.ClonePoints();
            bool pointsWasChange = false;

            for (int index = 0; index < points.Length; index++)
            {
                pointsWasChange |= points[index].Mutate();
                //if (pointsWasChange) break;
            }

            if (pointsWasChange)
            {
                if (IsNotSmallAngles(points) && !IsIntersect(points))
                {
                    this.Points = points;
                    drawing.SetDirty();
                }
            }

            #endregion


        }

        private void RemovePointByChance(DnaDrawing drawing)
        {

            if (Points.Length > Settings.ActivePointsPerPolygonMin && drawing.PointCount > Settings.ActivePointsMin)
            {
                if (Tools.WillMutate(Settings.ActiveRemovePointMutationRate))
                {
                    DnaPoint [] points = new DnaPoint[this.Points.Length - 1];

                    int newPointIndex = Tools.GetRandomNumber(0, this.Points.Length - 1);

                    for (int repeat = 0; repeat < this.Points.Length;repeat++ )
                    {
                    
                        int tmpIndex = 0;

                        for (int index = 0; index < this.Points.Length; index++)
                        {
                            if (index != newPointIndex)
                            {
                                points[tmpIndex] = this.Points[index];
                                tmpIndex++;
                            }
                        }
                        
                        if (newPointIndex > 0 && newPointIndex < (this.Points.Length - 1))
                        {
                            DnaPoint middlePoint = this.Points[newPointIndex];

                            DnaPoint newpoint = points[newPointIndex];
                            newpoint.X = (short)((newpoint.X + middlePoint.X) >> 1);
                            newpoint.Y = (short)((newpoint.Y + middlePoint.Y) >> 1);
                            points[newPointIndex] = newpoint;

                            newpoint = points[newPointIndex - 1];
                            newpoint.X = (short)((newpoint.X + middlePoint.X) >> 1);
                            newpoint.Y = (short)((newpoint.Y + middlePoint.Y) >> 1);
                            points[newPointIndex - 1] = newpoint;

                            //points[newPointIndex].X = (short)((points[newPointIndex].X + middlePoint.X) >> 1);
                            //points[newPointIndex].Y = (short)((points[newPointIndex].Y + middlePoint.Y) >> 1);

                            //points[newPointIndex-1].X = (short)((points[newPointIndex-1].X + middlePoint.X) >> 1);
                            //points[newPointIndex-1].Y = (short)((points[newPointIndex-1].Y + middlePoint.Y) >> 1);

                        }
                        else if (newPointIndex == 0)
                        {
                            DnaPoint middlePoint = this.Points[newPointIndex];

                            DnaPoint newpoint = points[newPointIndex];
                            newpoint.X = (short)((newpoint.X + middlePoint.X) >> 1);
                            newpoint.Y = (short)((newpoint.Y + middlePoint.Y) >> 1);
                            points[newPointIndex] = newpoint;

                            newpoint = points[points.Length - 1];
                            newpoint.X = (short)((newpoint.X + middlePoint.X) >> 1);
                            newpoint.Y = (short)((newpoint.Y + middlePoint.Y) >> 1);
                            points[points.Length - 1] = newpoint;

                            //points[newPointIndex].X = (short)((points[newPointIndex].X + middlePoint.X) >> 1);
                            //points[newPointIndex].Y = (short)((points[newPointIndex].Y + middlePoint.Y) >> 1);

                            //points[points.Length - 1].X = (short)((points[points.Length - 1].X + middlePoint.X) >> 1);
                            //points[points.Length - 1].Y = (short)((points[points.Length - 1].Y + middlePoint.Y) >> 1);

                        }

                        else if (newPointIndex == points.Length - 1)
                        {
                            DnaPoint middlePoint = this.Points[newPointIndex];

                            DnaPoint newpoint = points[newPointIndex];
                            newpoint.X = (short)((newpoint.X + middlePoint.X) >> 1);
                            newpoint.Y = (short)((newpoint.Y + middlePoint.Y) >> 1);
                            points[newPointIndex] = newpoint;

                            newpoint = points[0];
                            newpoint.X = (short)((newpoint.X + middlePoint.X) >> 1);
                            newpoint.Y = (short)((newpoint.Y + middlePoint.Y) >> 1);
                            points[0] = newpoint;


                            //points[newPointIndex].X = (short)((points[newPointIndex].X + middlePoint.X) >> 1);
                            //points[newPointIndex].Y = (short)((points[newPointIndex].Y + middlePoint.Y) >> 1);

                            //points[0].X = (short)((points[0].X + middlePoint.X) >> 1);
                            //points[0].Y = (short)((points[0].Y + middlePoint.Y) >> 1);

                        }
                        

                        if (IsNotSmallAngles(points) && !IsIntersect(points))
                        {
                            this.Points = points;
                            drawing.SetDirty();
                            break;
                        }
                        else
                        {
                            if (newPointIndex >= (this.Points.Length-1))
                                newPointIndex = 0;
                            else
                                newPointIndex++;
                        }
                    }
                }
            }
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

                    if (LineIntersect(i2item1, i2item2,
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

                    if (LineIntersect(i2item1, i2item2,
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

        private void AddPoint(DnaDrawing drawing)
        {
            if (Points.Length < Settings.ActivePointsPerPolygonMax)
            {
                if (drawing.PointCount < Settings.ActivePointsMax)
                {
                    for (int countTest = 0; countTest < 100; countTest++)
                    {
                        List<DnaPoint> points = new List<DnaPoint>(this.ClonePoints());

                        DnaPoint newPoint = new DnaPoint();

                        int index = Tools.GetRandomNumber(1, points.Count - 1);

                        DnaPoint prev = points[index - 1];
                        DnaPoint next = points[index];

                        newPoint.X = (short)((prev.X + next.X) >> 1);
                        newPoint.Y = (short)((prev.Y + next.Y) >> 1);

                        // some noise
                        int tmp = Tools.GetRandomNumber(1, 10);
                        newPoint.X = (short)Math.Min(Math.Max(0, newPoint.X + ((Tools.GetRandomNumber(0, 1000)>500)? -tmp : tmp) ), Tools.MaxWidth-1);
                        tmp = Tools.GetRandomNumber(1, 10);
                        newPoint.Y = (short)Math.Min(Math.Max(0, newPoint.Y + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxHeight - 1);


                        points.Insert(index, newPoint);

                        if (IsNotSmallAngles(points) && !IsIntersect(points) )
                        {
                            this.Points = points.ToArray();
                            drawing.SetDirty();
                            break;
                        }
                    }
                }
            }
        }


        private static bool LineIntersect(DnaPoint l1p1, DnaPoint l1p2, DnaPoint l2p1, DnaPoint l2p2 )
        {
            return LineIntersect(l1p1.X, l1p1.Y, l1p2.X, l1p2.Y, l2p1.X, l2p1.Y, l2p2.X, l2p2.Y);
        }

        private static bool LineIntersect(int l1X, int l1Y, int l1X2, int l1Y2, int l2X, int l2Y,  int l2X2, int l2Y2 )
        {
            //return false;
            float x1 = l1X, x2 = l1X2, x3 = l2X, x4 = l2X2;
            float y1 = l1Y, y2 = l1Y2, y3 = l2Y, y4 = l2Y2;

            float d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            // If d is zero, there is no intersection
            if (d == 0.0) return false;

            // Get the x and y
            float pre = (x1 * y2 - y1 * x2), post = (x3 * y4 - y3 * x4);
            float x = (pre * (x3 - x4) - (x1 - x2) * post) / d;
            float y = (pre * (y3 - y4) - (y1 - y2) * post) / d;

            // Check if the x and y coordinates are within both lines
            /*if (x < Math.Min(x1, x2) || x > Math.Max(x1, x2) ||
                x < Math.Min(x3, x4) || x > Math.Max(x3, x4)
                ) return false;
            if (y < Math.Min(y1, y2) || y > Math.Max(y1, y2) ||
            y < Math.Min(y3, y4) || y > Math.Max(y3, y4)) return false;
            */

            return !(
                (x < x1 && x < x2) || (x > x1 && x > x2) || (x < x3 && x < x4) || (x > x3 && x > x4) ||
                (y < y1 && y < y2) || (y > y1 && y > y2) || (y < y3 && y < y4) || (y > y3 && y > y4)
                );

            //    return false;    

            //return true;
           
        
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
            if(this.Points.Length < 3)
                return 0;

            long minX = long.MaxValue,minY = long.MaxValue;
            long maxX = 0,maxY = 0;

            for (int i = 0; i < 3; i++)
            {
                int x = this.Points[i].X;
                int y = this.Points[i].Y;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            return ((maxX - minX + 1) * (maxY - minY + 1)) / 2;
        }

      
    }
}