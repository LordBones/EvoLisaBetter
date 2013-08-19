#pragma once
class  FastFunctions
{
private :
	static int ApplyColor(int colorChanel, int axrem, int rem);

public:

	FastFunctions(void){}
	~FastFunctions(void){}

	static  void FastRowApplyColor(unsigned char * canvas, int len, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem);

    static __int64 computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length);
    static __int64 computeFittnessSumSquare(unsigned char * curr, unsigned char * orig, int length);
		
};
