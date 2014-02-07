using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Classes;

namespace GenArt.Core.Classes
{
    public class GraphicFunctions
    {
        public static bool IsPointInTriangle2(DnaPoint p1, DnaPoint p2, DnaPoint p3, DnaPoint p)
        {
            int denominator = ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
            float alpha = (denominator == 0) ? 1 :
                ((p2.Y - p3.Y) * (p.X - p3.X) + (p3.X - p2.X) * (p.Y - p3.Y)) /
                denominator;

            denominator = ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
            float beta = (denominator == 0) ? 1 : ((p3.Y - p1.Y) * (p.X - p3.X) + (p1.X - p3.X) * (p.Y - p3.Y)) /
                   denominator;
            float gamma = 1.0f - alpha - beta;

            return alpha > 0.0 && beta > 0.0 && gamma > 0.0;
        }

        public static bool IsPointInTriangle(DnaPoint a, DnaPoint b, DnaPoint c, DnaPoint p)
        {
            int as_x = p.X - a.X;
            int as_y = p.Y - a.Y;

            bool s_ab = (b.X - a.X) * as_y - (b.Y - a.Y) * as_x > 0;

            if ((c.X - a.X) * as_y - (c.Y - a.Y) * as_x > 0 == s_ab) return false;

            if ((c.X - b.X) * (p.Y - b.Y) - (c.Y - b.Y) * (p.X - b.X) > 0 != s_ab) return false;

            return true;
        }

        public static bool LineIntersect(DnaPoint l1p1, DnaPoint l1p2, DnaPoint l2p1, DnaPoint l2p2)
        {
            return LineIntersect(l1p1.X, l1p1.Y, l1p2.X, l1p2.Y, l2p1.X, l2p1.Y, l2p2.X, l2p2.Y);
        }

        private static bool LineIntersect(int l1X, int l1Y, int l1X2, int l1Y2, int l2X, int l2Y, int l2X2, int l2Y2)
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

        public static float GetDistancePointFromLine(int lineX0, int lineY0, int lineX1, int lineY1, int pointX, int pointY)
        {
            int v0x,v0y, c;

            v0x = lineX1 - lineX0;//-1
            v0y = lineY1 - lineY0;//1
            
            c = -(v0x * lineX0 + v0y * lineY0);

            // distance = abs(a*pointx+b*pointy+c)/sqrt(a*a+b*b)
            int tmp = Tools.fastAbs(v0x * pointX + v0y * pointY + c);

            float result = (float)(tmp / Math.Sqrt(v0x * v0x + v0y * v0y));
            return result;
        }

        private const double CONST_ANGLE = System.Math.PI * (20 / 180.0);
        private const double CONST_ANGLE_Inverse = System.Math.PI * 2 - CONST_ANGLE;

        private static double PointsAngle2(double px1, double py1, double px2, double py2)
        {
            Double Angle = Math.Atan2(py1 - 0, px1 - 0) - Math.Atan2(py2 - 0, px2 - 0);

            return Angle;
        }
        public static bool LinesHasMinimalAngle(int x1, int y1, int x2, int y2, int middleX, int middleY)
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

        public static bool TriangleHasNotSmallAngle(int x0,int y0,int x1,int y1,int x2,int y2)
        {
            if (!LinesHasMinimalAngle(x0, y0, x2, y2, x1, y1)) return false;
            if (!LinesHasMinimalAngle(x2, y2, x1, y1,x0, y0)) return false;
            if (!LinesHasMinimalAngle(x0, y0, x1, y1, x2, y2)) return false;

            return true;
        }
    }
}
