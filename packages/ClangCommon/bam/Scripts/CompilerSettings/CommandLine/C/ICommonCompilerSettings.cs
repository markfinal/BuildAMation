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
    public static partial class CommandLineCompilerImplementation
    {
        public static void
        Convert(
            this C.ICommonCompilerSettings settings,
            Bam.Core.StringArray commandLine)
        {
            if (settings.Bits.HasValue)
            {
                switch (settings.Bits.Value)
                {
                    case C.EBit.SixtyFour:
                        commandLine.Add("-arch x86_64");
                        break;

                    case C.EBit.ThirtyTwo:
                        commandLine.Add("-arch i386");
                        break;

                    default:
                        throw new Bam.Core.Exception("Unknown bit depth, {0}", settings.Bits.Value);
                }
            }
            if (settings.DebugSymbols.HasValue)
            {
                if (settings.DebugSymbols.Value)
                {
                    commandLine.Add("-g");
                }
            }
            foreach (var warning in settings.DisableWarnings)
            {
                commandLine.Add(System.String.Format("-Wno-{0}", warning));
            }
            foreach (var path in settings.IncludePaths)
            {
                commandLine.Add(System.String.Format("-I{0}", path.ParseAndQuoteIfNecessary()));
            }
            if (settings.OmitFramePointer.HasValue)
            {
                commandLine.Add(settings.OmitFramePointer.Value ? "-fomit-frame-pointer" : "-fno-omit-frame-pointer");
            }
            if (settings.Optimization.HasValue)
            {
                switch (settings.Optimization.Value)
                {
                    case C.EOptimization.Off:
                        commandLine.Add("-O0");
                        break;
                    case C.EOptimization.Size:
                        commandLine.Add("-Os");
                        break;
                    case C.EOptimization.Speed:
                        commandLine.Add("-O1");
                        break;
                    case C.EOptimization.Full:
                        commandLine.Add("-O3");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported optimization, {0}", settings.Optimization.Value);
                }
            }
            foreach (var define in settings.PreprocessorDefines)
            {
                if (System.String.IsNullOrEmpty(define.Value))
                {
                    commandLine.Add(System.String.Format("-D{0}", define.Key));
                }
                else
                {
                    var value = define.Value;
                    if (value.Contains("\""))
                    {
                        value = value.Replace("\"", "\\\"");
                    }
                    commandLine.Add(System.String.Format("-D{0}={1}", define.Key, value));
                }
            }
            foreach (var undefine in settings.PreprocessorUndefines)
            {
                commandLine.Add(System.String.Format("-U{0}", undefine));
            }
            foreach (var path in settings.SystemIncludePaths)
            {
                commandLine.Add(System.String.Format("-I{0}", path.ParseAndQuoteIfNecessary()));
            }
            if (settings.TargetLanguage.HasValue)
            {
                switch (settings.TargetLanguage.Value)
                {
                    case C.ETargetLanguage.C:
                        commandLine.Add("-x c");
                        break;
                    case C.ETargetLanguage.Cxx:
                        commandLine.Add("-x c++");
                        break;
                    case C.ETargetLanguage.ObjectiveC:
                        commandLine.Add("-x objective-c");
                        break;
                    case C.ETargetLanguage.ObjectiveCxx:
                        commandLine.Add("-x objective-c++");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported target language, {0}", settings.TargetLanguage.Value);
                }
            }
            if (settings.WarningsAsErrors.HasValue)
            {
                if (settings.WarningsAsErrors.Value)
                {
                    commandLine.Add("-Werror");
                }
                else
                {
                    commandLine.Add("-Wno-error");
                }
            }
            if (settings.OutputType.HasValue)
            {
                var module = (settings as Bam.Core.Settings).Module;
                switch (settings.OutputType.Value)
                {
                    case C.ECompilerOutput.CompileOnly:
                        commandLine.Add(System.String.Format("-c -o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                    case C.ECompilerOutput.Preprocess:
                        commandLine.Add(System.String.Format("-E -o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported output type, {0}", settings.OutputType.Value);
                }
            }
        }
    }
}
