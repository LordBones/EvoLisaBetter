using GenArt.AST;
using GenArt.Classes;
using GenArt.Core.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenArt.Core.AST
{
    public static class DnaPolygonBO
    {

        public static void InitNewTriangle(DnaPolygon poly, byte mutationRate,CanvasARGB _rawDestImage = null)
        {
            poly.Brush = new DnaBrush(
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256));

            var origin = new DnaPoint();



            origin.Init(_rawDestImage.WidthPixel, _rawDestImage.HeightPixel);
            DnaPoint[] points = new DnaPoint[3];

            for (int i = 0; i < points.Length; i ++)
            {
                var point = new DnaPoint();
                point = origin;
                Tools.MutatePointByRadial(ref point.X, ref point.Y, (short)(_rawDestImage.WidthPixel - 1), (short)(_rawDestImage.HeightPixel - 1), 16);
                points[i] = point;
            }

            poly._Points = points;
        }

        public static void InitNewTriangleByEdge(DnaPolygon poly, byte mutationRate, ImageEdges edgePoints, CanvasARGB _rawDestImage = null)
        {
            poly.Brush = new DnaBrush(
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256));


            DnaPoint[] points = new DnaPoint[3];

            for (int i = 0; i < points.Length; i++)
            {
                
                points[i] =  edgePoints.GetRandomEdgePoint(); 
            }

            poly._Points = points;
        }

        public static void InitNewRectangle(DnaRectangle rect, byte mutationRate,  CanvasARGB _rawDestImage = null)
        {
            rect.Brush = new DnaBrush(
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256));

            var origin = new DnaPoint();

            origin.Init(_rawDestImage.WidthPixel, _rawDestImage.HeightPixel);
            
            var pointStart = new DnaPoint();
            pointStart = origin;
            Tools.MutatePointByRadial(ref pointStart.X, ref pointStart.Y, (short)(_rawDestImage.WidthPixel - 1), (short)(_rawDestImage.HeightPixel - 1), 16);

            var pointEnd = new DnaPoint();
            pointEnd = origin;
            Tools.MutatePointByRadial(ref pointEnd.X, ref pointEnd.Y, (short)(_rawDestImage.WidthPixel - 1), (short)(_rawDestImage.HeightPixel - 1), 16);
            
            rect.EndPoint = pointEnd;
            rect.StartPoint = pointStart;

            rect.RepairOrderAxis();
        }

        public static void InitNewRectangleByEdge(DnaRectangle rect, byte mutationRate, ImageEdges edgePoints, CanvasARGB _rawDestImage = null)
        {
            rect.Brush = new DnaBrush(
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256),
               (byte)Tools.GetRandomNumber(0, 256));

            var pointStart = edgePoints.GetRandomEdgePoint();
            
            var pointEnd = edgePoints.GetRandomEdgePoint();
           
            rect.EndPoint = pointEnd;
            rect.StartPoint = pointStart;

            rect.RepairOrderAxis();
        }
    }
}
