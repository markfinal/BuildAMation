// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions
    {
        private void SetDelegates(Opus.Core.Target target)
        {
            // common linker options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine);
            this["SubSystem"].PrivateData = new PrivateData(null);
            this["IgnoreStandardLibraries"].PrivateData = new PrivateData(IgnoreStandardLibrariesCommandLine);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLine);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLine);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLine);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLine);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLine);

            // linker specific options
            this["64bit"].PrivateData = new PrivateData(SixtyFourBitCommandLine);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;

            this["64bit"] = new Opus.Core.ValueTypeOption<bool>(target.Platform == Opus.Core.EPlatform.Unix64);

            this.IgnoreStandardLibraries = false; // TODO: fix this - requires a bunch of stuff to be added to the command line

            /*
             This is an example link line using gcc with -v

Linker Error: ' C:/MinGW/bin/../libexec/gcc/mingw32/3.4.5/collect2.exe -Bdynamic -o d:\build\Test2-dev\Application\win32-debug-mingw\Application.exe C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../crt2.o C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtbegin.o -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5 -LC:/MinGW/bin/../lib/gcc -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../../mingw32/lib -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../.. --subsystem console d:\build\Test2-dev\Application\win32-debug-mingw\application.o d:\build\Test2-dev\Library\win32-debug-mingw\libLibrary.a d:\build\Test3-dev\Library2\win32-debug-mingw\libLibrary2.a -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt -luser32 -lkernel32 -ladvapi32 -lshell32 -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtend.o'
             */

            this.SetDelegates(target);
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override string OutputFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.Executable];
            }
            set
            {
                this.OutputPaths[C.OutputFileFlags.Executable] = value;
            }
        }

        public override string StaticImportLibraryFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.StaticImportLibrary];
            }
            set
            {
                this.OutputPaths[C.OutputFileFlags.StaticImportLibrary] = value;
            }
        }

        public override string MapFilePath
        {
            get
            {
                return this.OutputPaths[C.OutputFileFlags.MapFile];
            }
            set
            {
                this.OutputPaths[C.OutputFileFlags.MapFile] = value;
            }
        }

        protected static void OutputTypeSetHandler(object sender, Opus.Core.Option option)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                    {
                        string executablePathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName);
                        options.OutputFilePath = executablePathname;
                    }
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    {
                        string dynamicLibraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, "lib" + options.OutputName + ".so");
                        string importLibraryPathname = dynamicLibraryPathname;
                        options.OutputFilePath = dynamicLibraryPathname;
                        options.StaticImportLibraryFilePath = importLibraryPathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void OutputTypeCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                    commandLineBuilder.Append(System.String.Format("-o \"{0}\" ", options.OutputFilePath));
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    {
                        commandLineBuilder.Append(System.String.Format("-o \"{0}\" ", options.OutputFilePath));
                        // TODO: this needs more work, re: revisions
                        // see http://tldp.org/HOWTO/Program-Library-HOWTO/shared-libraries.html
                        // see http://www.adp-gmbh.ch/cpp/gcc/create_lib.html
                        commandLineBuilder.Append(System.String.Format("-Wl,-soname,\"{0}\" ", options.OutputFilePath));
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void DebugSymbolsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Append("-g ");
            }
        }

        private static void IgnoreStandardLibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Append("-nostdlib ");
            }
        }

        private static void DynamicLibraryCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                commandLineBuilder.Append("-shared ");
            }
        }

        private static void LibraryPathsCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string includePath in includePathsOption.Value)
            {
                commandLineBuilder.AppendFormat("-L\"{0}\" ", includePath);
            }
        }

        private static void StandardLibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            if (options.IgnoreStandardLibraries)
            {
                Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> includePathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
                foreach (string includePath in includePathsOption.Value)
                {
                    commandLineBuilder.AppendFormat("\"{0}\" ", includePath);
                }
            }
            commandLineBuilder.Append("-Wl,--end-group ");
        }

        private static void LibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            commandLineBuilder.Append("-Wl,--start-group ");
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.FileCollection>;
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                commandLineBuilder.AppendFormat("\"{0}\" ", libraryPath);
            }
        }

        protected static void GenerateMapFileSetHandler(object sender, Opus.Core.Option option)
        {
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                string mapPathName = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".map";
                options.MapFilePath = mapPathName;
            }
            else
            {
                options.MapFilePath = null;
            }
        }

        private static void GenerateMapFileCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                commandLineBuilder.AppendFormat("-Wl,-Map,\"{0}\" ", options.MapFilePath);
            }
        }

        private static void SixtyFourBitCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Append("-m64 ");
            }
            else
            {
                commandLineBuilder.Append("-m32 ");
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.OutputFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.OutputFilePath), false);
            }
            if (null != this.StaticImportLibraryFilePath)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(this.StaticImportLibraryFilePath), false);
            }

            return directoriesToCreate;
        }
    }
}
