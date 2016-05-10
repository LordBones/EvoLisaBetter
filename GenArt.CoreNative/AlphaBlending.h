#pragma once

class  AlphaBlending
{
private :


public:

    AlphaBlending(void){}
    ~AlphaBlending(void){}

	
	static  void NewFastRowApplyColor(unsigned char * canvas, int countPixel, int color);


    static  void NewFastRowApplyColorSSE(unsigned char * canvas, int countPixel, int color );

    static  void NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color,int alpha256);
    static  void NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color);
	static  void NewFastRowApplyColorSSE64Empty(unsigned char * canvas, int countPixel, int color);



    static  void NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color, int alpha256);
    static  void NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color);

	static  void NewFastRowApplyColorSSE256(unsigned char * canvas, int countPixel, int color, int alpha256);
	static  void NewFastRowApplyColorSSE256(unsigned char * canvas, int countPixel, int color);

   
};
