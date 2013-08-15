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


   
 int FastFunctions::ApplyColor(int colorChanel, int axrem, int rem)
{
    return ((axrem + rem * colorChanel) >> 16);
}

void FastFunctions::FastRowApplyColor(unsigned char * canvas, int len, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
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


    /*
    while(from <= to)
    {
        int index = from;
         canvas[index] = (unsigned char)FastFunctions::ApplyColor(canvas[index], colorABRrem, colorRem);
                    canvas[index + 1] = (unsigned char)FastFunctions::ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    canvas[index + 2] = (unsigned char)FastFunctions::ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                    from += 4;
    }*/
	
    while(len > 0)
    {
        
        //((axrem + rem * colorChanel) >> 16)
        unsigned int b = *canvas;
        unsigned int g = canvas[1];
		unsigned int r = canvas[2];
        
        b*=colorRem;
        g*=colorRem;
		r*=colorRem;
        
        b+=colorABRrem;
        g+=colorAGRrem;
		r+=colorARRrem;

        b>>=16;
        g>>=16;
		r>>=16;

        *canvas = (unsigned char)b;
        canvas[1] = (unsigned char)g;
		
        
		
        
		canvas[2] = (unsigned char)r;
        
  
        len -= 4;
		canvas+=4;
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


  int __fastcall FastAbs2(int data)
{
    int topbitreplicated = data >> 31;
    return (data ^ topbitreplicated) - topbitreplicated;  
}

__int64 FastFunctions::computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length)
		{
            NativeMedian8Bit medR = NativeMedian8Bit();
            NativeMedian8Bit medG = NativeMedian8Bit();
            NativeMedian8Bit medB = NativeMedian8Bit();

            
            //int index = 0;
            //while (index < length)
			for(int index = 0;index < length;index+=4)
            {
                medB.InsertData(labs(curr[index] - orig[index]));
                medG.InsertData(labs(curr[index + 1] - orig[index + 1]));
                medR.InsertData(labs(curr[index + 2] - orig[index + 2]));
              //  index += 4;
            }


            __int64 result = 0;
            result += (medB.ValueSum() + medB.SumStdDev()*2);
            result += (medG.ValueSum() + medG.SumStdDev() * 2);
            result += (medR.ValueSum() + medR.SumStdDev() * 2);

            return result;

			
		}




//#pragma managed  
//#pragma managed(pop)