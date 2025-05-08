using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.IService
{ 
    public interface  ILoginService
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
         Task<bool > LoginAsync(string userName, string password);
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        Task<bool> RegisterAsync(string userName,string password, string email, string phone);

    }
}
