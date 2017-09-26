using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSNetwork.Interface
{
    /// <summary>
    /// The basic pool interface
    /// </summary>
    public interface IPool
    {
        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        int TotalCount { get; }

        /// <summary>
        /// Gets the available count, the items count which are available to be used.
        /// </summary>
        /// <value>
        /// The available count.
        /// </value>
        int IdleCount { get; }
    }

    /// <summary>
    /// The basic pool interface for the object in type of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPool<T> : IPool
    {
        /// <summary>
        /// Gets one item from the pool.
        /// </summary>
        /// <returns></returns>
        T Alloc();

        /// <summary>
        /// Returns the specified item to the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        void Free(T item);
    }

    /// <summary>
    /// The pool item creator interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPoolCreator<T>
    {
        /// <summary>
        /// Creates the items of the specified count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        IEnumerable<T> Create(int count);
    }
}
