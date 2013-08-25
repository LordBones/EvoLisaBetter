// This is the main DLL file.

#include "stdafx.h"
#include "string.h"
#include "NativeMedian8bit.h"


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
				//br*=2;
				//bg*=7;

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


//#pragma managed(pop)



