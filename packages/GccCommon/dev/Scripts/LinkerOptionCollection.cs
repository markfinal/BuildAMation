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
namespace GccCommon
{
    public partial class LinkerOptionCollection :
        C.LinkerOptionCollection,
        C.ILinkerOptions,
        C.ILinkerOptionsOSX,
        GccCommon.ILinkerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var target = node.Target;

            var localLinkerOptions = this as ILinkerOptions;
            localLinkerOptions.SixtyFourBit = Bam.Core.OSUtilities.Is64Bit(target);

            var cLinkerOptions = this as C.ILinkerOptions;
            cLinkerOptions.DoNotAutoIncludeStandardLibraries = false; // TODO: fix this - requires a bunch of stuff to be added to the command line

            localLinkerOptions.CanUseOrigin = false;
            localLinkerOptions.AllowUndefinedSymbols = (node.Module is C.DynamicLibrary);
            localLinkerOptions.RPath = new Bam.Core.StringArray();

            if (null != node.Children)
            {
                // we use gcc as the link - if there is ObjectiveC code included, link against -lobjc
                foreach (var child in node.Children)
                {
                    if (child.Module is C.ObjC.ObjectFile || child.Module is C.ObjC.ObjectFileCollection |
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        cLinkerOptions.Libraries.Add("-lobjc");
                        break;
                    }
                }
            }

            /*
             This is an example link line using gcc with -v

Linker Error: ' C:/MinGW/bin/../libexec/gcc/mingw32/3.4.5/collect2.exe -Bdynamic -o d:\build\Test2-dev\Application\win32-debug-mingw\Application.exe C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../crt2.o C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtbegin.o -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5 -LC:/MinGW/bin/../lib/gcc -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../../mingw32/lib -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../.. --subsystem console d:\build\Test2-dev\Application\win32-debug-mingw\application.o d:\build\Test2-dev\Library\win32-debug-mingw\libLibrary.a d:\build\Test3-dev\Library2\win32-debug-mingw\libLibrary2.a -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt -luser32 -lkernel32 -ladvapi32 -lshell32 -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtend.o'
             */
        }

        public override void FinalizeOptions(Bam.Core.DependencyNode node)
        {
            if (null != node.Children)
            {
                var cLinkerOptions = this as C.ILinkerOptions;
                // we use gcc as the linker - if there is C++ code included, link against libstdc++
                // of libc++ depending whether Clang is used
                foreach (var child in node.Children)
                {
                    if (child.Module is C.Cxx.ObjectFile || child.Module is C.Cxx.ObjectFileCollection |
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        var cOptions = child.Module.Options as C.ICCompilerOptions;
                        if ((cOptions.LanguageStandard == C.ELanguageStandard.Cxx11) &&
                            string.Equals(node.Target.ToolsetName('='), "clang", System.StringComparison.OrdinalIgnoreCase))
                        {
                            cLinkerOptions.Libraries.Add("-lc++");
                        }
                        else
                        {
                            cLinkerOptions.Libraries.Add("-lstdc++");
                        }
                        break;
                    }
                }
            }

            base.FinalizeOptions (node);
        }

        public
        LinkerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
