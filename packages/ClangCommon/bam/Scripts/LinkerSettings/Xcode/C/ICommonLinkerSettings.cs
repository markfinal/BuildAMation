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
    public static partial class XcodeLinkerImplementation
    {
        public static void
        Convert(
            this C.ICommonLinkerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            switch (settings.OutputType)
            {
            case C.ELinkerOutput.Executable:
                {
                    configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(string.Empty);
                    configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(module.Tool.Macros["exeext"].Parse().TrimStart(new [] {'.'}));
                }
                break;

            case C.ELinkerOutput.DynamicLibrary:
                {
                    configuration["EXECUTABLE_PREFIX"] = new XcodeBuilder.UniqueConfigurationValue(module.Tool.Macros["dynamicprefix"].Parse());
                    configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(module.Tool.Macros["dynamicext"].Parse().TrimStart(new [] {'.'}));
                    configuration["MACH_O_TYPE"] = new XcodeBuilder.UniqueConfigurationValue("mh_dylib");
                    var osxOpts = settings as C.ILinkerSettingsOSX;
                    if (null != osxOpts.InstallName)
                    {
                        configuration["LD_DYLIB_INSTALL_NAME"] = new XcodeBuilder.UniqueConfigurationValue(osxOpts.InstallName.Parse());
                    }
                    var version = System.String.Format("{0}.{1}", module.Macros["MajorVersion"].Parse(), module.Macros["MinorVersion"].Parse());
                    configuration["DYLIB_CURRENT_VERSION"] = new XcodeBuilder.UniqueConfigurationValue(version);
                    configuration["DYLIB_COMPATIBILITY_VERSION"] = new XcodeBuilder.UniqueConfigurationValue(version);
                }
                break;
            }
            if (settings.LibraryPaths.Count > 0)
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in settings.LibraryPaths)
                {
                    option.Add(path.Parse());
                }
                configuration["LIBRARY_SEARCH_PATHS"] = option;
            }
            if (settings.DebugSymbols.GetValueOrDefault())
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                option.Add("-g");
                configuration["OTHER_LDFLAGS"] = option;
            }
        }
    }
}
