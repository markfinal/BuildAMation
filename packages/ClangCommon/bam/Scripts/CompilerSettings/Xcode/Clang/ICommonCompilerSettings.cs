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
namespace ClangCommon
{
    public static partial class XcodeCompilerImplementation
    {
        public static void
        Convert(
            this ClangCommon.ICommonCompilerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            if (settings.AllWarnings.HasValue)
            {
                if (settings.AllWarnings.Value)
                {
                    var warnings = new XcodeBuilder.MultiConfigurationValue();
                    warnings.Add("-Wall");
                    configuration["WARNING_CFLAGS"] = warnings;
                }
            }
            if (settings.ExtraWarnings.HasValue)
            {
                if (settings.ExtraWarnings.Value)
                {
                    var warnings = new XcodeBuilder.MultiConfigurationValue();
                    warnings.Add("-Wextra");
                    configuration["WARNING_CFLAGS"] = warnings;
                }
            }
            if (settings.Pedantic.HasValue)
            {
                configuration["GCC_WARN_PEDANTIC"] = new XcodeBuilder.UniqueConfigurationValue(settings.Pedantic.Value ? "YES" : "NO");
            }
            if (settings.Visibility.HasValue)
            {
                configuration["GCC_SYMBOLS_PRIVATE_EXTERN"] = new XcodeBuilder.UniqueConfigurationValue((settings.Visibility.Value == EVisibility.Default) ? "NO" : "YES");
            }
        }
    }
}
