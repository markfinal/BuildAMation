// <copyright file="ReferenceTypeOption.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class ReferenceTypeOption<T> : Option where T : class
    {
        public ReferenceTypeOption(T value)
        {
            this.Value = value;
        }

        public T Value
        {
            get;
            set;
        }

        public override object Clone()
        {
            System.ICloneable cloneable = this.Value as System.ICloneable;
            if (null == cloneable)
            {
                throw new Exception(System.String.Format("ReferenceTypeOption type, '{0}', is not cloneable", typeof(T).GetType().ToString()), false);
            }

            ReferenceTypeOption<T> clonedOption = new ReferenceTypeOption<T>(cloneable.Clone() as T);

            // we can share private data
            clonedOption.PrivateData = this.PrivateData;

            return clonedOption;
        }
    }
}