using HslCommunication;
using HslCommunication.Profinet.Omron;
 

namespace MyMachinePlatformClientCore.Service.OMRonService;
/// <summary>
/// 欧姆龙服务
/// </summary>
public class OMRonClientService
{
    private string _ipaddress;
    
    private int _port; 
    
    private OmronFinsNet _client;
    /// <summary>
    /// 
    /// </summary>
    private Action<string> _logDataCallBack;
    private bool _isConnect;

    public bool IsConnect
    {
        get => _isConnect;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipaddress"></param>
    /// <param name="port"></param>
    /// <param name="logDataCallBack"></param>
    public OMRonClientService(string ipaddress, int port, Action<string> logDataCallBack=null)
    {
        this._ipaddress = ipaddress; 
        this._port = port;
        this._logDataCallBack = logDataCallBack;
        
    }
    /// <summary>
    /// 
    /// </summary>
    public async Task  StartConnectionServer()
    {
        _client = new OmronFinsNet(  this._ipaddress,  this._port);
         var res= await _client.ConnectServerAsync();
         if (res.IsSuccess)
         {
             _isConnect = true;
             _logDataCallBack?.Invoke("欧姆龙设备连接成功");
         }
         else
         {
             _isConnect = false;
             _logDataCallBack?.Invoke("欧姆龙设备连接失败");
         }
        
        
    }

    public async Task CloseConnectionServer()
    {
        if(_client!=null) await _client.ConnectCloseAsync();
        _isConnect = false;
        _logDataCallBack?.Invoke("欧姆龙设备断开连接");
    }
    /// <summary>
    /// 读取寄存器数据bool
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<bool> ReadBool(string address)
    {
        if (_isConnect == false)
        {
            _logDataCallBack?.Invoke("欧姆龙设备未连接");
            return  false;
        }
        var res = await _client.ReadBoolAsync(address);
        if (res.IsSuccess) return res.Content;
        else
        {
            _logDataCallBack?.Invoke($"读取布尔值失败: {res.Message}");
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task< bool> WriteBool(string address, bool value)
    {
        if (_isConnect == false)
        {
            _logDataCallBack?.Invoke("欧姆龙设备未连接");
            return  false;
        }
        var res = await _client.WriteAsync(address,value);
        if (res.IsSuccess) return true;
        else
        {
            _logDataCallBack?.Invoke($"读取布尔值失败: {res.Message}");
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public short ReadInt16(string address)
    {
        
        if (_isConnect == false)
        {
            _logDataCallBack?.Invoke("欧姆龙设备未连接");
            return 0;
        }
        var res = _client.ReadInt16(address);
        if (res.IsSuccess) return res.Content;
        else
        {
            _logDataCallBack?.Invoke($"读取int16失败: {res.Message}");
            return 0;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task< bool> WriteInt16(string address, short value)
    {
        if (!_isConnect)
        {
            _logDataCallBack?.Invoke("未连接到欧姆龙设备，无法写入数据");
            return false;
        }

        var result = await _client.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            _logDataCallBack?.Invoke($"写入 Int16 值 {value} 到 {address} 成功");
            return true;
        }
        else
        {
            _logDataCallBack?.Invoke($"写入 Int16 值失败: {result.Message}");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public int ReadInt32(string address)
    {
        if (_isConnect == false)
        {
            _logDataCallBack?.Invoke("欧姆龙设备未连接");
            return 0;
        }
        var res = _client.ReadInt32(address);
        if (res.IsSuccess) return res.Content;
        else
        {
            _logDataCallBack?.Invoke($"读取Int32失败: {res.Message}");
            return 0;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> WriteInt32(string address, int value)
    {
        if (!_isConnect)
        {
            _logDataCallBack?.Invoke("未连接到欧姆龙设备，无法写入数据");
            return false;
        }
        var result = await _client.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            _logDataCallBack?.Invoke($"写入 Int32 值 {value} 到 {address} 成功");
            return true;
        }
        else
        {
            _logDataCallBack?.Invoke($"写入 Int32 值失败: {result.Message}");
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public float ReadFloat(string address)
    {
        if (_isConnect == false)
        {
            _logDataCallBack?.Invoke("欧姆龙设备未连接");
            return 0;
        }
        var res = _client.ReadFloat(address);
        if (res.IsSuccess) return res.Content;
        else
        {
            _logDataCallBack?.Invoke($"读取float失败: {res.Message}");
            return 0;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async  Task<bool> WriteFloat(string address, float value)
    {
        if (!_isConnect)
        {
            _logDataCallBack?.Invoke("未连接到欧姆龙设备，无法写入数据");
            return false;
        }
        var result = await _client.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            _logDataCallBack?.Invoke($"写入 Float 值 {value} 到 {address} 成功");
            return true;
        }
        else
        {
            _logDataCallBack?.Invoke($"写入 Float 值失败: {result.Message}");
            return false;
        }
    }
    
}