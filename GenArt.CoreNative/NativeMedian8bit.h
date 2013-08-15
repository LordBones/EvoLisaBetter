
//#pragma managed(push, off)

#pragma unmanaged
public class NativeMedian8Bit
{
private:
    static const int CONST_MedianTableSize = 256;
    long  _medianTable[CONST_MedianTableSize];

    unsigned char ComputeMedian();
    __int64 ComputeSumStdDev();

public:
    NativeMedian8Bit(void);
    ~NativeMedian8Bit(void);

    void InsertData(unsigned char data){
    
        _medianTable[data]++;
    }

    __int64 TotalCount()
    {
        __int64 sum = 0;
        for (int index = 0; index < CONST_MedianTableSize; index++) sum += _medianTable[index];
        return sum;
    }

    __int64 ValueSum()
    {
        __int64 sum = 0;
        for (int index = 0; index < CONST_MedianTableSize; index++) sum += _medianTable[index]*index;
        return sum;
    }

    unsigned int Median(){
        return ComputeMedian();
        }

    __int64 SumStdDev(){
        return ComputeSumStdDev();
        }
   
     double StdDev(){
         return ComputeMedian()/TotalCount();
        }
   
};

  
public class FastFunctions
{
private :
	static int ApplyColor(int colorChanel, int axrem, int rem);

public:

	FastFunctions(void){}
	~FastFunctions(void){}

	static  void FastRowApplyColor(unsigned char * canvas, int from, int to, int colorABRrem, int colorAGRrem, int colorARRrem, int colorRem);

    static __int64 computeFittnessWithStdDev(unsigned char * curr, unsigned char * orig, int length);
		
};
#pragma managed  

//#pragma managed(pop)
