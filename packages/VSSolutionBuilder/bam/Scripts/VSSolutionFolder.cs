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
using System.Linq;
namespace VSSolutionBuilder
{
    /// <summary>
    /// Class to represent an IDE folder shown in a solution.
    /// </summary>
    sealed class VSSolutionFolder :
        HasGuid
    {
        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="uniqueName">A unique name for the folder</param>
        /// <param name="path">Path associated with the folder</param>
        public VSSolutionFolder(
            string uniqueName,
            string path)
            :
            base("SolutionFolder" + uniqueName)
        {
            this.Path = path;
            this.NestedEntities = new Bam.Core.Array<HasGuid>();
        }

        /// <summary>
        /// Get the path associated with the folder.
        /// </summary>
        public string Path { get; private set; }

        private Bam.Core.Array<HasGuid> NestedEntities { get; set; }

        /// <summary>
        /// Append an entity to the folder.
        /// </summary>
        /// <param name="entity">Entity that has a GUID to append.</param>
        public void
        appendNestedEntity(
            HasGuid entity)
        {
            lock (this.NestedEntities)
            {
                this.NestedEntities.AddUnique(entity);
            }
        }

        /// <summary>
        /// Serialize the solution folder to the provided stringbuilder
        /// </summary>
        /// <param name="content">StringBuilder to serialise to</param>
        /// <param name="indentLevel">Number of levels of indent.</param>
        public void
        Serialize(
            System.Text.StringBuilder content,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            foreach (var nested in this.NestedEntities)
            {
                content.AppendLine($"{indent}{nested.GuidString} = {this.GuidString}");
            }
        }
    }
}
