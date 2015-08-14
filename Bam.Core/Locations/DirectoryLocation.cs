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
    /// <summary>
    /// DirectoryLocation represents a single directory on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// DirectoryLocations are cached internally, so only a single instance of a directory path exists.
    /// </summary>
    public sealed class DirectoryLocation :
        Location,
        System.ICloneable
    {
        private static System.Collections.Generic.Dictionary<int, DirectoryLocation> cache = new System.Collections.Generic.Dictionary<int, DirectoryLocation>();

        private
        DirectoryLocation(
            string absolutePath,
            Location.EExists exists)
        {
            if (exists == EExists.Exists)
            {
                if (!System.IO.Directory.Exists(absolutePath))
                {
                    throw new Exception("Directory '{0}' does not exist", absolutePath);
                }
            }
            this.AbsolutePath = absolutePath;
            this.Exists = exists;
        }

        public static DirectoryLocation
        Get(
            string absolutePath,
            Location.EExists exists)
        {
            var hash = absolutePath.GetHashCode();
            if (cache.ContainsKey(hash))
            {
                return cache[hash];
            }

            // because the cache might be written into by multiple threads
            lock (cache)
            {
                var instance = new DirectoryLocation(absolutePath, exists);
                cache[hash] = instance;
                return instance;
            }
        }

        public static DirectoryLocation
        Get(
            string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public override Location
        SubDirectory(
            string subDirName)
        {
            return this.SubDirectory(subDirName, this.Exists);
        }

        public override Location
        SubDirectory(
            string subDirName,
            EExists exists)
        {
            var subDirectoryPath = System.IO.Path.Combine(this.AbsolutePath, subDirName);
            return Get(subDirectoryPath, exists) as Location;
        }

        public override string
        ToString()
        {
            return System.String.Format("Directory '{0}':{1}", this.AbsolutePath, this.Exists.ToString());
        }

        public override LocationArray
        GetLocations()
        {
            return new LocationArray(this);
        }

        public override bool IsValid
        {
            get
            {
                return true;
            }
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            // Note, this is not using the hash location, as this really needs to be a separate instance
            var clone = new DirectoryLocation(this.AbsolutePath, this.Exists);
            return clone;
        }

        #endregion
    }
}
