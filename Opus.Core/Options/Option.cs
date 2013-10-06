// <copyright file="Option.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class Option : System.ICloneable
    {
        public object PrivateData
        {
            get;
            set;
        }

        public abstract object Clone();

        public abstract Option Complement(Option other);
        public abstract Option Intersect(Option other);
    }
}