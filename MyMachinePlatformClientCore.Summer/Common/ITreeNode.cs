using MyMachinePlatformClientCore.Summer.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public interface ITreeNode<T> where T : ITreeNode<T>
    {
        /// <summary>添加子节点。</summary>
        event EventHandler<T> NodeAdded;

        /// <summary>添加子节点中。</summary>
        event EventHandler<CancelEventArgs<T>> NodeAdding;

        /// <summary>删除子节点。</summary>
        event EventHandler<T> NodeRemoved;

        /// <summary>删除子节点中。</summary>
        event EventHandler<CancelEventArgs<T>> NodeRemoving;

        /// <summary>
        ///     All nodes along path toward root: Parent, Parent.Parent, Parent.Parent.Parent, ...
        /// </summary>
        IEnumerable<T> Ancestors { get; }

        /// <summary>Direct descendants.</summary>
        IEnumerable<T> Children { get; }

        /// <summary>Distance from Root.</summary>
        int Depth { get; }

        /// <summary>Children, Children[i].Children, ...</summary>
        IEnumerable<T> Descendants { get; }

        /// <summary>Distance from deepest descendant.</summary>
        int Height { get; }

        /// <summary>表示一个树节点列表，其中包含当前节点以及其后树节点。</summary>
        IEnumerable<T> Nodes { get; }

        /// <summary>父节点。</summary>
        T Parent { get; set; }

        /// <summary>根节点。</summary>
        T Root { get; }

        /// <summary>添加子节点。</summary>
        /// <param name="node"></param>
        /// <returns></returns>
        void Add(T node);

        /// <summary>批量添加子节点。</summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        void Add(params T[] nodes);

        /// <summary>删除子节点。</summary>
        /// <param name="node"></param>
        /// <returns></returns>
        void Remove(T node);

        /// <summary>批量删除子节点。</summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        void Remove(params T[] nodes);

        /// <summary>删除所有子节点</summary>
        void Clear();

        /// <summary>设置父节点。</summary>
        /// <param name="parent"></param>
        void SetParent(T parent);
    }
}
