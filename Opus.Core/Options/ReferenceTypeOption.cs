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
            if (null == this.Value)
            {
                Option defaultClone = System.Activator.CreateInstance(this.GetType(), new object[] { null }) as Option;
                defaultClone.PrivateData = this.PrivateData;
                return defaultClone;
            }

            System.ICloneable cloneable = this.Value as System.ICloneable;
            if (null == cloneable)
            {
                throw new Exception("ReferenceTypeOption type, '{0}', is not cloneable", typeof(T).ToString());
            }

            object untypedClonedValue = cloneable.Clone();
            T clonedValue = untypedClonedValue as T;
            if (null == clonedValue)
            {
                throw new Exception("Casting type '{0}' as '{1}' is not a defined type conversion", untypedClonedValue.GetType().ToString(), typeof(T).ToString());
            }
            ReferenceTypeOption<T> clonedOption = new ReferenceTypeOption<T>(clonedValue);

            // we can share private data
            clonedOption.PrivateData = this.PrivateData;

            return clonedOption;
        }
    }
}