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
    /// Attribute representing output paths
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)] // because there may be multiple outputs
    public class OutputPathAttribute :
        BaseAttribute
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="pathKey">Path key for the output file type</param>
        /// <param name="command_switch">Command line switch associated with these output file types</param>
        /// <param name="inheritExisting">Optional, whether to inherit values. Default is false.</param>
        /// <param name="target">Optional, which target to use. Default to settings.</param>
        /// <param name="handledByMetaData">Optional, whether this output file is handled by metadata. Default to false.</param>
        /// <param name="enableSideEffets">Optional, whether this output file type has side-effects enabled. Default to false.</param>
        public OutputPathAttribute(
            string pathKey,
            string command_switch,
            bool inheritExisting = false,
            TargetGroup target = TargetGroup.Settings,
            bool handledByMetaData = false,
            bool enableSideEffets = false)
            :
            base(command_switch, inheritExisting, target)
        {
            this.PathKey = pathKey;
            this.HandledByMetaData = handledByMetaData;
            this.EnableSideEffects = enableSideEffets;
        }

        /// <summary>
        /// The path key for these output file types.
        /// </summary>
        public string PathKey { get; private set; }

        /// <summary>
        /// Whether these output files are handled by metadata
        /// </summary>
        public bool HandledByMetaData { get; private set; }

        /// <summary>
        /// Whether side effects are enabled
        /// </summary>
        public bool EnableSideEffects { get; private set; }
    }
}
