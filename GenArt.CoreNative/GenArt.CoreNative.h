// GenArt.CoreNative.h

#pragma once
#include <math.h>
#include <vcclr.h> 
using namespace System;

namespace GenArtCoreNative {

	public ref class Class1
	{

		private:
		__int64 _fastcall computeFittness(unsigned char * curr, unsigned char * orig, int length);
		

		// TODO: Add your methods for this class here.
	public :
		__int64 ComputeFittness(array<System::Byte>^ current, array<System::Byte>^ orig, int length)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

			
			/*for(int index = 0;index < length;index+=4)
			{
				int br = pinCurr[index] - pinOrig[index];
                int bg = pinCurr[index+1] - pinOrig[index+1];
                int bb = pinCurr[index+2] - pinOrig[index+2];

				int tmpres = (int)sqrt((double)(br * br + bg * bg + bb * bb) / 3) * 3;
                result += tmpres;

			}*/

			result += computeFittness(pinCurr,pinOrig,length);
			

			/*for(int index = 0;index < length;index+=4)
			{
				int br = current[index] - orig[index];
                int bg = current[index+1] - orig[index+1];
                int bb = current[index+2] - orig[index+2];

				int tmpres = (int)sqrt((double)(br * br + bg * bg + bb * bb) / 3) * 3;
                result += tmpres;

			}*/

			return result;
		}
	};
}
