// This is the main DLL file.

#include "stdafx.h"
#include "string.h"
#include "NativeMedian8bit.h"
#include <iostream>

#include "GenArt.CoreNative.h"

//#define FastAbs(x) ((x^(x>>31))-(x>>31))

int FastAbs(int data)
{
    int topbitreplicated = data >> 31;
    return (data ^ topbitreplicated) - topbitreplicated;  
}

__int64 GenArtCoreNative::NativeFunctions::computeFittness(unsigned char * curr, unsigned char * orig, int length)
		{
			__int64 result = 0;

			for(int index = 0;index < length;index+=4)
			{
				int br = curr[index] - orig[index];
                int bg = curr[index+1] - orig[index+1];
                int bb = curr[index+2] - orig[index+2];
				br*=2;
				bg*=7;

				int tmpres = br * br + bg * bg + bb * bb;
                result += tmpres;
				 
			}

			/*for(int index = 0;index < length;index+=4)
			{
				int br = FastAbs(curr[index] - orig[index]);
                int bg = FastAbs(curr[index+1] - orig[index+1]);
                int bb = FastAbs(curr[index+2] - orig[index+2]);

                br = br*2126;
                bg = bg*7152;
                bb = bb*722;
				int tmpres = ((br + bg + bb)/10000);
                result += tmpres;

			}*/

            /*for(int index = 0;index < length;index+=4)
			{
				int br = FastAbs(curr[index] - orig[index]);
                int bg = FastAbs(curr[index+1] - orig[index+1]);
                int bb = FastAbs(curr[index+2] - orig[index+2]);

                result += br+bg+bb;
                
				//int tmpres = (br + bg + bb);
                //result += tmpres;

			}*/

            
			

			return result;
		}

__int64 GenArtCoreNative::NativeFunctions::computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length)
		{
            NativeMedian8Bit medR = NativeMedian8Bit();
            NativeMedian8Bit medG = NativeMedian8Bit();
            NativeMedian8Bit medB = NativeMedian8Bit();

            
            int index = 0;
            while (index < length)
            {
                medB.InsertData(FastAbs(curr[index] - orig[index]));
                medG.InsertData(FastAbs(curr[index + 1] - orig[index + 1]));
                medR.InsertData(FastAbs(curr[index + 2] - orig[index + 2]));
                index += 4;
            }


            __int64 result = 0;
            result += (medB.ValueSum() + medB.SumStdDev()*2);
            result += (medG.ValueSum() + medG.SumStdDev() * 2);
            result += (medR.ValueSum() + medR.SumStdDev() * 2);

            return result;

			
		}


__forceinline unsigned int ApplyColor(int colorChanel, int axrem, int rem)
{
    return ((axrem + rem * colorChanel) >> 16);
}

void GenArtCoreNative::NativeFunctions::FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
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
         canvas[index] = ApplyColor(canvas[index], colorABRrem, colorRem);
                    canvas[index + 1] = ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    canvas[index + 2] = ApplyColor(canvas[index + 2], colorARRrem, colorRem);

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

void FillSSEInt32(unsigned long * M, long Fill, unsigned int Count)
{
    __m128i f;

    // Fix mis-alignment.
    if (((unsigned int)M) & 0xf)
    {
		unsigned int tmp = ((unsigned int)M) & 0xf; 
		
        switch (tmp)
        {
            case 0x4: if (Count >= 1) { *M++ = Fill; Count--; }
            case 0x8: if (Count >= 1) { *M++ = Fill; Count--; }
            case 0xc: if (Count >= 1) { *M++ = Fill; Count--; }
        }
    }

    f.m128i_i32[0] = Fill;
    f.m128i_i32[1] = Fill;
    f.m128i_i32[2] = Fill;
    f.m128i_i32[3] = Fill;

	while (Count >= 4)
    {
        _mm_store_si128((__m128i *)M, f);
        M += 4;
        Count -= 4;
	}

	// Fill remaining LONGs.
    switch (Count & 0x3)
    {
        case 0x3: *M++ = Fill;
        case 0x2: *M++ = Fill;
        case 0x1: *M++ = Fill;
    }
}

void GenArtCoreNative::NativeFunctions::_ClearFieldByColor(unsigned char * curr, int length, int color)
{
	FillSSEInt32((unsigned long *)curr, color,length/4);
	//std::fill((unsigned int *)curr,((unsigned int *)curr)+length/4,  color);
}

