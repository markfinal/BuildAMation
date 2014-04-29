// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, ILinkerOptions
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            (this as C.ILinkerOptions).DoNotAutoIncludeStandardLibraries = false; // TODO: fix this - requires a bunch of stuff to be added to the command line
            (this as ILinkerOptions).EnableAutoImport = false;

            // we use gcc as the linker - if there is C++ code included, link against libstdc++
            foreach (Opus.Core.DependencyNode child in node.Children)
            {
                if (child.Module is C.Cxx.ObjectFile || child.Module is C.Cxx.ObjectFileCollection)
                {
                    (this as C.ILinkerOptions).Libraries.Add("-lstdc++");
                    break;
                }
            }

            /*
             This is an example link line using gcc with -v

Linker Error: ' C:/MinGW/bin/../libexec/gcc/mingw32/3.4.5/collect2.exe -Bdynamic -o d:\build\Test2-dev\Application\win32-debug-mingw\Application.exe C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../crt2.o C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtbegin.o -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5 -LC:/MinGW/bin/../lib/gcc -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../../mingw32/lib -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../.. --subsystem console d:\build\Test2-dev\Application\win32-debug-mingw\application.o d:\build\Test2-dev\Library\win32-debug-mingw\libLibrary.a d:\build\Test3-dev\Library2\win32-debug-mingw\libLibrary2.a -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt -luser32 -lkernel32 -ladvapi32 -lshell32 -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtend.o'
             */
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (this.OutputPaths.Has(C.OutputFileFlags.Executable))
            {
                string outputPathName = this.OutputFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(outputPathName));
            }

            if (this.OutputPaths.Has(C.OutputFileFlags.StaticImportLibrary))
            {
                string libraryPathName = this.StaticImportLibraryFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(libraryPathName));
            }

            return directoriesToCreate;
        }
    }
}