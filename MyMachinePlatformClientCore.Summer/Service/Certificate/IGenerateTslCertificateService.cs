using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Service 
{
    public interface IGenerateTslCertificateService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="certPath"></param>
        /// <param name="pfxPath"></param>
        /// <param name="keyStrength"></param>
        void GenerateTlsCertificate(string certPath, string pfxPath, int keyStrength = 2048);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pfxPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        X509Certificate2 LoadingTslCertificate(string pfxPath, string password);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pfxPath"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>

        X509Certificate2 FixPfxCertificatePasswrod(string pfxPath, string newPassword);
    }
}
