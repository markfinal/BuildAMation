// <copyright file="UniqueList.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class UniqueList<T> : System.Collections.Generic.List<T>
    {
        public new void Add(T obj)
        {
            if (!this.Contains(obj))
            {
                base.Add(obj);
            }
        }
    }
}