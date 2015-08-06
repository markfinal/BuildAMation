#region License
// Copyright 2010-2015 Mark Final
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
namespace V2
{
    public sealed class PreprocessorDefinitions :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>
    {
        private System.Collections.Generic.Dictionary<string, string> Defines = new System.Collections.Generic.Dictionary<string, string>();

        public void Add(string name, string value)
        {
            this.Defines.Add(name, value);
        }

        public void Add(string name)
        {
            this.Add(name, string.Empty);
        }

        public int Count
        {
            get
            {
                return this.Defines.Count;
            }
        }

        public bool
        Contains(
            string key)
        {
            return this.Defines.ContainsKey(key);
        }

        public string
        this[string key]
        {
            get
            {
                return this.Defines[key];
            }
        }

        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var item in this.Defines)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
    public sealed class DefineCollection :
        System.ICloneable,
        Bam.Core.ISetOperations<DefineCollection>
    {
        private Bam.Core.StringArray defines = new Bam.Core.StringArray();

        public void
        Add(
            object toAdd)
        {
            var define = toAdd as string;
            if (define.Contains(" "))
            {
                throw new Bam.Core.Exception("Preprocessor definitions cannot contain a space: '{0}'", define);
            }
            lock (this.defines)
            {
                if (!this.defines.Contains(define))
                {
                    this.defines.Add(define);
                }
                else
                {
                    Bam.Core.Log.DebugMessage("The define '{0}' is already present", define);
                }
            }
        }

        public int Count
        {
            get
            {
                return this.defines.Count;
            }
        }

        public System.Collections.Generic.IEnumerator<string>
        GetEnumerator()
        {
            return this.defines.GetEnumerator();
        }

        public object
        Clone()
        {
            var clonedObject = new DefineCollection();
            foreach (var define in this.defines)
            {
                clonedObject.Add(define.Clone() as string);
            }
            return clonedObject;
        }

        public Bam.Core.StringArray
        ToStringArray()
        {
            return this.defines;
        }

        public override bool
        Equals(
            object obj)
        {
            var otherCollection = obj as DefineCollection;
            return (this.defines.Equals(otherCollection.defines));
        }

        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        DefineCollection
        Bam.Core.ISetOperations<DefineCollection>.Complement(
            DefineCollection other)
        {
            var complementDefines = this.defines.Complement(other.defines);
            if (0 == complementDefines.Count)
            {
                throw new Bam.Core.Exception("DefineCollection complement is empty");
            }

            var complementDefinesCollection = new DefineCollection();
            complementDefinesCollection.defines.AddRange(complementDefines);
            return complementDefinesCollection;
        }

        DefineCollection
        Bam.Core.ISetOperations<DefineCollection>.Intersect(
            DefineCollection other)
        {
            var intersectDefines = this.defines.Intersect(other.defines);
            var intersectDefinesCollection = new DefineCollection();
            intersectDefinesCollection.defines.AddRange(intersectDefines);
            return intersectDefinesCollection;
        }
    }
}
