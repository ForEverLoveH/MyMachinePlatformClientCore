using System.Reflection;
using MyMachinePlatformClientCore.Rpc.Exceptions;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

 internal static class InvocationExecutor
 {
     internal static object Execute(object serviceInstance, InvocationData invData)
     {
         MethodInfo[] methods = serviceInstance.GetType().GetMethods();
         MethodInfo methodInfo = null;
         bool flag = false;
         MethodInfo[] array = methods;
         foreach (MethodInfo methodInfo2 in array)
         {
             if (!(methodInfo2.Name == invData.MethodName))
             {
                 continue;
             }

             flag = true;
             if (methodInfo2.GetGenericArguments().Length != invData.GenericArguments.Length)
             {
                 continue;
             }

             ParameterInfo[] parameters = methodInfo2.GetParameters();
             if (parameters.Length != invData.ArgumentTypes.Length)
             {
                 continue;
             }

             bool flag2 = true;
             if (parameters.Length != 0)
             {
                 for (int j = 0; j < parameters.Length; j++)
                 {
                     if (parameters[j].ParameterType != invData.ArgumentTypes[j])
                     {
                         flag2 = false;
                         break;
                     }
                 }
             }

             if (flag2)
             {
                 methodInfo = methodInfo2;
                 break;
             }
         }

         if (methodInfo == null)
         {
             if (flag)
             {
                 throw new RpcMethodNotMatchException();
             }

             throw new RpcMethodNotFoundException();
         }

         if (invData.GenericArguments.Length != 0)
         {
             methodInfo = methodInfo.MakeGenericMethod(invData.GenericArguments);
         }

         try
         {
             return methodInfo.Invoke(serviceInstance, invData.Arguments);
         }
         catch (Exception ex)
         {
             if (ex.InnerException != null)
             {
                 throw ex.InnerException;
             }

             throw;
         }
     }
 }