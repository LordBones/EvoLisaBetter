// This is the main DLL file.

#include "stdafx.h"

#include "GenArt.CoreNative.h"

__int64 _fastcall GenArtCoreNative::Class1::computeFittness(unsigned char * curr, unsigned char * orig, int length)
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

