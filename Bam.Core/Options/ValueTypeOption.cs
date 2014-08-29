#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
