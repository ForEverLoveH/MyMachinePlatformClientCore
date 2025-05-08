using System.Net;
using MyMachinePlatformClientCore.IService.IFTPService;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.FTPService;
/// <summary>
/// ftp客户端服务
/// </summary>
public class FTPClientService:IFTPClientService
{
    private readonly string _ftpServerIp;
    /// <summary>
    /// 
    /// </summary>
    private readonly int _ftpServerPort;
    /// <summary>
    /// 
    /// </summary>
    private readonly string _ftpUserName;
    /// <summary>
    /// 
    /// </summary>
    private readonly string _ftpPassword;


    private Action<LogMessage > _LogDataCallBack;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ftpServerIp"></param>
    /// <param name="ftpServerPort"></param>
    /// <param name="ftpUserName"></param>
    /// <param name="ftpPassword"></param>
    public FTPClientService(string ftpServerIp, int ftpServerPort, string ftpUserName, string ftpPassword, Action<LogMessage > logDataCallBack= null)
    {
        this._ftpServerIp = ftpServerIp;
        this._ftpServerPort = ftpServerPort;
        this._ftpUserName = ftpUserName;
        this._ftpPassword = ftpPassword;
        this. _LogDataCallBack = logDataCallBack;
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <param name="sizeThreshold">文件阈值 10MB</param>
    /// <returns></returns>
    public async Task<bool> UploadFile(string localFilePath, string remoteFilePath,long sizeThreshold = 10 * 1024 * 1024)
    {
        try
        {
            // 获取文件信息
            FileInfo fileInfo = new FileInfo(localFilePath);
            // 设置文件大小阈值为 10MB
            if (fileInfo.Length > sizeThreshold)
                // 若文件大小超过阈值，采用分段上传
                return await UploadFileInChunks(localFilePath, remoteFilePath);
            
            else
            {
                // 若文件大小未超过阈值，正常上传
                return await UploadSingleFile(localFilePath, remoteFilePath);
            }
        }
        catch (Exception ex)
        {
            _LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Error, $"上传文件失败: {ex.Message}"));
            
            return false; 
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <returns></returns>
    private async Task<bool> UploadSingleFile(string localFilePath, string remoteFilePath)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_ftpServerIp}/{remoteFilePath}");
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);

        using (FileStream fileStream = File.OpenRead(localFilePath))
        using (Stream requestStream = await request.GetRequestStreamAsync())
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await requestStream.WriteAsync(buffer, 0, bytesRead);
            }
        }
        using (FtpWebResponse response = (FtpWebResponse) await request.GetResponseAsync())
        {
            var status = response.StatusCode;
            if (status == FtpStatusCode.CommandOK)
            {
                _LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,
                    $"上传文件成功: {localFilePath} -> {remoteFilePath}"));
                 
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <returns></returns>
    private async Task<bool> UploadFileInChunks(string localFilePath, string remoteFilePath)
    {
        const int chunkSize = 1024 * 1024; // 1MB 块大小
        FileInfo fileInfo = new FileInfo(localFilePath);
        long fileLength = fileInfo.Length;
        long bytesUploaded = 0;

        using (FileStream fileStream = File.OpenRead(localFilePath))
        {
            while (bytesUploaded < fileLength)
            {
                long bytesToRead = Math.Min(chunkSize, fileLength - bytesUploaded);
                byte[] buffer = new byte[bytesToRead];
                int bytesRead = await fileStream.ReadAsync(buffer, 0, (int)bytesToRead);
                // 为每个块创建新的 FTP 请求
                string chunkRemoteFilePath = $"{remoteFilePath}.part{bytesUploaded / chunkSize}";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_ftpServerIp}/{chunkRemoteFilePath}");
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead);
                }
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    var status = response.StatusCode;
                    if (status != FtpStatusCode.CommandOK)
                    {
                        Console.WriteLine($"上传文件块失败: {chunkRemoteFilePath}");
                    }
                }
                bytesUploaded += bytesRead;
            }
        }
        _LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,
            $"分段上传文件成功: {localFilePath} -> {remoteFilePath}"));
        // 这里可以添加合并文件块的逻辑，需要服务器端支持
        
        return true;
    }

    /// <summary>
    /// 从 FTP 服务器下载文件
    /// </summary>
    /// <param name="remoteFilePath">远程文件路径</param>
    /// <param name="localFilePath">本地文件路径</param>
    public async Task<bool> DownloadFile(string remoteFilePath, string localFilePath)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_ftpServerIp}/{remoteFilePath}");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (FileStream fileStream = File.Create(localFilePath))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                }

                if (response.StatusCode == FtpStatusCode.CommandOK)
                {
                    _LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,$"下载文件成功: {remoteFilePath} -> {localFilePath}"
                         ));
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }


        }
        catch (Exception ex)
        {
            _LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Error, $"下载文件失败: {ex.Message}"));
            
            return false; 
        }
    }
    
    /// <summary>
    /// 删除 FTP 服务器上的文件
    /// </summary>
    /// <param name="remoteFilePath">远程文件路径</param>
    public async Task<bool> DeleteFile(string remoteFilePath)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_ftpServerIp}/{remoteFilePath}");
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);

            using (FtpWebResponse response =  (FtpWebResponse) await request.GetResponseAsync())
            {
                var status = response.StatusCode;
                if (status == FtpStatusCode.CommandOK)
                {
                    _LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,$"删除文件成功"
                    ));
                    
                    return true;
                }   
                return false; 
               
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除文件失败: {ex.Message}");
            return false;
        }
    }
}