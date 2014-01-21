#pragma once

class  FastFunctions
{
private :
	static int ApplyColor(int colorChanel, int axrem, int rem);

public:

	FastFunctions(void){}
	~FastFunctions(void){}

    static  void NewFastRowApplyColorSSE(unsigned char * canvas, int countPixel, int color , int alpha);
    static  void NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color , int alpha);
    static  void NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color , int alpha);
    static  void RenderRectangle(unsigned char * canvas,int canvasWidth, int x,int y, int width, int height, int color , int alpha);
    static  void RenderTriangleByRanges(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int startY, int color, int alpha);
    static  void RenderTriangle(unsigned char * canvas, int canvasWidth, int canvasHeight, 
        short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color, int alpha);
    static  void RenderTrianglePokus(unsigned char * canvas, int canvasWidth, int canvasHeight, 
        short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color, int alpha);
     static  void RenderTriangleNew(unsigned char * canvas, int canvasWidth, int canvasHeight, 
        short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color, int alpha);


    static  void RenderOneRow(int * listRowsForApply, int countRows, unsigned char * canvas);

	static  void FastRowApplyColor(unsigned char * canvas, int countPixel, int r , int g, int b, int alpha);
    static  void FastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int r , int g, int b, int alpha);
    static  void FastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int r , int g, int b, int alpha);

	static  void FastRowsApplyColor(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColorSSE64(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColorSSE128(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColorSSE128_test(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);


	static void  ClearFieldByColor(unsigned char * curr, int length, int color);

    static __int64 computeFittnessTile(unsigned char * curr, unsigned char * orig, int length, int widthPixel);
    static __int64 computeFittness_2d(unsigned char * curr, unsigned char * orig, int length, int width);
    static __int64 computeFittness_2d_2x2(unsigned char * curr, unsigned char * orig, int length, int width);

    static __int64 computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length);
    static __int64 computeFittnessSumSquare(unsigned char * curr, unsigned char * orig, int length);
    static unsigned __int64 computeFittnessSumSquareASM( unsigned char* p1, unsigned char* p2, int count );

		
};
