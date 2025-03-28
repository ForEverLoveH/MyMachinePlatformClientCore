namespace MyMachinePlatformClientCore.Service.OMRonService;

public enum MemAreaTCPKV
{
    R,
    B,
    MR,
    LR,
    CR,
    VB,
    DM,
    EM,
    FM,
    ZF,
    W,
    TM,
    Z,
    T,
    TC,
    TS,
    C,
    CC,
    CS,
    AT,
    CM,
    VM,
    Max
}
public enum MainCmd
{
    MutilWRITE = 0x1401,

    MutilREAD = 0x0401,

    RANDWRITE = 0x1402,
    RANDWordREAD = 0x0403
}

public enum SubCmd
{
    //字
    WordADDR = 0x0002,

    //位
    BITADDR = 0x0003
}