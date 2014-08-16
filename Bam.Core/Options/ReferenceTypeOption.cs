// <copyright file="ReferenceTypeOption.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class ReferenceTypeOption<T> :
        Option where T : class
    {
        public
        ReferenceTypeOption(
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
            if (null == this.Value)
            {
                var defaultClone = System.Activator.CreateInstance(this.GetType(), new object[] { null }) as Option;
                defaultClone.PrivateData = this.PrivateData;
                return defaultClone;
            }

            // TODO: might be necessary if we ever have to clone certain option collections
#if false
            // handle System.Type and System.Enum values as reference types with value semantics
            if (typeof(System.Type).IsInstanceOfType(this.Value) ||
                typeof(System.Enum).IsInstanceOfType(this.Value))
            {
                ReferenceTypeOption<T> specialClone = new ReferenceTypeOption<T>(this.Value);

                // we can share the private data
                specialClone.PrivateData = this.PrivateData;

                return specialClone;
            }
#endif

            var cloneable = this.Value as System.ICloneable;
            if (null == cloneable)
            {
                throw new Exception("ReferenceTypeOption type, '{0}', is not cloneable", typeof(T).ToString());
            }

            var untypedClonedValue = cloneable.Clone();
            var clonedValue = untypedClonedValue as T;
            if (null == clonedValue)
            {
                throw new Exception("Casting type '{0}' as '{1}' is not a defined type conversion", untypedClonedValue.GetType().ToString(), typeof(T).ToString());
            }
            var clonedOption = new ReferenceTypeOption<T>(clonedValue);

            // we can share private data
            clonedOption.PrivateData = this.PrivateData;

            return clonedOption;
        }

        public override bool
        Equals(
            object obj)
        {
            var thisValue = this.Value;
            var otherValue = ((ReferenceTypeOption<T>)(obj)).Value;
            if (thisValue == null)
            {
                return (otherValue == null);
            }
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
            var thisValue = this.Value;
            var otherValue = (other as ReferenceTypeOption<T>).Value;

            if (!typeof(ISetOperations<T>).IsAssignableFrom(thisValue.GetType()))
            {
                if (thisValue.GetType() == typeof(string))
                {
                    if (thisValue.Equals(otherValue))
                    {
                        return null;
                    }
                    else
                    {
                        return this.Clone() as Option;
                    }
                }
                else
                {
                    throw new Exception("Type {0} does not implement the Opus.Core.ISetOperations<{1}> interface", thisValue.GetType().ToString(), typeof(T).ToString());
                }
            }

            var complementInterface = thisValue as ISetOperations<T>;
            var complementResult = complementInterface.Complement(otherValue);

            var complementOption = new ReferenceTypeOption<T>(complementResult);
            // we can share private data
            complementOption.PrivateData = this.PrivateData;

            return complementOption;
        }

        public override Option
        Intersect(
            Option other)
        {
            var thisValue = this.Value;
            var otherValue = (other as ReferenceTypeOption<T>).Value;

            if (!typeof(ISetOperations<T>).IsAssignableFrom(thisValue.GetType()))
            {
                if (thisValue.GetType() == typeof(string))
                {
                    if (thisValue.Equals(otherValue))
                    {
                        return this.Clone() as Option;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    throw new Exception("Type {0} does not implement the Opus.Core.ISetOperations<{1}> interface", thisValue.GetType().ToString(), typeof(T).ToString());
                }
            }

            var complementInterface = thisValue as ISetOperations<T>;
            var intersectResult = complementInterface.Intersect(otherValue);

            var intersectOption = new ReferenceTypeOption<T>(intersectResult);
            // we can share private data
            intersectOption.PrivateData = this.PrivateData;

            return intersectOption;
        }
    }
}
