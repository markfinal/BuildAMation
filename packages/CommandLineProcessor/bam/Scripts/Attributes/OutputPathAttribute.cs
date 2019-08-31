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
namespace CommandLineProcessor
{
    /// <summary>
    /// Attribute representing an output path for command line conversion
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)] // because there may be multiple outputs
    sealed class OutputPathAttribute :
        BaseAttribute
    {
        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="pathKey">Path key to the output file type.</param>
        /// <param name="command_switch">Switch for the settings property.</param>
        /// <param name="path_modifier">Optional modifier to the path. Default to null.</param>
        /// <param name="ignore">Optional ignore this path. Default to false.</param>
        public OutputPathAttribute(
            string pathKey,
            string command_switch,
            string path_modifier = null,
            bool ignore = false)
            :
            base(command_switch)
        {
            this.PathKey = pathKey;
            this.PathModifier = path_modifier;
            this.Ignore = ignore;
        }

        /// <summary>
        /// Path key for the output file type.
        /// </summary>
        public string PathKey { get; private set; }

        /// <summary>
        /// Modifier for the path.
        /// </summary>
        public string PathModifier { get; private set; }

        /// <summary>
        /// Whether to ignore the output file.
        /// </summary>
        public bool Ignore { get; private set; }
    }
}
