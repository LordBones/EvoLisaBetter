using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.Classes;

namespace GenArt.Core.Classes
{
    public class MatchStatistics
    {
        public double ChDiff_AvgB;
        public double ChDiff_AvgG;
        public double ChDiff_AvgR;

        public double ChDiff_StdDevB;
        public double ChDiff_StdDevG;
        public double ChDiff_StdDevR;

        public double ComplexError
        {
            get { return ChDiff_AvgB + ChDiff_AvgG + ChDiff_AvgR + ChDiff_StdDevB * 2 + ChDiff_StdDevG * 2 + ChDiff_StdDevR * 2; }
        }

        public MatchStatistics()
        {

        }

        public void ComputeImageMatchStat(CanvasBGRA source, CanvasBGRA dest)
        {
            ComputeStatChanels(source, dest);
        }

        private void ComputeStatChanels(CanvasBGRA source, CanvasBGRA dest)
        {
            long sumR = 0;
            long sumG = 0;
            long sumB = 0;

            int index = 0;
            while (index < source.Length)
            {
                sumB += Tools.fastAbs(source.Data[index] - dest.Data[index]);
                sumG += Tools.fastAbs(source.Data[index+1] - dest.Data[index+1]);
                sumR += Tools.fastAbs(source.Data[index+2] - dest.Data[index+2]);
                index += 4;
            }


            ChDiff_AvgB = sumB / (double)source.CountPixels;
            ChDiff_AvgG = sumG / (double)source.CountPixels;
            ChDiff_AvgR = sumR / (double)source.CountPixels;

           
            // spocteni prumerne std odchylky
            index = 0;

            double sumStdDevB = 0;
            double sumStdDevG = 0;
            double sumStdDevR = 0;
            while (index < source.Length)
            {
                sumStdDevB += Math.Abs(Tools.fastAbs(source.Data[index] - dest.Data[index])-this.ChDiff_AvgB);
                sumStdDevG += Math.Abs(Tools.fastAbs(source.Data[index + 1] - dest.Data[index + 1]) - this.ChDiff_AvgG);
                sumStdDevR += Math.Abs(Tools.fastAbs(source.Data[index + 2] - dest.Data[index + 2])-this.ChDiff_AvgR);
                index += 4;
            }

            ChDiff_StdDevB = sumStdDevB / source.CountPixels;
            ChDiff_StdDevG = sumStdDevG / source.CountPixels;
            ChDiff_StdDevR = sumStdDevR / source.CountPixels;
        }
    }
}
