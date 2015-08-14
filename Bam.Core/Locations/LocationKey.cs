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
namespace V2
{
    using System.Linq;

    /// <summary>
    /// Unique keys representing a set of files, through a factory method
    /// </summary>
    public sealed class FileKey
    {
        private FileKey(string key)
        {
            this.Id = key;
        }

        private static System.Collections.Generic.List<FileKey> GeneratedKeys = new System.Collections.Generic.List<FileKey>();

        public static FileKey Generate(string key)
        {
            var matches = GeneratedKeys.Where(item => (item.Id == key));
            if (1 == matches.Count())
            {
                return matches.ElementAt(0);
            }
            var newKey = new FileKey(key);
            GeneratedKeys.Add(newKey);
            return newKey;
        }

        public string Id
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Id.Equals((obj as FileKey).Id);
        }

        public override string ToString()
        {
            return this.Id;
        }
    }
}

    /// <summary>
    /// Abstraction of the key to the LocationMap
    /// </summary>
    public sealed class LocationKey
    {
        public
        LocationKey(
            string identifier,
            ScaffoldLocation.ETypeHint type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        private string Identifier
        {
            get;
            set;
        }

        public ScaffoldLocation.ETypeHint Type
        {
            get;
            private set;
        }

        public bool IsFileKey
        {
            get
            {
                bool isFileKey = (this.Type == ScaffoldLocation.ETypeHint.File);
                return isFileKey;
            }
        }

        public bool IsDirectoryKey
        {
            get
            {
                bool isDirectoryKey = (this.Type == ScaffoldLocation.ETypeHint.Directory);
                return isDirectoryKey;
            }
        }

        public bool IsSymlinkKey
        {
            get
            {
                bool isSymlinkKey = (this.Type == ScaffoldLocation.ETypeHint.Symlink);
                return isSymlinkKey;
            }
        }

        public override string
        ToString()
        {
            return this.Identifier;
        }
    }
}
