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
namespace Mingw
{
    /// <summary>
    /// Class representing metadata for this version of Mingw
    /// </summary>
    public class MetaData :
        Bam.Core.PackageMetaData,
        C.IToolchainDiscovery
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string, object>();

        /// <summary>
        /// Indexer into the meta data
        /// </summary>
        /// <param name="index">String index as lookup</param>
        /// <returns>Meta data at the index</returns>
        public override object this[string index] => this.Meta[index];

        public override bool
        Contains(
            string index) => this.Meta.ContainsKey(index);

        /// <summary>
        /// Suffix applied to tool filenames
        /// </summary>
        public string ToolSuffix => this.Meta["ToolSuffix"] as string;

        void
        C.IToolchainDiscovery.Discover(
            C.EBit? depth)
        {
            if (this.Contains("InstallDir"))
            {
                return;
            }

            // TODO: can this come from the registry?
            this.Meta.Add("InstallDir", Bam.Core.TokenizedString.CreateVerbatim(@"C:\MinGW"));

            // TODO: some installations may not have a suffix - need to confirm
            this.Meta.Add("ToolSuffix", "-4.8.1");

            Bam.Core.Log.Info($"Using Mingw 4.8.1 installed at {this.Meta["InstallDir"].ToString()}");
        }
    }
}
