// <copyright file="ValueTypeOption.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class ValueTypeOption<T> :
        Option where T : struct
    {
        public
        ValueTypeOption(
            T value)
        {
            this.Value = value;
        }

        public T Value
        {
            get;
            set;
        }

        public override object
        Clone()
        {
            var clonedOption = new ValueTypeOption<T>(this.Value);

            // we can share the private data
            clonedOption.PrivateData = this.PrivateData;

            return clonedOption;
        }

        public override string
        ToString()
        {
            return System.String.Format("{0}", this.Value);
        }

        public override bool
        Equals(
            object obj)
        {
            var thisValue = this.Value;
            var otherValue = ((ValueTypeOption<T>)(obj)).Value;
            var equals = thisValue.Equals(otherValue);
            return equals;
        }

        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Option
        Complement(
            Option other)
        {
            // two value types will be singular value so if they are not equal, their complement is the first value
            return this.Clone() as Option;
        }

        public override Option
        Intersect(
            Option other)
        {
            // two value types will be singular value so if they are not equal, they will not intersect
            return null;
        }
    }
}
