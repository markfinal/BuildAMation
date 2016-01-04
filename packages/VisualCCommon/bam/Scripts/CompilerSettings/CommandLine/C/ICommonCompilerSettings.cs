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
namespace VisualCCommon
{
    public static partial class CommandLineImplementation
    {
        public static void
        Convert(
            this C.ICommonCompilerSettings settings,
            Bam.Core.StringArray commandLine)
        {
            var module = (settings as Bam.Core.Settings).Module;
            if (settings.DebugSymbols.HasValue)
            {
                if (settings.DebugSymbols.Value)
                {
                    commandLine.Add("-Z7");
                }
            }
            foreach (var warning in settings.DisableWarnings)
            {
                commandLine.Add(System.String.Format("-wd{0}", warning));
            }
            foreach (var path in settings.IncludePaths)
            {
                commandLine.Add(System.String.Format("-I{0}", path.ParseAndQuoteIfNecessary()));
            }
            if (settings.OmitFramePointer.HasValue)
            {
                commandLine.Add(settings.OmitFramePointer.Value ? "-Oy" : "-Oy-");
            }
            if (settings.Optimization.HasValue)
            {
                switch (settings.Optimization.Value)
                {
                    case C.EOptimization.Off:
                        commandLine.Add("-Od");
                        break;
                    case C.EOptimization.Size:
                        commandLine.Add("-Os");
                        break;
                    case C.EOptimization.Speed:
                        commandLine.Add("-O1");
                        break;
                    case C.EOptimization.Full:
                        commandLine.Add("-Ox");
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
                    commandLine.Add(System.String.Format("-D{0}={1}", define.Key, define.Value));
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
                        commandLine.Add("-TC");
                        break;
                    case C.ETargetLanguage.Cxx:
                        commandLine.Add("-TP");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unsupported target language, {0}", settings.TargetLanguage.Value.ToString());
                }
            }
            if (settings.WarningsAsErrors.HasValue)
            {
                if (settings.WarningsAsErrors.Value)
                {
                    commandLine.Add("-WX");
                }
            }
            if (settings.OutputType.HasValue)
            {
                switch (settings.OutputType.Value)
                {
                    case C.ECompilerOutput.CompileOnly:
                        commandLine.Add(System.String.Format("-c -Fo{0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                    case C.ECompilerOutput.Preprocess:
                        commandLine.Add(System.String.Format("-E -Fo{0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                        break;
                    default:
                        throw new Bam.Core.Exception("Unknown output type, {0}", settings.OutputType.Value.ToString());
                }
            }
        }
    }
}
