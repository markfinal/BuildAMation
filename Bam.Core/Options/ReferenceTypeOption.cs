#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
                    throw new Exception("Type {0} does not implement the Bam.Core.ISetOperations<{1}> interface", thisValue.GetType().ToString(), typeof(T).ToString());
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
                    throw new Exception("Type {0} does not implement the Bam.Core.ISetOperations<{1}> interface", thisValue.GetType().ToString(), typeof(T).ToString());
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
