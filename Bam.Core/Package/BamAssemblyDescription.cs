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
namespace Bam.Core
{
    /// <summary>
    /// Information concerned each Bam assembly reference, including name and minimum
    /// version number supported.
    /// </summary>
    public class BamAssemblyDescription
    {
        /// <summary>
        /// Construct a new instance, with just a name.
        /// </summary>
        /// <param name="name">Name.</param>
        public
        BamAssemblyDescription(
            string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Construct a new instance, with a name and a major version number.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="major">Major.</param>
        public
        BamAssemblyDescription(
            string name,
            int major)
            :
            this(name)
        {
            this.MajorVersion = major;
        }

        /// <summary>
        /// Construct a new instance, with a name, major and minor version numbers.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        public BamAssemblyDescription(
            string name,
            int major,
            int minor)
            :
            this(name, major)
        {
            this.MinorVersion = minor;
        }

        /// <summary>
        /// Construct a new instance, with a name, major, minor and patch version numbers.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="patch">Patch.</param>
        public BamAssemblyDescription(
            string name,
            int major,
            int minor,
            int patch)
            :
            this(name, major, minor)
        {
            this.PatchVersion = patch;
        }

        /// <summary>
        /// Get the name of the assembly.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Determine the minimum version number of the assembly, including
        /// all the parts of the version number.
        /// </summary>
        /// <returns>The version number.</returns>
        public string MinimumVersionNumber()
        {
            if (!this.MajorVersion.HasValue)
            {
                return null;
            }
            var number = new System.Text.StringBuilder();
            number.Append(this.MajorVersion.Value.ToString());
            if (this.MinorVersion.HasValue)
            {
                number.AppendFormat(".{0}", this.MinorVersion.Value.ToString());
                if (this.PatchVersion.HasValue)
                {
                    number.AppendFormat(".{0}", this.PatchVersion.Value.ToString());
                }
            }
            return number.ToString();
        }

        /// <summary>
        /// Get the assembly major version number, if defined.
        /// </summary>
        /// <value>The major version.</value>
        public int? MajorVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the assembly minor version number, if defined.
        /// </summary>
        /// <value>The minor version.</value>
        public int? MinorVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the assembly patch version number, if defined.
        /// </summary>
        /// <value>The patch version.</value>
        public int? PatchVersion
        {
            get;
            private set;
        }
    }
}
