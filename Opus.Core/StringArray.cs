// <copyright file="StringArray.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class StringArray : Array<string>
    {
        public StringArray()
            : base()
        {
        }

        public StringArray(params string[] itemsToAdd)
        {
            foreach (string item in itemsToAdd)
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
            foreach (string item in array)
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
            string output = "";
            foreach (string item in this.list)
            {
                output += item.ToString() + separator;
            }
            output = output.TrimEnd(separator);
            return output;
        }
    }
}