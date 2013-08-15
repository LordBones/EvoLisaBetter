#pragma once
class __declspec(dllexport) FastFunctions2
{
public:
    FastFunctions2(void);
    ~FastFunctions2(void);

    static  void FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem);
};

