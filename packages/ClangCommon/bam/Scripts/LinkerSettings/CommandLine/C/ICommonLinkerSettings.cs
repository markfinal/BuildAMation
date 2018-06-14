#region License
// Copyright (c) 2010-2018, Mark Final
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
            this C.ICommonLinkerSettings settings,
            Bam.Core.StringArray commandLine)
        {
            var module = (settings as Bam.Core.Settings).Module;
            switch (settings.Bits)
            {
            case C.EBit.SixtyFour:
                commandLine.Add("-arch x86_64");
                break;

            case C.EBit.ThirtyTwo:
                commandLine.Add("-arch i386");
                break;

            default:
                throw new Bam.Core.Exception("Unknown bit depth, {0}", settings.Bits.ToString());
            }
            switch (settings.OutputType)
            {
                case C.ELinkerOutput.Executable:
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ConsoleApplication.Key].ToStringQuoteIfNecessary()));
                    break;

            case C.ELinkerOutput.DynamicLibrary:
                {
                    commandLine.Add("-dynamiclib");
                    commandLine.Add(System.String.Format("-o {0}", module.GeneratedPaths[C.ConsoleApplication.Key].ToStringQuoteIfNecessary()));

                    var fullVersionNumber = module.CreateTokenizedString("$(MajorVersion).$(MinorVersion)#valid(.$(PatchVersion))");
                    fullVersionNumber.Parse();
                    var versionString = fullVersionNumber.ToString();
                    commandLine.Add(System.String.Format("-current_version {0}", versionString));
                    // TODO: offer an option of setting the compatibility version differently
                    commandLine.Add(System.String.Format("-compatibility_version {0}", versionString));
                }
                break;
            }
            foreach (var path in settings.LibraryPaths.ToEnumerableWithoutDuplicates())
            {
                commandLine.Add(System.String.Format("-L{0}", path.ToStringQuoteIfNecessary()));
            }
            foreach (var path in settings.Libraries)
            {
                commandLine.Add(path);
            }
            if (settings.DebugSymbols)
            {
                commandLine.Add("-g");
            }
        }
    }
}
