#region License
// Copyright (c) 2010-2017, Mark Final
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
    public static partial class XcodeAssemblerImplementation
    {
        public static void
        Convert(
            this C.ICommonAssemblerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            switch (settings.Bits.Value)
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
                throw new Bam.Core.Exception("Unknown bit depth, {0}", settings.Bits.Value);
            }
            configuration["GCC_GENERATE_DEBUGGING_SYMBOLS"] = new XcodeBuilder.UniqueConfigurationValue(settings.DebugSymbols ? "YES" : "NO");
            if (settings.IncludePaths.Count > 0)
            {
                var paths = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in settings.IncludePaths)
                {
                    var fullPath = path.Parse();
                    var relPath = Bam.Core.RelativePathUtilities.GetPath(fullPath, configuration.Project.SourceRoot);
                    if (Bam.Core.RelativePathUtilities.IsPathAbsolute(relPath))
                    {
                        paths.Add(fullPath);
                    }
                    else
                    {
                        paths.Add(System.String.Format("$(SRCROOT)/{0}", relPath));
                    }
                }
                configuration["USER_HEADER_SEARCH_PATHS"] = paths;
            }
            if (settings.PreprocessorDefines.Count > 0)
            {
                var defines = new XcodeBuilder.MultiConfigurationValue();
                foreach (var define in settings.PreprocessorDefines)
                {
                    if (System.String.IsNullOrEmpty(define.Value))
                    {
                        defines.Add(define.Key);
                    }
                    else
                    {
                        var value = define.Value;
                        if (value.Contains("\""))
                        {
                            // note the number of back slashes here
                            // required to get \\\" for each " in the original value
                            value = value.Replace("\"", "\\\\\\\"");
                        }
                        defines.Add(System.String.Format("{0}={1}", define.Key, value));
                    }
                }
                configuration["GCC_PREPROCESSOR_DEFINITIONS"] = defines;
            }
            configuration["GCC_TREAT_WARNINGS_AS_ERRORS"] = new XcodeBuilder.UniqueConfigurationValue(settings.WarningsAsErrors ? "YES" : "NO");
        }
    }
}
