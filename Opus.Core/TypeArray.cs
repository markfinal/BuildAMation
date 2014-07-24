// <copyright file="TypeArray.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class TypeArray :
        Array<System.Type>
    {
        public
        TypeArray() :
        base()
        {}

        public
        TypeArray(
            params System.Type[] itemsToAdd) :
        base(itemsToAdd)
        {}
    }
}
