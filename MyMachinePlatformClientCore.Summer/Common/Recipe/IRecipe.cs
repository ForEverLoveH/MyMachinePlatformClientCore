using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public interface IRecipe : IEnumerable<RecipeItem>, IEnumerable, IRecipeProvider
    {
        /// <summary>获取指定的配方配置</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        new T Get<T>(string name);

        /// <summary>当前配方的名称</summary>
        string Name { get; }

        /// <summary>设置指定的配方配置</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void Set<T>(string name, T value);

        /// <summary>判断配方中是否包含指定的配置</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Contains(string name);
    }
}
