// GenArt.CoreNative.h

#pragma once
#include <math.h>
#include <vcclr.h> 
using namespace System;
using namespace System::Drawing;

namespace GenArtCoreNative {

	public ref class NativeFunctions
	{

		private:

	    void _ClearFieldByColor(unsigned char * curr, int length, int color);

		__int64 computeFittness(unsigned char * curr, unsigned char * orig, int length);
		__int64 computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length);


        void FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem);

		

		// TODO: Add your methods for this class here.
	public :

		void ClearFieldByColor(array<System::Byte>^ canvas, int color)
		{
			pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			_ClearFieldByColor(pinCanvas, canvas->Length, color);

		}

        void RowApplyColor(array<System::Byte>^ canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

            FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);
        }

		__int64 ComputeFittness(array<System::Byte>^ current, array<System::Byte>^ orig)
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

            result = computeFittness(pinCurr,pinOrig,orig->Length);

			//result = computeFittnessWithStdDev(pinCurr,pinOrig,orig->Length);


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
