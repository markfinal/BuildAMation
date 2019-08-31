#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace XcodeBuilder
{
    /// <summary>
    /// Base class for all objects in the Xcode project.
    /// </summary>
    abstract class Object
    {
        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="project">Project that the object belongs to.</param>
        /// <param name="name">Name of the object.</param>
        /// <param name="isa">IsA string, referring to the type of the object.</param>
        /// <param name="hashComponents">Array of string hashes for the components of the object.</param>
        protected Object(
            Project project,
            string name,
            string isa,
            params string[] hashComponents)
        {
            this.Project = project;
            if (null == project)
            {
                if (this is Project)
                {
                    this.Project = this as Project;
                }
                else
                {
                    throw new Bam.Core.Exception("Invalid project");
                }
            }
            this.Name = name;
            this.IsA = isa;
            this.GUID = this.MakeGUID(name, isa, hashComponents);
        }

        /// <summary>
        /// Make a 96-bit GUID for Xcode objects.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="isa">IsA type of the object.</param>
        /// <param name="hashComponents">Array of string hashes for components.</param>
        /// <returns></returns>
        private string
        MakeGUID(
            string name,
            string isa,
            params string[] hashComponents)
        {
            // create the string to hash
            var source = new System.Text.StringBuilder();
            source.Append(name);
            source.Append(isa);
            foreach (var comp in hashComponents)
            {
                source.Append(comp);
            }
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(source.ToString());
            var hash = md5.ComputeHash(inputBytes);
            var stringBuilder = new System.Text.StringBuilder();
            // Xcode GUIDs are 96-bits, so take the least significant bits
            for (var i = 0; i < 12; ++i)
            {
                stringBuilder.Append(hash[i].ToString("X2"));
            }
            var guid = stringBuilder.ToString();
            // ensure that the generating UUID is unique within the project
            this.Project.AddGUID(guid, this);
            return guid;
        }

        /// <summary>
        /// Get the project that the Object belongs to.
        /// </summary>
        public Project Project { get; private set; }

        /// <summary>
        /// Get the GUID of the Object.
        /// </summary>
        public string GUID { get; private set; }

        /// <summary>
        /// Get the name of the Object.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get the IsA type of the object.
        /// </summary>
        public string IsA { get; private set; }

        /// <summary>
        /// Serialize the Object.
        /// </summary>
        /// <param name="text">StringBuffer to write to.</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public abstract void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel);

        /// <summary>
        /// Cleanse the provided path for any inconsistencies.
        /// </summary>
        /// <param name="inputString">Path to cleanse.</param>
        /// <returns>Cleansed path.</returns>
        protected string
        CleansePaths(
            string inputString)
        {
            // Xcode always treats /private/tmp as /tmp internally, so the project must do so too
            // otherwise string comparisons of paths will fail, especially for output paths of
            // shell script build phases
            var cleanedString = inputString?.Replace("/private/tmp", "/tmp");
            return cleanedString;
        }
    }
}
