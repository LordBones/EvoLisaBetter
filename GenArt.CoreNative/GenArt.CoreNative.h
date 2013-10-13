// GenArt.CoreNative.h

#pragma once
#include <vcclr.h>
#include "FastFunctions.h"

using namespace System;
//using namespace System::Drawing;

namespace GenArtCoreNative {

	public ref class NativeFunctions
	{

		private:


		// TODO: Add your methods for this class here.
	public :

		void ClearFieldByColor(array<System::Byte>^ canvas, int color)
		{
			pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
			ClearFieldByColor(pinCanvas, canvas->Length, color);

		}

        void NewRowApplyColor(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
            NewFastRowApplyColorSSE(pinCanvas+startPixelIndex,countPixel, color, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void NewRowApplyColor64(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
            NewFastRowApplyColorSSE64(pinCanvas+startPixelIndex,countPixel, color, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void NewRowApplyColor128(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
            NewFastRowApplyColorSSE128(pinCanvas+startPixelIndex,countPixel, color, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void RowApplyColor(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int r , int g, int b, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                FastRowApplyColor(pinCanvas+startPixelIndex,countPixel,  r ,  g, b, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void RowApplyColorSSE64(array<System::Byte>^ canvas,int startPixelIndex, int countPixel, int r , int g, int b, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                FastRowApplyColorSSE64(pinCanvas+startPixelIndex,countPixel,r,g,b,alpha);

    
        }

        void RowApplyColorSSE128(array<System::Byte>^ canvas, int from, int to, int r , int g, int b, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                FastRowApplyColorSSE128(pinCanvas+from,to-from+1,r,g,b,alpha);

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

        void RowApplyColorBetter(array<System::Byte>^ canvas,int canvasWidth, array<System::Int16>^ ranges, int startY, int endY, int r, int g, int b, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
			pin_ptr<System::Int16> pinRanges(&ranges[0]);

            int countPolygonRows = (endY-startY+1)*2;
			FastFunctions::
                FastRowsApplyColorSSE64(pinCanvas,canvasWidth,pinRanges+startY*2,countPolygonRows, startY, r, g, b, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        __int64 ComputeFittnessAdvance(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

            
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
