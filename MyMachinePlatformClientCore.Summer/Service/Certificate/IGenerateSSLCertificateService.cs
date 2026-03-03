using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Service 
{
    public interface IGenerateSSLCertificateService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="certPath"></param>
        /// <param name="pfxPath"></param>
        /// <param name="friendlyName"></param>
        /// <param name="keyStrength"></param>
        void CreateGenerateCertificate(string certPath, string pfxPath, string friendlyName, int keyStrength = 2048);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pfxPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        Tuple<string, string> LoadingPfxCertificateKey(string pfxPath, string password);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pfxPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Tuple<DateTime, DateTime> LoadingPfxCertificateTime(string pfxPath, string password);

        void FixPfxCertificatePasswrod(string pfxPath, string newPassword);
    }
}
