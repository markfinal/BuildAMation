// <copyright file="StringArray.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class StringArray : Array<string>, System.ICloneable, Opus.Core.ISetOperations<StringArray>
    {
        public StringArray()
            : base()
        {
        }

        public StringArray(params string[] itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public StringArray(System.Collections.ICollection collection)
        {
            foreach (string item in collection)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public StringArray(StringArray array)
        {
            foreach (var item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public StringArray(Opus.Core.Array<string> array)
        {
            foreach (var item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public override void Add(string item)
        {
            if (System.String.IsNullOrEmpty(item))
            {
                return;
            }

            this.list.Add(item);
        }

        public override string ToString()
        {
            return this.ToString(' ');
        }

        public string ToString(char separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                builder.AppendFormat("{0}{1}", item.ToString(), separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator);
            return output;
        }

        public void RemoveDuplicates()
        {
            var newList = new System.Collections.Generic.List<string>();
            foreach (var item in this.list)
            {
                if (!newList.Contains(item))
                {
                    newList.Add(item);
                }
            }

            this.list = newList;
        }

        object System.ICloneable.Clone()
        {
            var clone = new StringArray();
            clone.list.AddRange(this.list);
            return clone;
        }

        #region ISetOperations implementation

        StringArray ISetOperations<StringArray>.Complement (StringArray other)
        {
            return new StringArray((this as Array<string>).Complement(other as Array<string>));
        }

        StringArray ISetOperations<StringArray>.Intersect (StringArray other)
        {
            return new StringArray((this as Array<string>).Intersect(other as Array<string>));
        }

        #endregion
    }
}