// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
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
