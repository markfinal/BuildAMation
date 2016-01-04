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
namespace GccCommon
{
    public static partial class CommandLineImplementation
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
                        commandLine.Add("-m64");
                        break;

                    case C.EBit.ThirtyTwo:
                        commandLine.Add("-m32");
                        break;

                    default:
                        throw new Bam.Core.Exception("Unknown machine bit size, {0}", settings.Bits.Value.ToString());
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
                        throw new Bam.Core.Exception("Unknown optimization level, {0}", settings.Optimization.Value.ToString());
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
                    if (define.Value.Contains("\""))
                    {
                        // specifically, without the outer quotes, this was causing an issue in a bash shell
                        // when the value contained an escaped double quote
                        commandLine.Add(System.String.Format("-D{0}=\"{1}\"", define.Key, define.Value));
                    }
                    else
                    {
                        commandLine.Add(System.String.Format("-D{0}={1}", define.Key, define.Value));
                    }
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
                        throw new Bam.Core.Exception("Unsupported target language, {0}", settings.TargetLanguage.Value.ToString());
                }
            }
            if (settings.WarningsAsErrors.HasValue)
            {
                if (settings.WarningsAsErrors.Value)
                {
                    commandLine.Add("-Werror");
                }
            }
            if (settings.OutputType.HasValue)
            {
                var module = (settings as Bam.Core.Settings).Module;
                switch (settings.OutputType)
                {
                    case C.ECompilerOutput.CompileOnly:
                        commandLine.Add(System.String.Format("-c -o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                    case C.ECompilerOutput.Preprocess:
                        commandLine.Add(System.String.Format("-E -o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                }
            }
        }
    }
}
