#include "stdafx.h"
#include <math.h>
#include "NativeMedian8bit.h"

//#pragma managed(push, off)
NativeMedian8Bit::NativeMedian8Bit(void)
{
    for (int index=0; index < CONST_MedianTableSize; index++)
            {
                _medianTable[index] = 0;
            }
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


 int FastFunctions::ApplyColor(int colorChanel, int axrem, int rem)
{
    return ((axrem + rem * colorChanel) >> 16);
}

void FastFunctions::FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
{
    //unsigned int * ptrColor = (unsigned int *)(canvas+from);

	//int count = (to - from)>>2;

	// while(count>=0)
 //   {
 //       
	//	unsigned int Color = ptrColor[count];
	//	unsigned int res = Color&0xff000000;

	//	res |= ApplyColor((Color>>16)&0xFF, colorARRrem, colorRem) << 16; // r
	//	unsigned int G = (Color>>8)&0xFF;
	//	res |= ApplyColor(G, colorAGRrem, colorRem)<<8;
	//	unsigned int B = Color&0xFF;
	//	

 //       res |= ((colorABRrem + colorRem * B)>> 16);// ApplyColor(B, colorABRrem, colorRem);
 //       
 //       

	//	ptrColor[count] = res; // (Color&0xff000000) | (R<<16) | (G << 8) | B;

	//	
	//	count--;
 //       
 //   }



    while(from <= to)
    {
        int index = from;
         canvas[index] = (unsigned char)FastFunctions::ApplyColor(canvas[index], colorABRrem, colorRem);
                    canvas[index + 1] = (unsigned char)FastFunctions::ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    canvas[index + 2] = (unsigned char)FastFunctions::ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                    from += 4;
    }

   /* canvas = canvas + from;

    while(from <= to)
    {
         *canvas = ApplyColor(*canvas, colorABRrem, colorRem);
                    canvas[1] = ApplyColor(canvas[1], colorAGRrem, colorRem);
                    canvas[2] = ApplyColor(canvas[2], colorARRrem, colorRem);

                    from += 4;
                    canvas +=4;

    }*/
}

//#pragma managed(pop)