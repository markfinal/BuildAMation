// <copyright file="FlagsBase.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class FlagsBase
    {
        public int InternalValue
        {
            get; 
            protected set;
        }

        private int next = 1;
        private string name;

        protected FlagsBase(string name)
        {
            this.name = name;
            this.InternalValue = this.next;
            this.next <<= 1;
        }

        public override string ToString()
        {
            return name;
        }
    }
}