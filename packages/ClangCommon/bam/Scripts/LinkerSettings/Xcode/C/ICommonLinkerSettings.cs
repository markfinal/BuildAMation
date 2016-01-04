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
    public static partial class XcodeLinkerImplementation
    {
        public static void
        Convert(
            this C.ICommonLinkerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            switch (settings.Bits)
            {
            case C.EBit.ThirtyTwo:
                {
                    configuration["VALID_ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("i386");
                    configuration["ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("$(ARCHS_STANDARD_32_BIT)");
                }
                break;

            case C.EBit.SixtyFour:
                {
                    configuration["VALID_ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("x86_64");
                    configuration["ARCHS"] = new XcodeBuilder.UniqueConfigurationValue("$(ARCHS_STANDARD_64_BIT)");
                }
                break;

            default:
                throw new Bam.Core.Exception("Unknown bit depth, {0}", settings.Bits.ToString());
            }
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
                    configuration["EXECUTABLE_EXTENSION"] = new XcodeBuilder.UniqueConfigurationValue(module.Tool.Macros["dynamicextonly"].Parse().TrimStart(new [] {'.'}));
                    configuration["MACH_O_TYPE"] = new XcodeBuilder.UniqueConfigurationValue("mh_dylib");

                    var versionString = module.CreateTokenizedString("$(MajorVersion).$(MinorVersion)#valid(.$(PatchVersion))").Parse();
                    configuration["DYLIB_CURRENT_VERSION"] = new XcodeBuilder.UniqueConfigurationValue(versionString);
                    configuration["DYLIB_COMPATIBILITY_VERSION"] = new XcodeBuilder.UniqueConfigurationValue(versionString);
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
            if (settings.DebugSymbols)
            {
                var option = new XcodeBuilder.MultiConfigurationValue();
                option.Add("-g");
                configuration["OTHER_LDFLAGS"] = option;
            }
        }
    }
}
