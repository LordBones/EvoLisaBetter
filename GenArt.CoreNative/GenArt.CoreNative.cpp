// This is the main DLL file.

#include "stdafx.h"
#include "string.h"
#include <iostream>

#include "GenArt.CoreNative.h"

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
			__int64 result = 0;

			_int64 sumR = 0;
			_int64 sumG = 0;
			_int64 sumB = 0;


			for(int index = 0;index < length;index+=4)
			{
				sumR +=  FastAbs(curr[index] - orig[index]);
                sumG += FastAbs(curr[index+1] - orig[index+1]);
                sumB += FastAbs(curr[index+2] - orig[index+2]);
			}


			_int64 sumAvgR = sumR / (length *4);
			_int64 sumAvgG = sumG / (length *4);
			_int64 sumAvgB = sumB / (length *4);

			_int64 sumDiffR = 0;
			_int64 sumDiffG = 0;
			_int64 sumDiffB = 0;

			for(int index = 0;index < length;index+=4)
			{
				sumDiffR += FastAbs( FastAbs(curr[index] - orig[index]) - sumAvgR);
                sumDiffG += FastAbs(FastAbs(curr[index+1] - orig[index+1]) - sumAvgG);
                sumDiffB += FastAbs(FastAbs(curr[index+2] - orig[index+2]) - sumAvgB);
			}

			result += sumR + sumG+sumB;
			result += sumDiffR + sumDiffG+sumDiffB;
			

			return result;
		}


__forceinline unsigned char ApplyColor(int colorChanel, int axrem, int rem)
{
    return (unsigned char)((axrem + rem * colorChanel) >> 16);
}

void GenArtCoreNative::NativeFunctions::FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
{
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

