using System.Runtime.InteropServices;
using MyMachinePlatformClientCore.IService.IIniReaderService;

namespace MyMachinePlatformClientCore.Service.IniReaderService;
/// <summary>
/// 
/// </summary>
public class CIniReaderService:ICIniReaderService
{
    
 
        private const int MAX_ENTRY = 32768;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int lpDefault, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpString, System.Text.StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        public string Filename { get; set; }

        public string Section { get; set; }

        /// <summary>
        /// 读取ini文件内指定位置的整型数值。
        /// </summary>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nDefault"> nDefault 读取失败时的赋值. </param>
        /// <returns></returns>
        public int IniReadInt(string lpszKey, int nDefault)
        {
            int readInt = GetPrivateProfileInt(Section, lpszKey, nDefault, Filename);
            return readInt;
        }

        /// <summary>
        ///读取ini文件内指定位置的整型数值。
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nDefault">nDefault 读取失败时的赋值</param>
        /// <returns>返回读取值</returns>
        public int IniReadInt(string lpszSection, string lpszKey, int nDefault)
        {
            int readInt = GetPrivateProfileInt(lpszSection, lpszKey, nDefault, Filename);
            return readInt;
        }

        /// <summary>
        ///读取ini文件内指定位置的整型数值。
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nDefault">nDefault 读取失败时的赋值</param>
        /// <param name="lpszIniFile"> lpszIniFile 要读取的文件名及路径</param>
        /// <returns></returns>
        public int IniReadInt(string lpszSection, string lpszKey, int nDefault, string lpszIniFile)
        {
            int readInt = GetPrivateProfileInt(lpszSection, lpszKey, nDefault, lpszIniFile);
            return readInt;
        }

        /// <summary>
        /// 读取ini文件内指定位置的字符
        /// </summary>
        /// <param name="lpszKey"> lpszKey 项下的键值名</param>
        /// <param name="lpszDefault"> lpszDefault 读取失败时的赋值</param>
        /// <returns></returns>
        public string IniReadString(string lpszKey, string lpszDefault)
        {
            string str = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_ENTRY);
            int len = GetPrivateProfileString(Section, lpszKey, lpszDefault, sb, MAX_ENTRY, Filename);
            if (len <= 0)
                str = lpszDefault;
            else
                str = sb.ToString();
            return str;
        }

        /// <summary>
        /// 读取ini文件内指定位置的字符
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="lpszDefault">lpszDefault 读取失败时的赋值</param>
        /// <returns></returns>
        public string IniReadString(string lpszSection, string lpszKey, string lpszDefault)
        {
            string str = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_ENTRY);
            int len = GetPrivateProfileString(lpszSection, lpszKey, lpszDefault, sb, MAX_ENTRY, Filename);
            if (len <= 0)
                str = lpszDefault;
            else
                str = sb.ToString();
            return str;
        }

        /// <summary>
        ///读取ini文件内指定位置的字符
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="lpszDefault">lpszDefault 读取失败时的赋值</param>
        /// <param name="lpszIniFile">lpszIniFile 要读取的文件名及路径</param>
        /// <returns></returns>
        public string IniReadString(string lpszSection, string lpszKey, string lpszDefault, string lpszIniFile)
        {
            string str = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(MAX_ENTRY);
            int len = GetPrivateProfileString(lpszSection, lpszKey, lpszDefault, sb, MAX_ENTRY, lpszIniFile);
            if (len <= 0)
                str = lpszDefault;
            else
                str = sb.ToString();
            return str;
        }

        /// <summary>
        /// 读取ini文件内指定位置的布尔数值
        /// </summary>
        /// <param name="lpszKey"> lpszKey 项下的键值名</param>
        /// <param name="bDefault">bDefault 读取失败时的赋值</param>
        /// <returns></returns>
        public bool IniReadBoolean(string lpszKey, bool bDefault)
        {
            string str = IniReadString(Section, lpszKey, bDefault.ToString(), Filename);
            string strupper = "";
            strupper = str.ToUpper();
            if (strupper == "YES" || strupper == "TRUE" || strupper == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
            //   return bool.Parse(IniReadString(Section, lpszKey, bDefault.ToString(), Filename));
        }

        /// <summary>
        ///读取ini文件内指定位置的布尔数值
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="bDefault">bDefault 读取失败时的赋值</param>
        /// <param name="lpszIniFile">lpszIniFile 要读取的文件名及路径</param>
        /// <returns></returns>
        public bool IniReadBoolean(string lpszSection, string lpszKey, bool bDefault)
        {
            string str = IniReadString(lpszSection, lpszKey, bDefault.ToString(), Filename);
            string strupper = "";
            strupper = str.ToUpper();
            if (strupper == "YES" || strupper == "TRUE" || strupper == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
            // return bool.Parse(IniReadString(lpszSection, lpszKey, bDefault.ToString(), Filename));
        }

        /// <summary>
        ///读取ini文件内指定位置的布尔数值
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="bDefault">bDefault 读取失败时的赋值</param>
        /// <param name="lpszIniFile">lpszIniFile 要读取的文件名及路径</param>
        /// <returns></returns>
        public bool IniReadBoolean(string lpszSection, string lpszKey, bool bDefault, string lpszIniFile)
        {
            string str = IniReadString(lpszSection, lpszKey, bDefault.ToString(), lpszIniFile);
            string strupper = "";
            strupper = str.ToUpper();
            if (strupper == "YES" || strupper == "TRUE" || strupper == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
            //return bool.Parse(IniReadString(lpszSection, lpszKey, bDefault.ToString(), lpszIniFile));
        }

        /// <summary>
        /// 读取ini文件内指定位置的双精度数值
        /// </summary>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="dDefault">bDefault 读取失败时的赋值</param>
        /// <returns></returns>
        public double IniReadDouble(string lpszKey, double dDefault)
        {
            string str = "";
            double dValue = 0.0;
            String strDefault = dDefault.ToString();
            str = IniReadString(Section, lpszKey, strDefault, Filename);
            dValue = double.Parse(str);
            return dValue;
        }

        /// <summary>
        ///读取ini文件内指定位置的双精度数值
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="dDefault">bDefault 读取失败时的赋值</param>
        /// <returns></returns>
        public double IniReadDouble(string lpszSection, string lpszKey, double dDefault)
        {
            string str = "";
            double dValue = 0.0;
            String strDefault = dDefault.ToString();
            str = IniReadString(lpszSection, lpszKey, strDefault, Filename);
            dValue = double.Parse(str);
            return dValue;
        }

        /// <summary>
        ///读取ini文件内指定位置的双精度数值
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="dDefault">bDefault 读取失败时的赋值</param>
        /// <param name="lpszIniFile">lpszIniFile 要读取的文件名及路径</param>
        /// <returns></returns>
        public double IniReadDouble(string lpszSection, string lpszKey, double dDefault, string lpszIniFile)
        {
            string str = "";
            double dValue = 0.0;
            String strDefault = dDefault.ToString();
            str = IniReadString(lpszSection, lpszKey, strDefault, lpszIniFile);
            dValue = double.Parse(str);
            return dValue;
        }

        /// <summary>
        ///  写入给定布尔赋值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="bValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszKey, bool bValueToWrite)
        {
            if (!File.Exists(Filename))
            {
               
                return false;
            }
            string str = "";
            if (bValueToWrite)
                str = "YES";
            else
                str = "NO";
            return IniWrite(Section, lpszKey, str, Filename);
        }

        /// <summary>
        /// 写入给定布尔赋值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="bValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, bool bValueToWrite)
        {
            if (!File.Exists(Filename))
            {
                
                return false;
            }
            string str = "";
            if (bValueToWrite)
                str = "YES";
            else
                str = "NO";
            return IniWrite(lpszSection, lpszKey, str, Filename);
        }

        /// <summary>
        /// 写入给定布尔赋值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="bValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <param name="lpszIniFile">lpszIniFile 要写入的文件名及路径</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, bool bValueToWrite, string lpszIniFile)
        {
            if (!File.Exists(lpszIniFile))
            {
                
                return false;
            }
            string str = "";
            if (bValueToWrite)
                str = "YES";
            else
                str = "NO";
            return IniWrite(lpszSection, lpszKey, str, lpszIniFile);
        }

        /// <summary>
        /// 写入整型数值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszKey, int nValueToWrite)
        {
            return IniWrite(Section, lpszKey, nValueToWrite, Filename);
        }

        /// <summary>
        ///写入整型数值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, int nValueToWrite)
        {
            return IniWrite(lpszSection, lpszKey, nValueToWrite, Filename);
        }

        /// <summary>
        ///写入整型数值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <param name="lpszIniFile">lpszIniFile 要写入的文件名及路径</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, int nValueToWrite, string lpszIniFile)
        {
            return IniWrite(lpszSection, lpszKey, nValueToWrite.ToString(), lpszIniFile);
        }

        /// <summary>
        /// 写入双精度数值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszKey, double nValueToWrite)
        {
            return IniWrite(Section, lpszKey, nValueToWrite, Filename);
        }

        /// <summary>
        /// 写入双精度数值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite">lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, double nValueToWrite)
        {
            return IniWrite(lpszSection, lpszKey, nValueToWrite, Filename);
        }

        /// <summary>
        /// 写入双精度数值到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <param name="lpszIniFile"> lpszIniFile 要写入的文件名及路径</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, double nValueToWrite, string lpszIniFile)
        {
            return IniWrite(lpszSection, lpszKey, nValueToWrite.ToString(), lpszIniFile);
        }

        /// <summary>
        /// 写入字符到ini文件内指定位置
        /// </summary>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszKey, string nValueToWrite)
        {
            return IniWrite(Section, lpszKey, nValueToWrite, Filename);
        }

        /// <summary>
        /// 写入字符到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, string nValueToWrite)
        {
            return IniWrite(lpszSection, lpszKey, nValueToWrite, Filename);
        }

        /// <summary>
        /// 写入字符到ini文件内指定位置
        /// </summary>
        /// <param name="lpszSection">lpszSection 文件项名，[XXX]</param>
        /// <param name="lpszKey">lpszKey 项下的键值名</param>
        /// <param name="nValueToWrite"> lpszStrToWrite 需要写入的字符</param>
        /// <param name="lpszIniFile"> lpszIniFile 要写入的文件名及路径</param>
        /// <returns></returns>
        public bool IniWrite(string lpszSection, string lpszKey, string nValueToWrite, string lpszIniFile)
        {
            return (WritePrivateProfileString(lpszSection, lpszKey, nValueToWrite, lpszIniFile) != 0);
        }
}