// This is the main DLL file.

#include "stdafx.h"

#include "GenArt.CoreNative.h"

__int64 GenArtCoreNative::Class1::computeFittness(unsigned char * curr, unsigned char * orig, int length)
		{
			__int64 result = 0;

			/*for(int index = 0;index < length;index+=4)
			{
				int br = curr[index] - orig[index];
                int bg = curr[index+1] - orig[index+1];
                int bb = curr[index+2] - orig[index+2];

				int tmpres = (int)sqrt((double)((br * br + bg * bg + bb * bb) / 3)) * 3;
                result += tmpres;

			}*/

			for(int index = 0;index < length;index+=4)
			{
				int br = abs(curr[index] - orig[index]);
                int bg = abs(curr[index+1] - orig[index+1]);
                int bb = abs(curr[index+2] - orig[index+2]);

				int tmpres = br + bg + bb;
                result += tmpres;

			}
			

			return result;
		}

__forceinline unsigned char ApplyColor(int colorChanel, int axrem, int rem)
{
    return (unsigned char)((axrem + rem * colorChanel) >> 16);
}

void GenArtCoreNative::Class1::FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
{
    /*while(from <= to)
    {
        int index = from;
         canvas[index] = ApplyColor(canvas[index], colorABRrem, colorRem);
                    canvas[index + 1] = ApplyColor(canvas[index + 1], colorAGRrem, colorRem);
                    canvas[index + 2] = ApplyColor(canvas[index + 2], colorARRrem, colorRem);

                    from += 4;
    }*/

    canvas = canvas + from;

    while(from <= to)
    {
         *canvas = ApplyColor(*canvas, colorABRrem, colorRem);
                    canvas[1] = ApplyColor(canvas[1], colorAGRrem, colorRem);
                    canvas[2] = ApplyColor(canvas[2], colorARRrem, colorRem);

                    from += 4;
                    canvas +=4;

    }
}

