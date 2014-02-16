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
			ClearFieldByColorInt(pinCanvas, canvas->Length/4, color);

		}

        void ClearFieldByColorInt(array<System::Byte>^ canvas,int startIndexPixel,  int countPixel, int color)
		{
			pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                ClearFieldByColorInt(pinCanvas+startIndexPixel, countPixel, color);

		}

        void ClearFieldByColor(array<System::Byte>^ canvas,int startIndex,  int count, System::Byte data)
		{
			pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                ClearFieldByColor(pinCanvas+startIndex, count, data);

		}

        void NewRowApplyColor(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
            NewFastRowApplyColorSSE(pinCanvas+startPixelIndex,countPixel, color);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void NewRowApplyColor64(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color, int alpha256)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                NewFastRowApplyColorSSE64(pinCanvas+startPixelIndex,countPixel, color,alpha256);
        }

        void NewRowApplyColor64(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
                NewFastRowApplyColorSSE64(pinCanvas+startPixelIndex,countPixel, color);
        }

        void NewRowApplyColor128(array<System::Byte>^ canvas, int startPixelIndex, int countPixel, int color)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

			FastFunctions::
            NewFastRowApplyColorSSE128(pinCanvas+startPixelIndex,countPixel, color);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

         void RenderRectangle(array<System::Byte>^ canvas, 
             int canvasWidth, int x,int y, int width, int height, int color, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);

            FastFunctions::RenderRectangle(pinCanvas, canvasWidth, x, y, width, height, color, alpha);

            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void RenderTriangleByRanges(array<System::Byte>^ canvas,int canvasWidth, array<System::Int16>^ ranges, int startY, int endY, int color, int alpha)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
			pin_ptr<System::Int16> pinRanges(&ranges[0]);


			//FastFunctions::
            //    FastRowsApplyColorSSE128(pinCanvas,canvasWidth,pinRanges,ranges->Length, startY, color, alpha);
            FastFunctions::RenderTriangleByRanges(pinCanvas,canvasWidth,
                &pinRanges[startY*2],(endY-startY)*2,startY,color,alpha);
            // FastFunctions2::FastRowApplyColor(pinCanvas,from,to,colorABRrem,colorAGRrem,colorARRrem,colorRem);

        }

        void RenderTriangle(array<System::Byte>^ canvas,int canvasWidth,int canvasHeight,
            short int px0,short int py0,short int px1,short int py1,short int px2,short int py2,int color)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
			
			FastFunctions::RenderTriangle(pinCanvas,canvasWidth,canvasHeight,
                px0,py0,px1,py1,px2,py2,color);
        }

        void RenderTriangleNew(array<System::Byte>^ canvas,int canvasWidth,int canvasHeight,
            short int px0,short int py0,short int px1,short int py1,short int px2,short int py2,int color)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
			
			FastFunctions::RenderTriangleNew(pinCanvas,canvasWidth,canvasHeight,
                px0,py0,px1,py1,px2,py2,color);
        }

        void RenderTriangleNewOptimize(array<System::Byte>^ canvas,int canvasWidth,int canvasHeight,
            short int px0,short int py0,short int px1,short int py1,short int px2,short int py2,int color)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
			
            FastFunctions::RenderTriangleNewOptimize(pinCanvas,canvasWidth,canvasHeight,
                px0,py0,px1,py1,px2,py2,color);
        }

        bool TriangleGetRowIntersect(int y, System::Int32% startX, System::Int32%  endX, 
            short int px0,short int py0,short int px1,short int py1,short int px2,short int py2)
        {
            int start = 0;
            int end = 0;
            bool result = FastFunctions::TriangleGetRowIntersect(y,&start,&end,
                px0,py0,px1,py1,px2,py2);

            startX = start;
            endX = end;
            return result;
        }


        ///
        /// listRowsForElements => format : [pixelStartIndex][count pixel][color]
        /// first is clear row
        /// countRows - sekvence others row from primites for render into same line
        void RenderOneRow(array<System::Int32>^ listRowsForApply, int countRows, array<System::Byte>^ canvas)
        {
            pin_ptr<System::Byte> pinCanvas(&canvas[0]);
            pin_ptr<System::Int32> pinListRows(&listRowsForApply[0]);

            FastFunctions::RenderOneRow(pinListRows,countRows,pinCanvas);
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

        __int64 ComputeFittnessAdvance_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

            
            return  FastFunctions::computeFittnessWithStdDev_ARGB(pinCurr,pinOrig,orig->Length);
        }
        __int64 ComputeFittnessTile_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig, int widthPixel)
		{
			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

            
            return  FastFunctions::computeFittnessTile_ARGB(pinCurr,pinOrig,orig->Length,widthPixel);
        }

        __int64 ComputeFittness_2d_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig, int width)
		{
			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

            
            return  FastFunctions::computeFittness_2d_ARGB(pinCurr,pinOrig,orig->Length,width);
        }

        __int64 ComputeFittness_2d_2x2_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig, int width)
		{
			pin_ptr<System::Byte> pinCurr(&current[0]);
			pin_ptr<System::Byte> pinOrig(&orig[0]);

            
            return  FastFunctions::computeFittness_2d_2x2_ARGB(pinCurr,pinOrig,orig->Length,width);
        }

		__int64 ComputeFittnessSquareSSE_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr = &current[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

			
			

            result = FastFunctions::computeFittnessSumSquareASM_ARGB(pinCurr,pinOrig,orig->Length);
            
			return result;
		}

        __int64 ComputeFittnessSquare_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr = &current[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

			
			

            result = FastFunctions::computeFittnessSumSquare_ARGB(pinCurr,pinOrig,orig->Length);
            
			return result;
		}

        __int64 ComputeFittnessSquareLine_ARGB(array<System::Byte>^ line, array<System::Byte>^ orig, int origStartIndex)
		{
			__int64 result = 0;

            pin_ptr<System::Byte> pinLine = &line[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

            result = FastFunctions::computeFittnessSumSquare_ARGB(pinLine,pinOrig+origStartIndex,line->Length);
            
			return result;
		}

        __int64 ComputeFittnessSquareLineSSE_ARGB(array<System::Byte>^ line, array<System::Byte>^ orig, int origStartIndex)
		{
			__int64 result = 0;

            pin_ptr<System::Byte> pinLine = &line[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

            result = FastFunctions::computeFittnessSumSquareASM_ARGB(pinLine,pinOrig+origStartIndex,line->Length);
            
			return result;
		}

        
        __int64 ComputeFittnessABSSSE_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr = &current[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

            result = FastFunctions::computeFittnessSumABSASM_ARGB(pinCurr,pinOrig,orig->Length);
            
			return result;
		}

         __int64 ComputeFittnessABSSSE(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr = &current[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

			result = FastFunctions::computeFittnessSumABSASM(pinCurr,pinOrig,orig->Length);
            
			return result;
		}

        __int64 ComputeFittnessABS_ARGB(array<System::Byte>^ current, array<System::Byte>^ orig)
		{
			__int64 result = 0;

			pin_ptr<System::Byte> pinCurr = &current[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

			
			

            result = FastFunctions::computeFittnessSumABS_ARGB(pinCurr,pinOrig,orig->Length);
            
			return result;
		}

        __int64 ComputeFittnessABSLine_ARGB(array<System::Byte>^ line, array<System::Byte>^ orig, int origStartIndex)
		{
			__int64 result = 0;

            pin_ptr<System::Byte> pinLine = &line[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

            result = FastFunctions::computeFittnessSumABS_ARGB(pinLine,pinOrig+origStartIndex,line->Length);
            
			return result;
		}

        __int64 ComputeFittnessABSLineSSE_ARGB(array<System::Byte>^ line, array<System::Byte>^ orig, int origStartIndex)
		{
			__int64 result = 0;

            pin_ptr<System::Byte> pinLine = &line[0];
			pin_ptr<System::Byte> pinOrig = &orig[0];

            result = FastFunctions::computeFittnessSumABSASM_ARGB(pinLine,pinOrig+origStartIndex,line->Length);
            
			return result;
		}
	};
}
