namespace MyMachinePlatformClientCore.IService.IFTPService;

public interface IFTPClientService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <param name="sizeThreshold"></param>
    /// <returns></returns>
    Task<bool> UploadFile(string localFilePath, string remoteFilePath, long sizeThreshold = 10 * 1024 * 1024);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="remoteFilePath"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    Task<bool> DownloadFile(string remoteFilePath, string localFilePath);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="remoteFilePath"></param>
    /// <returns></returns>
    Task<bool> DeleteFile(string remoteFilePath);
}