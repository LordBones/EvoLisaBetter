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
        public double Diff_AvgB;
        public double Diff_AvgG;
        public double Diff_AvgR;

        public double Diff_MedB;
        public double Diff_MedG;
        public double Diff_MedR;

        public double Diff_MedStdDevB;
        public double Diff_MedStdDevG;
        public double Diff_MedStdDevR;

        public double Diff_AvgStdDevB;
        public double Diff_AvgStdDevG;
        public double Diff_AvgStdDevR;


        public double ComplexErrorAvg
        {
            get { return Diff_AvgB + Diff_AvgG + Diff_AvgR + Diff_AvgStdDevB * 2 + Diff_AvgStdDevG * 2 + Diff_AvgStdDevR * 2; }
        }

        public double ComplexErrorMed
        {
            get { return Diff_MedB + Diff_MedG + Diff_MedR + Diff_MedStdDevB * 2 + Diff_MedStdDevG * 2 + Diff_MedStdDevR * 2; }
        }


        public MatchStatistics()
        {

        }

        public void ComputeImageMatchStatAvg(CanvasBGRA source, CanvasBGRA dest)
        {
            ComputeStatChanelsAvg(source, dest);
        }

        public void ComputeImageMatchStatMed(CanvasBGRA source, CanvasBGRA dest)
        {
            ComputeStatChanelsMedian(source, dest);
        }

        private void ComputeStatChanelsMedian(CanvasBGRA source, CanvasBGRA dest)
        {
            Median8bit medR = new Median8bit();
            Median8bit medG = new Median8bit();
            Median8bit medB = new Median8bit();


            int index = 0;
            while (index < source.Length)
            {
                medB.InsertData((byte)Tools.fastAbs(source.Data[index] - dest.Data[index]));
                medG.InsertData((byte)Tools.fastAbs(source.Data[index+1] - dest.Data[index+1]));
                medR.InsertData((byte)Tools.fastAbs(source.Data[index+2] - dest.Data[index+2]));
                index += 4;
            }

            Diff_MedB = medB.Median;
            Diff_MedG = medG.Median;
            Diff_MedR = medR.Median;


            Diff_MedStdDevB = medB.StdDev;
            Diff_MedStdDevG = medG.StdDev;
            Diff_MedStdDevR = medR.StdDev;
        }

        private void ComputeStatChanelsAvg(CanvasBGRA source, CanvasBGRA dest)
        {
            Median8bit med = new Median8bit();
            

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


            Diff_AvgB = sumB / (double)source.CountPixels;
            Diff_AvgG = sumG / (double)source.CountPixels;
            Diff_AvgR = sumR / (double)source.CountPixels;

           
            // spocteni prumerne std odchylky
            index = 0;

            double sumStdDevB = 0;
            double sumStdDevG = 0;
            double sumStdDevR = 0;
            while (index < source.Length)
            {
                sumStdDevB += Math.Abs(Tools.fastAbs(source.Data[index] - dest.Data[index])-this.Diff_AvgB);
                sumStdDevG += Math.Abs(Tools.fastAbs(source.Data[index + 1] - dest.Data[index + 1]) - this.Diff_AvgG);
                sumStdDevR += Math.Abs(Tools.fastAbs(source.Data[index + 2] - dest.Data[index + 2])-this.Diff_AvgR);
                index += 4;
            }

            Diff_AvgStdDevB = sumStdDevB / source.CountPixels;
            Diff_AvgStdDevG = sumStdDevG / source.CountPixels;
            Diff_AvgStdDevR = sumStdDevR / source.CountPixels;
        }
    }
}
