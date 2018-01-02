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
namespace MingwCommon
{
    public static partial class CommandLineImplementation
    {
        public static void
        Convert(
            this C.ICommonAssemblerSettings settings,
            Bam.Core.StringArray commandLine)
        {
            var module = (settings as Bam.Core.Settings).Module;
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
            if (settings.DebugSymbols)
            {
                commandLine.Add("-g");
            }
            switch (settings.OutputType)
            {
                case C.ECompilerOutput.CompileOnly:
                    commandLine.Add(System.String.Format("-c -o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                    break;
                case C.ECompilerOutput.Preprocess:
                    commandLine.Add(System.String.Format("-E -o {0}", module.GeneratedPaths[C.ObjectFile.Key].ToString()));
                    break;
                default:
                    throw new Bam.Core.Exception("Unknown output type, {0}", settings.OutputType.ToString());
            }
            if (settings.WarningsAsErrors)
            {
                commandLine.Add("-Werror");
            }
            else
            {
                commandLine.Add("-Wno-error");
            }
            foreach (var path in settings.IncludePaths.ToEnumerableWithoutDuplicates())
            {
                commandLine.Add(System.String.Format("-I{0}", path.ToStringQuoteIfNecessary()));
            }
            foreach (var define in settings.PreprocessorDefines)
            {
                if (null == define.Value)
                {
                    commandLine.Add(System.String.Format("-D{0}", define.Key));
                }
                else
                {
                    var defineValue = define.Value.ToString();
                    if (defineValue.Contains("\""))
                    {
                        defineValue = defineValue.Replace("\"", "\\\"");
                    }
                    commandLine.Add(System.String.Format("-D{0}={1}", define.Key, defineValue));
                }
            }
        }
    }
}
