using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace MyMachinePlatformClientCore.Common.GenerateCertificateService;

/// <summary>
/// TSL证书生成服务
/// 该服务用于生成TSL证书，用于https请求
/// </summary>
public class MyGenerateTslCertificateService
{
    /// <summary>
    /// 证书开始时间
    /// </summary>
    private DateTime _startTime;
    /// <summary>
    /// 证书结束时间
    /// </summary>
    private DateTime _endTime;
    /// <summary>
    /// 证书密码
    /// </summary>
    private string _password;
    /// <summary>
    /// 用户名
    /// </summary>
    private string _userName;
    /// <summary>
    /// 证书别名
    /// </summary>
    private string _friendlyName;

    public MyGenerateTslCertificateService(DateTime startTime, DateTime endTime, string password, string userName, string friendlyName)
    {
        _startTime = startTime;
        _endTime = endTime;
        _password = password;
        _userName = userName;
        _friendlyName = friendlyName;
    }

    /// <summary>
    /// 生成TLS证书
    /// </summary>
    /// <param name="certPath">公钥证书保存路径</param>
    /// <param name="pfxPath">包含公私钥的PFX文件保存路径</param>
    /// <param name="keyStrength">密钥长度，默认2048</param>
    public void GenerateTlsCertificate(string certPath, string pfxPath, int keyStrength = 2048)
    {
        // 创建安全随机数生成器
        SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());
        var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);

        // 生成 RSA 密钥对
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

        // 配置证书信息
        X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

        // 设置证书序列号
        BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        certificateGenerator.SetSerialNumber(serialNumber);

        // 设置证书有效期
        certificateGenerator.SetNotBefore(_startTime);
        certificateGenerator.SetNotAfter(_endTime);

        // 设置证书颁发者和使用者信息
        X509Name subjectDN = new X509Name($"CN={_userName}");
        X509Name issuerDN = subjectDN;
        certificateGenerator.SetIssuerDN(issuerDN);
        certificateGenerator.SetSubjectDN(subjectDN);

        // 设置公钥
        certificateGenerator.SetPublicKey(subjectKeyPair.Public);

        // 添加扩展信息
        certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
        certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
        certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));

        // 创建签名工厂
        ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", subjectKeyPair.Private, random);

        // 生成证书
        Org.BouncyCastle.X509.X509Certificate bouncyCert = certificateGenerator.Generate(signatureFactory);

        // 转换为 .NET 证书
        X509Certificate2 cert = new X509Certificate2(DotNetUtilities.ToX509Certificate(bouncyCert));
        cert.FriendlyName = _friendlyName;

        // 保存公钥证书（.cer）
        byte[] cerBytes = cert.Export(X509ContentType.Cert);
        File.WriteAllBytes(certPath, cerBytes);

        // 创建 PKCS#12 存储
        Pkcs12Store store = new Pkcs12Store();
        // 将 X509Certificate2 转换为 Org.BouncyCastle.X509.X509Certificate
        Org.BouncyCastle.X509.X509Certificate bcCert = DotNetUtilities.FromX509Certificate(cert);
        X509CertificateEntry certEntry = new X509CertificateEntry(bcCert);
        store.SetCertificateEntry(_friendlyName, certEntry);
        store.SetKeyEntry(_friendlyName, new AsymmetricKeyEntry(subjectKeyPair.Private), new[] { certEntry });

        // 保存包含公私钥的 PFX 文件
        using (FileStream fs = File.Create(pfxPath))
        {
            store.Save(fs, _password.ToCharArray(), random);
        }
    }
    /// <summary>
    /// 加载证书
    /// </summary>
    /// <param name="pfxPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>

    public X509Certificate2 LoadingTslCertificate( string pfxPath, string password)
    {
        return new X509Certificate2(pfxPath, password,X509KeyStorageFlags.Exportable);
    }
    /// <summary>
    /// 修改证书密码
    /// </summary>
    /// <param name="pfxPath"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>

    public X509Certificate2 FixPfxCertificatePasswrod(string pfxPath, string newPassword)
    {
        X509Certificate2 oldCert = new X509Certificate2(pfxPath, _password, X509KeyStorageFlags.Exportable);
        byte[] pfxData = oldCert.Export(X509ContentType.Pfx, _password);
        X509Certificate2 newCert = new X509Certificate2(pfxData, newPassword, X509KeyStorageFlags.Exportable);
        return newCert;
    }
}
