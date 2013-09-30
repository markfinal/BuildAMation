// <copyright file="IComplement.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    // TODO: this needs to be renamed to something corresponding to Set operations
    public interface IComplement<T>
    {
        T Complement(T other);
        T Intersect(T other);
    }
}
