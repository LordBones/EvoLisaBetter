#pragma once

class  AlphaBlending
{
private :


public:

    AlphaBlending(void){}
    ~AlphaBlending(void){}

    static  void NewFastRowApplyColorSSE(unsigned char * canvas, int countPixel, int color );

    static  void NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color,int alpha256);
    static  void NewFastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int color);

    static  void NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color, int alpha256);
    static  void NewFastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int color);

    static void FastChanelRowApplyColor(unsigned char * canvas, int count, unsigned char color, int alpha256);
    static void FastChanelRowApplyColor8SSE(unsigned char * canvas, int count, unsigned char color, int alpha256);

   
    static  void FastRowApplyColor(unsigned char * canvas, int countPixel, int r , int g, int b, int alpha);
    static  void FastRowApplyColorSSE64(unsigned char * canvas, int countPixel, int r , int g, int b, int alpha);
    static  void FastRowApplyColorSSE128(unsigned char * canvas, int countPixel, int r , int g, int b, int alpha);

    static  void FastRowsApplyColor(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColorSSE64(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColorSSE128(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);
    static  void FastRowsApplyColorSSE128_test(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int rangeStartY, int r, int g, int b, int a);

};
