namespace MyMachinePlatformClientCore.Rpc.Rpc;

public class ReturnData
{
    public Guid Id { get; set; }

    public int HttpStatusCode { get; set; }

    public string ExceptionMessage { get; set; }
}
internal class ReturnData<T> : ReturnData
{
    public T ReturnObject { get; set; }
}