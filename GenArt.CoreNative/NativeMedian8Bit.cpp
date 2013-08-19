//#include "stdafx.h"
#include <math.h>
#include <cstring>
#include "NativeMedian8bit.h"

//#pragma unmanaged
//#pragma managed(push, off)
NativeMedian8Bit::NativeMedian8Bit(void)
{
	memset(_medianTable,0,sizeof(long)*CONST_MedianTableSize);
  /*  for (int index=0; index < CONST_MedianTableSize; index++)
            {
                _medianTable[index] = 0;
            }*/
}


NativeMedian8Bit::~NativeMedian8Bit(void)
{
}

 unsigned char NativeMedian8Bit::ComputeMedian()
 {
     __int64 totalLength = TotalCount();
     __int64 halfLength = totalLength / 2 + (totalLength & 1);

     if (totalLength == 0) return 0;


     __int64 sum = 0;
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

                     return (startValue + index) >> 1;
                 }
             }
             else  // median je toto cislo
             {
                 return index; ;
             }
         }

         return 0;
 }

 __int64 NativeMedian8Bit::ComputeSumStdDev()
 {
     int Median = ComputeMedian();

            __int64 sum = 0;
          
            for (int index = 0; index < CONST_MedianTableSize; index++)
            {
                int dataCount = _medianTable[index];
                if (dataCount > 0)
                {
                    sum += labs(index - Median) * dataCount;
                    
                }
            }

            return sum;
 }


   



//#pragma managed  
//#pragma managed(pop)