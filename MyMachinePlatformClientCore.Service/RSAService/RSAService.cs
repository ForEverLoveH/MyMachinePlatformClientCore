using System.Security.Cryptography;
using System.Text;

namespace MyMachinePlatformClientCore.Service ;
/// <summary>
/// RSA加密解密服务
/// </summary>
public class RSAService
{
    
    private  string privateKey;

    private string publicKey;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="privateKey"></param>
    /// <param name="publicKey"></param>
    public RSAService(string privateKey, string publicKey)
    {
        this.privateKey = privateKey;
        this.publicKey = publicKey;

    }
    /// <summary>
    /// 使用公钥进行加密
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public  string Encrypt(string plainText)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(publicKey);
        byte[] data = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedData = rsa.Encrypt(data, false);
        return Convert.ToBase64String(encryptedData);


    }
    /// <summary>
    /// 使用私钥进行解密
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public string Decrypt(string plainText)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(privateKey);
        byte[] data = Convert.FromBase64String(plainText);
        byte[] decryptedData = rsa.Decrypt(data, false);
        return Encoding.UTF8.GetString(decryptedData);
    }
}