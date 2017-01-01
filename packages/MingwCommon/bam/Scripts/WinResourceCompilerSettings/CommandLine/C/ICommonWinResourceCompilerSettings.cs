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
        private static string
        GetShortPathName(
            string longPath)
        {
            var shortPath = new System.Text.StringBuilder(longPath.Length + 1);

            if (0 == GetShortPathName(longPath, shortPath, shortPath.Capacity))
            {
                return longPath;
            }

            return shortPath.ToString();
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern System.Int32 GetShortPathName(string path, System.Text.StringBuilder shortPath, System.Int32 shortPathLength);

        public static void
        Convert(
            this C.ICommonWinResourceCompilerSettings settings,
            Bam.Core.StringArray commandLine)
        {
            if (settings.Verbose.HasValue && settings.Verbose.Value)
            {
                commandLine.Add("-v");
            }
            foreach (var path in settings.IncludePaths)
            {
                var realpath = path.Parse();
                if (path.ContainsSpace)
                {
                    // windres cannot cope with paths with spaces, even when quoted
                    // https://sourceware.org/bugzilla/show_bug.cgi?id=4356
                    var shortPath = GetShortPathName(realpath);
                    commandLine.Add(System.String.Format("--include-dir={0}", shortPath));
                }
                else
                {
                    commandLine.Add(System.String.Format("--include-dir={0}", realpath));
                }
            }

            var resource = (settings as Bam.Core.Settings).Module as C.WinResource;
            commandLine.Add("--use-temp-file"); // avoiding a popen error, see https://amindlost.wordpress.com/2012/06/09/mingw-windres-exe-cant-popen-error/
            commandLine.Add(System.String.Format("-o {0}", resource.GeneratedPaths[C.ObjectFile.Key].ParseAndQuoteIfNecessary()));
        }
    }
}
