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
namespace C
{
namespace V2
{
    public sealed class PreprocessorDefinitions :
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>
    {
        private System.Collections.Generic.Dictionary<string, string> Defines = new System.Collections.Generic.Dictionary<string, string>();

        public PreprocessorDefinitions()
        {}

        public PreprocessorDefinitions(
            System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> items)
        {
            foreach (var item in items)
            {
                this.Defines.Add(item.Key, item.Value);
            }
        }

        public void Add(string name, string value)
        {
            if (this.Defines.ContainsKey(name))
            {
                if (this.Defines[name] != value)
                {
                    throw new Bam.Core.Exception("Preprocessor define {0} already exists with value {1}. Cannot change it to {2}", name, this.Defines[name], value);
                }
                return;
            }
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

        public override string ToString()
        {
            var content = new System.Text.StringBuilder();
            foreach (var item in this.Defines)
            {
                if (System.String.IsNullOrEmpty(item.Value))
                {
                    content.AppendFormat("{0};", item.Key);
                }
                else
                {
                    content.AppendFormat("{0}={1};", item.Key, item.Value);
                }
            }
            return content.ToString();
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
