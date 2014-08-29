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
namespace C
{
    public sealed class VisualStudioToolAttributeDictionary :
        System.Collections.Generic.Dictionary<string, string>
    {
        public void
        Merge(
            VisualStudioToolAttributeDictionary dictionary)
        {
            foreach (var option in dictionary)
            {
                var attributeName = option.Key;
                var attributeValue = option.Value;
                if (this.ContainsKey(attributeName))
                {
                    var updatedAttributeValue = new System.Text.StringBuilder(this[attributeName]);
                    var splitter = attributeValue.ToString()[attributeValue.Length - 1];
                    if (System.Char.IsLetterOrDigit(splitter))
                    {
                        throw new Bam.Core.Exception("Splitter character is a letter or digit");
                    }
                    var splitNew = attributeValue.ToString().Split(splitter);
                    foreach (var split in splitNew)
                    {
                        if (!System.String.IsNullOrEmpty(split) && !this[attributeName].Contains(split))
                        {
                            updatedAttributeValue.AppendFormat("{0}{1}", split, splitter);
                        }
                    }
                    this[attributeName] = updatedAttributeValue.ToString();
                }
                else
                {
                    this[attributeName] = attributeValue.ToString();
                }
            }
        }
    }
}
