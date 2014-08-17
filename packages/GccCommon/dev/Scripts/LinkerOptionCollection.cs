#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
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
                // we use gcc as the linker - if there is C++ code included, link against libstdc++
                foreach (var child in node.Children)
                {
                    if (child.Module is C.Cxx.ObjectFile || child.Module is C.Cxx.ObjectFileCollection |
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        cLinkerOptions.Libraries.Add("-lstdc++");
                        break;
                    }
                }

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

        public
        LinkerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
