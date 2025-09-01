namespace MyMachinePlatformClientCore.Rpc.Rpc;

//
// 摘要:
//     Represents a invocation context while sending to server.
public class SendInvocationContext
{
    //
    // 摘要:
    //     Gets the id of invockation
    public Guid Id { get; }

    //
    // 摘要:
    //     Gets the content of invocation,in bytes.
    public byte[] InvocationBytes { get; }

    //
    // 摘要:
    //     The custom token associated with client registered service.
    public object ServiceToken { get; }

    //
    // 摘要:
    //     Create an instance of SendInvocationContext.
    //
    // 参数:
    //   id:
    //     The id of the invocation.
    //
    //   invocationBytes:
    //     The content of the invocation, in bytes.
    //
    //   serviceToken:
    //     The custom token associated with client registered service.
    public SendInvocationContext(Guid id, byte[] invocationBytes, object serviceToken)
    {
        Id = id;
        InvocationBytes = invocationBytes;
        ServiceToken = serviceToken;
    }

    //
    // 摘要:
    //     Create an instance of SendInvocationContext.
    //
    // 参数:
    //   id:
    //     The id of the invocation.
    //
    //   invocationBytes:
    //     The content of the invocation, in bytes.
    public SendInvocationContext(Guid id, byte[] invocationBytes)
        : this(id, invocationBytes, null)
    {

    }
}