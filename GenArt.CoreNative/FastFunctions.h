#pragma once

class  FastFunctions
{
private :


public:

    FastFunctions(void){}
    ~FastFunctions(void){}

    static  void RenderRectangle(unsigned char * canvas,int const canvasWidth,const int x,const int y, const int width,const int height,const int color ,const int alpha);
    static  void RenderTriangleByRanges(unsigned char * canvas, int canvasWidth, short int * ranges, int rlen, int startY, int color, int alpha);
    static  void RenderTriangle(unsigned char * canvas, int canvasWidth, int canvasHeight, 
        short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color);
    static  void RenderTriangleNew(unsigned char * canvas, int canvasWidth, int canvasHeight, 
        short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color);
    static  void RenderTriangleNewOptimize(unsigned char * canvas, int canvasWidth, int canvasHeight, 
        short int px0,short int py0,short int px1,short int py1,short int px2,short int py2, int color);
	  
  

    static  void RenderOneRow(int * listRowsForApply, int countRows, unsigned char * canvas);
	
    static void  ClearFieldByColorInt(unsigned char * curr, int lengthPixel, int color);
    


    static __int64 computeFittnessTile_ARGB(unsigned char * curr, unsigned char * orig, int length, int widthPixel);
    static __int64 computeFittness_2d_ARGB(unsigned char * curr, unsigned char * orig, int length, int width);
    static __int64 computeFittness_2d_2x2_ARGB(unsigned char * curr, unsigned char * orig, int length, int width);

    static __int64 computeFittnessWithStdDev_ARGB(unsigned char * curr, unsigned char * orig, int length);
    static __int64 computeFittnessSumSquare_ARGB(unsigned char * curr, unsigned char * orig, int length);
    static __int64 computeFittnessSumSquareASM_ARGB( unsigned char* p1, unsigned char* p2, int count );
    static __int64 computeFittnessSumABS_ARGB(unsigned char * curr, unsigned char * orig, int length);
    static __int64 computeFittnessSumABSASM_ARGB( unsigned char* p1, unsigned char* p2, int count );
    static __int64 computeFittnessSumABSASM( unsigned char* p1, unsigned char* p2, int count );
    


};
