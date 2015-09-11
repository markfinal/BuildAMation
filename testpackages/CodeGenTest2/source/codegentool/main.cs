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
namespace CodeGenTool
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Not enough arguments");
                return -1;
            }

            string path = System.String.Format("{0}{1}{2}.c", args[0], System.IO.Path.DirectorySeparatorChar, args[1]);

            using (System.IO.TextWriter file = new System.IO.StreamWriter(path))
            {
                if (null == file)
                {
                    System.Console.WriteLine("Unable to open '{0}' for writing", path);
                    return -2;
                }

                file.WriteLine(@"#include <stdio.h>");
                file.WriteLine(@"void MyGeneratedFunction()");
                file.WriteLine(@"{");
                file.WriteLine("\tprintf(\"Hello world\\n\");");
                file.WriteLine(@"}");
                file.WriteLine("");

                System.Console.WriteLine("Generated source file written to '{0}'", path);
            }

            return 0;
        }
    }
}
