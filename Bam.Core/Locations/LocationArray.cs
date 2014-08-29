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
    public sealed class LocationArray :
        Array<Location>,
        System.ICloneable
    {
        public
        LocationArray(
            params Location[] input)
        {
            this.AddRange(input);
        }

        public
        LocationArray(
            Array<Location> input)
        {
            this.AddRange(input);
        }

        public override string
        ToString(
            string separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                var locationPath = item.ToString(); // this must be immutable
                builder.AppendFormat("{0}{1}", locationPath, separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        public string
        Stringify(
            string separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                var locationPath = item.GetSinglePath(); // this can be mutable
                builder.AppendFormat("{0}{1}", locationPath, separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            var clone = new LocationArray();
            clone.list.AddRange(this.list);
            return clone;
        }

        #endregion
    }
}
