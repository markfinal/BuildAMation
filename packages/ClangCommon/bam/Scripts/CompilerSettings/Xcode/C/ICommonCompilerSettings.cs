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
    public static partial class XcodeImplementation
    {
        public static void
        Convert(
            this C.ICommonCompilerSettings options,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            //var objectFile = module as C.ObjectFile;
            if (null != options.Bits)
            {
                switch (options.Bits)
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
                        throw new Bam.Core.Exception("Unknown bit depth");
                }
            }
            if (null != options.DebugSymbols)
            {
                configuration["GCC_GENERATE_DEBUGGING_SYMBOLS"] = new XcodeBuilder.UniqueConfigurationValue((options.DebugSymbols == true) ? "YES" : "NO");
            }
            if (options.DisableWarnings.Count > 0)
            {
                var warnings = new XcodeBuilder.MultiConfigurationValue();
                foreach (var warning in options.DisableWarnings)
                {
                    warnings.Add(System.String.Format("-Wno-{0}", warning));
                }
                configuration["WARNING_CFLAGS"] = warnings;
            }
            if (options.IncludePaths.Count > 0)
            {
                var paths = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in options.IncludePaths)
                {
                    paths.Add(path.ToString());
                }
                configuration["USER_HEADER_SEARCH_PATHS"] = paths;
            }
            if (null != options.OmitFramePointer)
            {
                var arg = (true == options.OmitFramePointer) ? "-fomit-frame-pointer" : "-fno-omit-frame-pointer";
                configuration["OTHER_CFLAGS"] = new XcodeBuilder.MultiConfigurationValue(arg);
            }
            if (null != options.Optimization)
            {
                switch (options.Optimization)
                {
                    case C.EOptimization.Off:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("0");
                        break;
                    case C.EOptimization.Size:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("s");
                        break;
                    case C.EOptimization.Speed:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("1");
                        break;
                    case C.EOptimization.Full:
                        configuration["GCC_OPTIMIZATION_LEVEL"] = new XcodeBuilder.UniqueConfigurationValue("3");
                        break;
                }
            }
            if (options.PreprocessorDefines.Count > 0)
            {
                var defines = new XcodeBuilder.MultiConfigurationValue();
                foreach (var define in options.PreprocessorDefines)
                {
                    if (System.String.IsNullOrEmpty(define.Value))
                    {
                        defines.Add(define.Key);
                    }
                    else
                    {
                        defines.Add(System.String.Format("{0}={1}", define.Key, define.Value));
                    }
                }
                configuration["GCC_PREPROCESSOR_DEFINITIONS"] = defines;
            }
            if (options.PreprocessorUndefines.Count > 0)
            {
                var undefines = new XcodeBuilder.MultiConfigurationValue();
                foreach (var undefine in options.PreprocessorUndefines)
                {
                    undefines.Add(System.String.Format("-U{0}", undefine));
                }
                configuration["OTHER_CFLAGS"] = undefines;
            }
            if (options.SystemIncludePaths.Count > 0)
            {
                var paths = new XcodeBuilder.MultiConfigurationValue();
                foreach (var path in options.SystemIncludePaths)
                {
                    paths.Add(path.ToString());
                }
                configuration["HEADER_SEARCH_PATHS"] = paths;
            }
            if (null != options.TargetLanguage)
            {
                switch (options.TargetLanguage)
                {
                    case C.ETargetLanguage.Default:
                        configuration["GCC_INPUT_FILETYPE"] = new XcodeBuilder.UniqueConfigurationValue("automatic");
                        break;
                    case C.ETargetLanguage.C:
                        configuration["GCC_INPUT_FILETYPE"] = new XcodeBuilder.UniqueConfigurationValue("sourcecode.c.c");
                        break;
                    case C.ETargetLanguage.Cxx:
                        configuration["GCC_INPUT_FILETYPE"] = new XcodeBuilder.UniqueConfigurationValue("sourcecode.cpp.cpp");
                        break;
                    case C.ETargetLanguage.ObjectiveC:
                        configuration["GCC_INPUT_FILETYPE"] = new XcodeBuilder.UniqueConfigurationValue("sourcecode.c.objc");
                        break;
                    case C.ETargetLanguage.ObjectiveCxx:
                        configuration["GCC_INPUT_FILETYPE"] = new XcodeBuilder.UniqueConfigurationValue("sourcecode.cpp.objcpp");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported target language");
                }
            }
            if (null != options.WarningsAsErrors)
            {
                configuration["GCC_TREAT_WARNINGS_AS_ERRORS"] = new XcodeBuilder.UniqueConfigurationValue((true == options.WarningsAsErrors) ? "YES" : "NO");
            }
            if (null != options.OutputType)
            {
                // TODO: anything?
            }
        }
    }
}
