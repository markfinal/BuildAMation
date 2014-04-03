// <copyright file="ISetOperations.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface ISetOperations<T>
    {
        T Complement(T other);
        T Intersect(T other);
    }
}
