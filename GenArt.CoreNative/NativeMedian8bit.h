
//#pragma managed(push, off)

//#pragma unmanage
class  NativeMedian8Bit
{
private:
    static const int CONST_MedianTableSize = 256;
    long  _medianTable[CONST_MedianTableSize];

    unsigned char ComputeMedian();
    __int64 ComputeSumStdDev();

public:
    NativeMedian8Bit(void);
    ~NativeMedian8Bit(void);

    void _fastcall InsertData(unsigned char data){
    
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

  
 
//#pragma managed  

//#pragma managed(pop)
