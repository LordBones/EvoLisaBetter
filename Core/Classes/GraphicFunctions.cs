using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;

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
            float beta = (denominator == 0)? 1 :  ((p3.Y - p1.Y) * (p.X - p3.X) + (p1.X - p3.X) * (p.Y - p3.Y)) /
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
    }
}
