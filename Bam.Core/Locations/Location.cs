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
    /// Location is the abstract base class that can represent any of the higher form of Locations.
    /// </summary>
    public abstract class Location
    {
        public enum EExists
        {
            Exists,
            WillExist
        }

        public abstract Location
        SubDirectory(
            string subDirName);

        public abstract Location
        SubDirectory(
            string subDirName,
            EExists exists);

        public virtual string AbsolutePath
        {
            get;
            protected set;
        }

        public abstract LocationArray
        GetLocations();

        public abstract bool IsValid
        {
            get;
        }

        public EExists Exists
        {
            get;
            protected set;
        }

        /// <summary>
        /// This is a special case of GetLocations. It requires a Location resolves to a single path, which is returned.
        /// If the path contains a space, it will be returned quoted.
        /// </summary>
        /// <returns>Single path of the resolved Location</returns>
        public string
        GetSinglePath()
        {
            var path = this.GetSingleRawPath();
            if (path.Contains(" "))
            {
                path = System.String.Format("\"{0}\"", path);
            }
            return path;
        }

        /// <summary>
        /// This is a special case of GetLocations. It requires a Location resolves to a single path, which is returned.
        /// No checking whether the path contains spaces is performed.
        /// </summary>
        /// <returns>Single path of the resolved Location</returns>
        public string
        GetSingleRawPath()
        {
            var resolvedLocations = this.GetLocations();
            if (resolvedLocations.Count != 1)
            {
                throw new Exception("Location '{0}' did not resolve just one path, but {1}", this.ToString(), resolvedLocations.Count);
            }
            var path = resolvedLocations[0].AbsolutePath;
            return path;
        }
    }
}
