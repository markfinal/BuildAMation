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
    public static partial class XcodeCompilerImplementation
    {
        public static void
        Convert(
            this C.ICxxOnlyCompilerSettings settings,
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            if (settings.ExceptionHandler.HasValue)
            {
                switch (settings.ExceptionHandler.Value)
                {
                case C.Cxx.EExceptionHandler.Disabled:
                    configuration["GCC_ENABLE_CPP_EXCEPTIONS"] = new XcodeBuilder.UniqueConfigurationValue("NO");
                    break;

                case C.Cxx.EExceptionHandler.Asynchronous:
                case C.Cxx.EExceptionHandler.Synchronous:
                    configuration["GCC_ENABLE_CPP_EXCEPTIONS"] = new XcodeBuilder.UniqueConfigurationValue("YES");
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized exception handler option, {0}", settings.ExceptionHandler.Value.ToString());
                }
            }
            if (settings.EnableRunTimeTypeInfo.HasValue)
            {
                configuration["GCC_ENABLE_CPP_RTTI"] = new XcodeBuilder.UniqueConfigurationValue(settings.EnableRunTimeTypeInfo.Value ? "YES" : "NO");
            }
            if (settings.LanguageStandard.HasValue)
            {
                XcodeBuilder.UniqueConfigurationValue standard = null;
                switch (settings.LanguageStandard.Value)
                {
                    case C.Cxx.ELanguageStandard.Cxx98:
                        standard = new XcodeBuilder.UniqueConfigurationValue("c++98");
                        break;

                    case C.Cxx.ELanguageStandard.GnuCxx98:
                        standard = new XcodeBuilder.UniqueConfigurationValue("gnu++98");
                        break;

                    case C.Cxx.ELanguageStandard.Cxx03:
                        standard = new XcodeBuilder.UniqueConfigurationValue("c++03");
                        break;

                    case C.Cxx.ELanguageStandard.GnuCxx03:
                        standard = new XcodeBuilder.UniqueConfigurationValue("gnu++03");
                        break;

                    case C.Cxx.ELanguageStandard.Cxx11:
                        standard = new XcodeBuilder.UniqueConfigurationValue("c++11");
                        break;

                    case C.Cxx.ELanguageStandard.GnuCxx11:
                        standard = new XcodeBuilder.UniqueConfigurationValue("gnu++11");
                        break;

                    case C.Cxx.ELanguageStandard.Cxx14:
                        standard = new XcodeBuilder.UniqueConfigurationValue("c++14");
                        break;

                    case C.Cxx.ELanguageStandard.GnuCxx14:
                        standard = new XcodeBuilder.UniqueConfigurationValue("gnu++14");
                        break;

                    default:
                        throw new Bam.Core.Exception("Invalid C++ language standard, {0}", settings.LanguageStandard.Value.ToString());
                }
                configuration["CLANG_CXX_LANGUAGE_STANDARD"] = standard;
            }
            if (settings.StandardLibrary.HasValue)
            {
                switch (settings.StandardLibrary.Value)
                {
                case C.Cxx.EStandardLibrary.NotSet:
                    break;

                case C.Cxx.EStandardLibrary.libstdcxx:
                    configuration["CLANG_CXX_LIBRARY"] = new XcodeBuilder.UniqueConfigurationValue("libstdc++");
                    break;

                case C.Cxx.EStandardLibrary.libcxx:
                    configuration["CLANG_CXX_LIBRARY"] = new XcodeBuilder.UniqueConfigurationValue("libc++");
                    break;

                default:
                    throw new Bam.Core.Exception("Invalid C++ standard library, {0}", settings.StandardLibrary.Value.ToString());
                }
            }
        }
    }
}
