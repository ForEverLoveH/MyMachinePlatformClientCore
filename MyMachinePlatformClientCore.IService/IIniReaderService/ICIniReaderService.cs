namespace MyMachinePlatformClientCore.IService.IIniReaderService;

public interface ICIniReaderService
{
    int IniReadInt(string lpszKey, int nDefault);

    int IniReadInt(string lpszSection, string lpszKey, int nDefault);

    int IniReadInt(string lpszSection, string lpszKey, int nDefault, string lpszIniFile);
    string IniReadString(string lpszKey, string lpszDefault);

    string IniReadString(string lpszSection, string lpszKey, string lpszDefault);
    string IniReadString(string lpszSection, string lpszKey, string lpszDefault, string lpszIniFile);
    bool IniReadBoolean(string lpszKey, bool bDefault);
    bool IniReadBoolean(string lpszSection, string lpszKey, bool bDefault);
    bool IniReadBoolean(string lpszSection, string lpszKey, bool bDefault, string lpszIniFile);
    double IniReadDouble(string lpszKey, double dDefault);
    double IniReadDouble(string lpszSection, string lpszKey, double dDefault);
    double IniReadDouble(string lpszSection, string lpszKey, double dDefault, string lpszIniFile);
    bool IniWrite(string lpszKey, bool bValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, bool bValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, bool bValueToWrite, string lpszIniFile);
    bool IniWrite(string lpszKey, int nValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, int nValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, int nValueToWrite, string lpszIniFile);
    bool IniWrite(string lpszKey, double nValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, double nValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, double nValueToWrite, string lpszIniFile);
    bool IniWrite(string lpszKey, string nValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, string nValueToWrite);
    bool IniWrite(string lpszSection, string lpszKey, string nValueToWrite, string lpszIniFile);
}