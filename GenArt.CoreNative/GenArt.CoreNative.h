// GenArt.CoreNative.h

#pragma once
#include <math.h>
#include <vcclr.h>
#include "FastFunctions.h"

using namespace System;
//using namespace System::Drawing;

namespace GenArtCoreNative {

	public ref class NativeFunctions
	{

		private:

		__int64 computeFittness(unsigned char * curr, unsigned char * orig, int length);
		__int64 computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length);



		// TODO: Add your methods for this class here.
	public :

		void ClearFieldByColor(array<System::Byte>^ canvas, int color)
		{
			pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
			ClearFieldByColor(pinCanvas, canvas->Length, color);

		}

        void RowApplyColor(array<System::Byte>^ canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[from]);

			FastFunctions::
            FastRowApplyColor(pinCanvas,to-from+1,colorABRrem,colorAGRrem,colorARRrem,colorRem);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

		void RowApplyColorBetter(array<System::Byte>^ canvas,int canvasWidth, array<System::Int16>^ ranges, int startY, int r, int g, int b, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
			pin_ptr<System::Int16> pinRanges(&ranges[0]);


			FastFunctions::
                FastRowsApplyColorSSE128(pinCanvas,canvasWidth,pinRanges,ranges->Length, startY, r, g, b, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        __int64 ComputeFittnessAdvance(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

            //return computeFittnessWithStdDev(pinCurr,pinOrig,orig->Length);

            return  FastFunctions::computeFittnessWithStdDev(pinCurr,pinOrig,orig->Length);
        }

		__int64 ComputeFittness(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr = &current[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

			
			

            result = FastFunctions::computeFittnessSumSquareASM(pinCurr,pinOrig,orig->Length);
            
			return result;
		}
	};
}
