namespace MyMachinePlatformClientCore.Rpc.Rpc;

public class InvocationData
{
    public Guid Id { get; set; }

    public Type MethodDeclaringType { get; set; }

    public string MethodName { get; set; }

    public Type[] GenericArguments { get; set; }

    public Type[] ArgumentTypes { get; set; }

    public object[] Arguments { get; set; }

    public Type ReturnType { get; set; }
}