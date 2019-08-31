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
namespace VisualStudioProcessor
{
    /// <summary>
    /// Attribute representing inputs to a module
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    sealed class InputPathsAttribute :
        BaseAttribute
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="pathKey">The path key for the input file</param>
        /// <param name="command_switch">Switch associated with this type of input file</param>
        /// <param name="max_file_count">Optional, maximum number of files expected. Default to -1 which is unlimited.</param>
        /// <param name="inheritExisting">Optional, whether values are inherited from the parent. Default is false.</param>
        /// <param name="target">Optional, which target is used. Default to settings.</param>
        /// <param name="handledByMetaData">Optional, whether this input file is handled in metadata rather than requiring an explicit command line switch. Default is false.</param>
        public InputPathsAttribute(
            string pathKey,
            string command_switch,
            int max_file_count = -1,
            bool inheritExisting = false,
            TargetGroup target = TargetGroup.Settings,
            bool handledByMetaData = false)
            :
            base(command_switch, inheritExisting, target)
        {
            this.PathKey = pathKey;
            this.MaxFileCount = max_file_count;
            this.HandledByMetaData = handledByMetaData;
        }

        /// <summary>
        /// Path key for these input files.
        /// </summary>
        public string PathKey { get; private set; }

        /// <summary>
        /// Maximum number of input files handled.
        /// </summary>
        public int MaxFileCount { get; private set; }

        /// <summary>
        /// Whether these input files are handled by metadata.
        /// </summary>
        public bool HandledByMetaData { get; private set; }
    }
}
