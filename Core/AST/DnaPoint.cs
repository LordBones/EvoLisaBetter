using System;
using GenArt.Classes;

namespace GenArt.AST
{
    [Serializable]
    public struct DnaPoint
    {
        public short X;// { get; set; }
        public short Y;// { get; set; }

        public DnaPoint(short x, short y)
        {
            this.X = x;
            this.Y = y;
        }

        public static bool Compare(DnaPoint p1, DnaPoint p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public void Init()
        {
            X = (short)Tools.GetRandomNumber(0, Tools.MaxWidth);
            Y = (short)Tools.GetRandomNumber(0, Tools.MaxHeight);
        }

        //public DnaPoint Clone()
        //{
        //    //return new DnaPoint
        //    //           {
        //    //               X = X,
        //    //               Y = Y,
        //    //           };
        //    return new DnaPoint(X,Y);
        //}

        public void MutateWholeRange()
        {
            X = (short)Tools.GetRandomNumber(0, Tools.MaxWidth );
            Y = (short)Tools.GetRandomNumber(0, Tools.MaxHeight );
        }

        public void MutateMiddle()
        {
            int tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMid);

            X = (short)Math.Min(Math.Max(0,
                   X + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxWidth - 1);
            tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMid);

            Y = (short)Math.Min(Math.Max(0,
                   Y + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxHeight - 1);

        }

        public void MutateSmall()
        {
            int tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMin);

            X = (short)Math.Min(Math.Max(0,
                   X + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxWidth - 1);
            tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMin);

            Y = (short)Math.Min(Math.Max(0,
                   Y + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxHeight - 1);

        }

        public bool Mutate()
        {
            bool result = false;
            if (Tools.WillMutate(Settings.ActiveMovePointMaxMutationRate))
            {
                X = (short)Tools.GetRandomNumber(0, Tools.MaxWidth - 1);
                Y = (short)Tools.GetRandomNumber(0, Tools.MaxHeight - 1);
                return result = true;
            }

            if (Tools.WillMutate(Settings.ActiveMovePointMidMutationRate))
             {
                 int tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMid);

                 X = (short)Math.Min(Math.Max(0,
                        X + ((Tools.GetRandomNumber(0,1000)> 500 ) ? -tmp : tmp)), Tools.MaxWidth - 1);
                 tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMid);

                 Y = (short)Math.Min(Math.Max(0,
                        Y + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxHeight - 1);


                 //X =
                 //    Math.Min(
                 //        Math.Max(0,
                 //                 X +
                 //                 Tools.GetRandomNumber(-Settings.ActiveMovePointRangeMid,
                 //                                       Settings.ActiveMovePointRangeMid)), Tools.MaxWidth-1);
                 //Y =
                 //    Math.Min(
                 //        Math.Max(0,
                 //                 Y +
                 //                 Tools.GetRandomNumber(-Settings.ActiveMovePointRangeMid,
                 //                                       Settings.ActiveMovePointRangeMid)), Tools.MaxHeight-1);
                 return result = true;
             }

             if (Tools.WillMutate(Settings.ActiveMovePointMinMutationRate))
             {
                 int tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMin);

                 X = (short)Math.Min(Math.Max(0,
                        X + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxWidth - 1);
                 tmp = Tools.GetRandomNumber(1, Settings.ActiveMovePointRangeMin);

                 Y = (short)Math.Min(Math.Max(0,
                        Y + ((Tools.GetRandomNumber(0, 1000) > 500) ? -tmp : tmp)), Tools.MaxHeight - 1);

                 return result = true;
             }
             
            return result;
        }
    }
}