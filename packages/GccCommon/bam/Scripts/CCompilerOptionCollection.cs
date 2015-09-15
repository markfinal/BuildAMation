#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace GccCommon
{
    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void
            Defaults(this GccCommon.ICommonCompilerSettings settings, Bam.Core.Module module)
            {
                settings.PositionIndependentCode = false;
            }

            public static void
            Defaults(this GccCommon.ICommonLinkerSettings settings, Bam.Core.Module module)
            {
                settings.CanUseOrigin = false;
                settings.RPath = new Bam.Core.StringArray();
                settings.RPathLink = new Bam.Core.StringArray();
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this GccCommon.ICommonCompilerSettings options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.PositionIndependentCode)
            {
                if (true == options.PositionIndependentCode)
                {
                    commandLine.Add("-fPIC");
                }
            }
        }

        public static void
        Convert(
            this GccCommon.ICommonLinkerSettings options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.CanUseOrigin)
            {
                if (true == options.CanUseOrigin)
                {
                    commandLine.Add("-Wl,-z,origin");
                }
            }
            foreach (var rpath in options.RPath)
            {
                commandLine.Add(System.String.Format("-Wl,-rpath,{0}", rpath));
            }
            foreach (var rpath in options.RPathLink)
            {
                commandLine.Add(System.String.Format("-Wl,-rpath-link,{0}", rpath));
            }
        }
    }
}
