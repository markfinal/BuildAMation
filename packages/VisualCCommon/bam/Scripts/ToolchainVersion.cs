#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace VisualCCommon
{
    /// <summary>
    /// VisualC toolchain version wrapper.
    /// </summary>
    public sealed class ToolchainVersion :
        C.ToolchainVersion
    {
        /// <summary>
        /// VisualC 2010
        /// </summary>
        public static readonly C.ToolchainVersion VC2010 = FromMSCVer(1600);

        /// <summary>
        /// VisualC 2012
        /// </summary>
        public static readonly C.ToolchainVersion VC2012 = FromMSCVer(1700);

        /// <summary>
        /// VisualC 2013
        /// </summary>
        public static readonly C.ToolchainVersion VC2013 = FromMSCVer(1800);

        /// <summary>
        /// VisualC 2015
        /// </summary>
        public static readonly C.ToolchainVersion VC2015 = FromMSCVer(1900);

        /// <summary>
        /// VisualC 2017 15.0
        /// </summary>
        public static readonly C.ToolchainVersion VC2017_15_0 = FromMSCVer(1910);

        /// <summary>
        /// VisualC 2017 15.3
        /// </summary>
        public static readonly C.ToolchainVersion VC2017_15_3 = FromMSCVer(1911);

        /// <summary>
        /// VisualC 2017 15.5
        /// </summary>
        public static readonly C.ToolchainVersion VC2017_15_5 = FromMSCVer(1912);

        /// <summary>
        /// VisualC 2017 15.6
        /// </summary>
        public static readonly C.ToolchainVersion VC2017_15_6 = FromMSCVer(1913);

        /// <summary>
        /// VisualC 2017 15.7
        /// </summary>
        public static readonly C.ToolchainVersion VC2017_15_7 = FromMSCVer(1914);

        /// <summary>
        /// VisualC 2017 15.8
        /// </summary>
        public static readonly C.ToolchainVersion VC2017_15_8 = FromMSCVer(1915);

        private ToolchainVersion(
            int mscVer) => this.combinedVersion = mscVer;

        /// <summary>
        /// Generate a VisualC toolchain version from _MSC_VER
        /// </summary>
        /// <param name="mscVer">The specified _MSC_VER value.</param>
        /// <returns>Toolchain version</returns>
        static public C.ToolchainVersion
        FromMSCVer(
            int mscVer) => new ToolchainVersion(mscVer);
    }
}
