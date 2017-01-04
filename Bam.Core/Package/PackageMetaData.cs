#region License
// Copyright (c) 2010-2017, Mark Final
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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Base class for all package metadata.
    /// </summary>
    public abstract class PackageMetaData
    {
        /// <summary>
        /// Construct the basic metadata.
        /// User scripts can hook into this construction, by implementing the IPackageMetaDataConfigure interface.
        /// </summary>
        protected PackageMetaData()
        {
            var thisType = this.GetType();
            var configureInterfaceType = typeof(IPackageMetaDataConfigure<>).MakeGenericType(thisType);
            var metaType = Graph.Instance.ScriptAssembly.GetTypes().FirstOrDefault(item => configureInterfaceType.IsAssignableFrom(item));
            if (null == metaType)
            {
                return;
            }
            var configureInstance = System.Activator.CreateInstance(metaType);
            var configureMethod = configureInterfaceType.GetMethod("Configure", new[] { thisType });
            configureMethod.Invoke(configureInstance, new[] { this });
        }

        /// <summary>
        /// Obtain data stored in the metadata at a string based index.
        /// </summary>
        /// <param name="index">Index.</param>
        public abstract object this[string index]
        {
            get;
        }

        /// <summary>
        /// Determine whether an index is present in the metadata.
        /// </summary>
        /// <param name="index">Index.</param>
        public abstract bool
        Contains(
            string index);
    }
}
