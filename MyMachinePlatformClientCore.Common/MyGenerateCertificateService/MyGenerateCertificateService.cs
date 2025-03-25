
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Math;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.Encoders;


namespace MyMachinePlatformClientCore.Common.GenerateCertificateService;
/// <summary>
/// 证书服务类
/// </summary>
public class MyGenerateCertificateService
{
    /// <summary>
    /// 证书生效时间
    /// </summary>
    private DateTime takeEffect = DateTime.Now;
    /// <summary>
    /// 证书有效时间
    /// </summary>
    private readonly DateTime loseEffect = DateTime.Now.AddYears(1);
    /// <summary>
    /// 证书密码
    /// </summary>
    private   string password = "ABDC123456";
    /// <summary>
    /// 签名算法
    /// </summary>
    private  const string signatureAlgorithm = "SHA256WITHRSA";   
    /// <summary>
    /// 别名
    /// </summary>
    private   string friendlyName ;
    /// <summary>
    /// 
    /// </summary>
    private  X509Name issuer;
    private  X509Name subject;

    public MyGenerateCertificateService(DateTime loseEffect, string password, string friendlyName)
    {
        this.loseEffect = loseEffect;
        this.password = password;
        this.friendlyName = friendlyName;
        issuer = CreateIssure();
        subject = CreateSubject();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private X509Name? CreateSubject()
    {
        ArrayList list = new ArrayList
        {
            X509Name.C,
            X509Name.O,
            X509Name.CN,
        };
        Hashtable hashtable = new Hashtable()
        {
            [X509Name.C] = "CN",// 证书的语言    
            [X509Name.O] = "ICH",//设置证书的办法者
            [X509Name.CN] = "*.dpps.fun"
        };
        return new X509Name(list, hashtable);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private X509Name? CreateIssure()
    {
        ArrayList list = new ArrayList()
        {
            X509Name.C,
            X509Name.O,
            X509Name.OU,
            X509Name.L,
            X509Name.ST,
            X509Name.E,

        };
        Hashtable hashtable = new Hashtable()
        {
            [X509Name.C] = "CN",// 证书的语言
            [X509Name.O] = "dpps.fun",//设置证书的办法者
            [X509Name.OU] = "dpps.fun Fulu RSA CA 2020",
            [X509Name.L] = "dpps",
            [X509Name.ST] = "dpps",
            [X509Name.E] = "13294062492@163.com",
        };
        return new X509Name( list, hashtable);
          
    }


    /// <summary>
    /// 创建证书
    /// </summary>
    /// <param name="certPath">只有公钥</param>
    /// <param name="pfxPath">含公私钥</param>
    /// <param name="signatureAlgorithm">设置将用于签署此证书的签名算法</param>
    /// <param name="friendlyName">设置证书友好名称（可选）</param>
    /// <param name="keyStrength">密钥长度</param>
    public void CreateGenerateCertificate(string certPath, string pfxPath, string friendlyName, int keyStrength = 2048)
    {
        SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());
        var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator(); //RSA密钥对生成器
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
        ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, subjectKeyPair.Private, random);
        X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
        var spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(subjectKeyPair.Public);
        /// 允许作为一个CA证书（可以颁发下级证书或进行签名）
        certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true));
        //使用者密钥标识符
        certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifier(spki));
        certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, true, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));
        //证书序列号
        BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        certificateGenerator.SetSerialNumber(serialNumber);
        certificateGenerator.SetIssuerDN(issuer);
        certificateGenerator.SetSubjectDN(subject);
        certificateGenerator.SetNotBefore(takeEffect);
        certificateGenerator.SetNotAfter(loseEffect);
        certificateGenerator.SetPublicKey(subjectKeyPair.Public);
        Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(signatureFactory);
        //生成cer证书，公钥证
        var cer = new X509Certificate2(DotNetUtilities.ToX509Certificate(certificate))
        {
            FriendlyName = friendlyName,
        };
        var buffer = cer.Export(X509ContentType.Cert);
        using (FileStream stream = new FileStream(certPath, FileMode.Create))
        {
            stream.Write(buffer, 0, buffer.Length);
        }
        //另一种代码生成p12证书的方式（要求使用.net standard 2.1）
        //certificate2 = certificate2.CopyWithPrivateKey(DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)keyPair.Private));
        //var bytes2 = certificate2.Export(X509ContentType.Pfx, password);
        //using (var fs = new FileStream(pfxPath, FileMode.Create))
        //{             //    fs.Write(bytes2, 0, bytes2.Length);             //}
        // 生成pfx证书，公私钥证
        var certEntry = new X509CertificateEntry(certificate);
        var store = new Pkcs12StoreBuilder().Build();
        store.SetCertificateEntry(friendlyName, certEntry);   //设置证书
        var chain = new X509CertificateEntry[1];
        chain[0] = certEntry;
        store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(subjectKeyPair.Private), chain);   //设置私钥
        using (var fs = File.Create(pfxPath))
        {
            store.Save(fs, password.ToCharArray(), random); //保存
        };
    }

    /// <summary>
    /// 加载pfx证书的key
    /// </summary>
    /// <param name="pfxPath"></param>
    /// <param name="password"></param>
    /// <returns>返回私钥 公钥</returns>
    public Tuple<string, string> LoadingPfxCertificateKey(string pfxPath, string password)
    {
        X509Certificate2 pfx = new X509Certificate2(pfxPath, password, X509KeyStorageFlags.Exportable);
        var keyPair = DotNetUtilities.GetKeyPair(pfx.PrivateKey);
        //var keyPair = pfx.GetRSAPrivateKey();
        var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
        var privateKey = Base64.ToBase64String(privateKeyInfo.ParsePrivateKey().GetEncoded());
        var publicKey = Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded());
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        return new Tuple<string, string>(publicKey, privateKey);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pfxPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public Tuple<DateTime, DateTime> LoadingPfxCertificateTime(string pfxPath, string password)
    {
        X509Certificate2 pfx = new X509Certificate2(pfxPath, password, X509KeyStorageFlags.Exportable);
        DateTime startDate = pfx.NotBefore;
        // 获取证书的开始时间
        DateTime endDate = pfx.NotAfter;
        return new Tuple<DateTime, DateTime>(startDate, endDate);

    }
    /// <summary>
    /// 修改证书密码
    /// </summary>
    /// <param name="pfxPath"></param>
    /// <param name="password"></param>
    public void FixPfxCertificatePasswrod(string pfxPath, string newPassword)
    {
        // 加载现有的PFX证书
        X509Certificate2 pfx = new X509Certificate2(pfxPath, this.password, X509KeyStorageFlags.Exportable);

        // 使用新密码导出PFX证书
        byte[] newPfxBytes = pfx.Export(X509ContentType.Pfx, newPassword);

        // 将新的PFX证书保存到原文件路径
        File.WriteAllBytes(pfxPath, newPfxBytes);

    }
    
}