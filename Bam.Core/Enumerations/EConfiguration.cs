#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// Configuration enumeration
    /// </summary>
    [System.Flags]
    public enum EConfiguration
    {
        /// <summary>
        /// No such configuration
        /// </summary>
        Invalid   = 0,

        /// <summary>
        /// Debug settings are enabled by default, with no optimizations.
        /// </summary>
        Debug     = (1 << 0),

        /// <summary>
        /// Full optimizations are enabled by default, with no debug settings.
        /// </summary>
        Optimized = (1 << 1),

        /// <summary>
        /// Full optimizations are enabled by default, but debug information is available.
        /// </summary>
        Profile   = (1 << 2),

        /// <summary>
        /// Alias for all configurations
        /// </summary>
        All       = Debug | Optimized | Profile,

        /// <summary>
        /// Alias for not the debug configuration
        /// </summary>
        NotDebug     = ~Debug & All,

        /// <summary>
        /// Alias for not the optimized configuration
        /// </summary>
        NotOptimized = ~Optimized & All,


        /// <summary>
        /// Alias for not the profile configuration
        /// </summary>
        NotProfile   = ~Profile & All
    }
}
