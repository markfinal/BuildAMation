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
namespace ClangCommon
{
    public static partial class CommandLineLinkerImplementation
    {
        public static void
        Convert(
            this C.ICommonLinkerSettingsOSX settings,
            Bam.Core.StringArray commandLine)
        {
            var module = (settings as Bam.Core.Settings).Module;
            foreach (var framework in settings.Frameworks)
            {
                var frameworkName = System.IO.Path.GetFileNameWithoutExtension(framework.Parse());
                commandLine.Add(System.String.Format("-framework {0}", frameworkName));
            }
            foreach (var path in settings.FrameworkSearchPaths)
            {
                commandLine.Add(System.String.Format("-F {0}", path.Parse()));
            }
            if (null != settings.InstallName)
            {
                if (module is C.IDynamicLibrary)
                {
                    commandLine.Add(System.String.Format("-Wl,-dylib_install_name,{0}", settings.InstallName.Parse()));
                }
            }
            if (settings.MinimumVersionSupported != null)
            {
                var minVersionRegEx = new System.Text.RegularExpressions.Regex("^(?<Type>[a-z]+)(?<Version>[0-9.]+)$");
                var match = minVersionRegEx.Match(settings.MinimumVersionSupported);
                if (!match.Groups["Type"].Success)
                {
                    throw new Bam.Core.Exception("Unable to extract SDK type from: '{0}'", settings.MinimumVersionSupported);
                }
                if (!match.Groups["Version"].Success)
                {
                    throw new Bam.Core.Exception("Unable to extract SDK version from: '{0}'", settings.MinimumVersionSupported);
                }
                commandLine.Add(System.String.Format("-m{0}-version-min={1}",
                    match.Groups["Type"].Value,
                    match.Groups["Version"].Value));
            }
        }
    }
}
