#pragma once

class  FastFunctions
{
private :
	static int ApplyColor(int colorChanel, int axrem, int rem);

public:

	FastFunctions(void){}
	~FastFunctions(void){}

	static  void FastRowApplyColor(unsigned char * canvas, int len, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem);
	static  void FastRowsApplyColor(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColor2(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);

	static void  ClearFieldByColor(unsigned char * curr, int length, int color);

    static __int64 computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length);
    static __int64 computeFittnessSumSquare(unsigned char * curr, unsigned char * orig, int length);
    static unsigned __int64 computeFittnessSumSquareASM( unsigned char* p1, unsigned char* p2, int count );

		
};
