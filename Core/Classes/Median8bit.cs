using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.Classes;

namespace GenArt.Core.Classes
{
    public class Median8bit
    {
        private const int CONST_MedianTableSize = 256;
        private int [] _medianTable = new int[CONST_MedianTableSize];


        #region property

        public long TotalCount
        {
            get
            {
                long sum = 0;
                for (int index = 0; index < CONST_MedianTableSize; index++) sum += _medianTable[index];
                return sum;
            }
        }

        public long TotalSum
        {
            get
            {
                long sum = 0;
                for (int index = 0; index < CONST_MedianTableSize; index++) sum += _medianTable[index]*index;
                return sum;
            }
        }

        public double Median
        {
            get { return ComputeMedian(); }
        }

        public double SumStdDev
        {
            get{return ComputeSumStdDev();}
        }

        public double StdDev
        {
            get { return ComputeSumStdDev()/TotalCount; }
        }

      

        #endregion


        public Median8bit()
        {

        }

        public void Clear()
        {
            for (int index=0; index < CONST_MedianTableSize; index++)
            {
                _medianTable[index] = 0;
            }
        }

        public void InsertData(byte data)
        {
            _medianTable[data]++;
        }

        private double ComputeSumStdDev()
        {
            double Median = ComputeMedian();

            double sum = 0.0;
            long totalcount = 0;

            for (int index = 0; index < CONST_MedianTableSize; index++)
            {
                int dataCount = _medianTable[index];
                if (dataCount > 0)
                {
                    sum += Math.Abs(index - Median) * dataCount;
                    totalcount += dataCount;
                }
            }

            return sum;
        }

        private double ComputeMedian()
        {
            long totalLength = TotalCount;
            long halfLength = totalLength / 2 + (totalLength & 1);

            if (totalLength == 0) return 0.0;


            long sum = 0;
            int index = 0;

            if (totalLength == 1)
            {
                while (_medianTable[index] == 0) index++;
                return index;
            }

            while (index < CONST_MedianTableSize)
            {
                int dataCount = _medianTable[index];
                if (dataCount + sum < halfLength)
                {
                    sum += dataCount;
                    index++;
                }
                else
                    break;
            }

            if (index < CONST_MedianTableSize)
            {

                // median nalezen, ale mozna je potreba ho spocitat z dlasiho indexu
                if (_medianTable[index] + sum == halfLength)
                {
                    // if not odd
                    if ((totalLength & 1) == 1) return index;
                    else
                    {
                        int startValue = index;
                        index++;
                        // find next index with not zero count
                        while (_medianTable[index] == 0) index++;

                        return (startValue + index) / 2.0;
                    }
                }
                else  // median je toto cislo
                {
                    return index; ;
                }
            }


            return 0.0;
        }
    }
}
