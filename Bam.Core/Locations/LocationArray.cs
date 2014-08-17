// <copyright file="LocationArray.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
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
