// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions
    {
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            // common linker options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine);
            this["SubSystem"].PrivateData = new PrivateData(null);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLine);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLine);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLine);
            this["StandardLibraries"].PrivateData = new PrivateData(null);
            this["Libraries"].PrivateData = new PrivateData(null);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLine);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLine);

            // linker specific options
            this["64bit"].PrivateData = new PrivateData(SixtyFourBitCommandLine);
            this["CanUseOrigin"].PrivateData = new PrivateData(CanUseOriginCL);
            this["RPath"].PrivateData = new PrivateData(RPathCL);
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;

            this["64bit"] = new Opus.Core.ValueTypeOption<bool>(Opus.Core.OSUtilities.Is64Bit(target.Platform));

            this.DoNotAutoIncludeStandardLibraries = false; // TODO: fix this - requires a bunch of stuff to be added to the command line

            this.CanUseOrigin = false;
            this.RPaths = new Opus.Core.StringArray();

            /*
             This is an example link line using gcc with -v

Linker Error: ' C:/MinGW/bin/../libexec/gcc/mingw32/3.4.5/collect2.exe -Bdynamic -o d:\build\Test2-dev\Application\win32-debug-mingw\Application.exe C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../crt2.o C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtbegin.o -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5 -LC:/MinGW/bin/../lib/gcc -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../../mingw32/lib -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../.. --subsystem console d:\build\Test2-dev\Application\win32-debug-mingw\application.o d:\build\Test2-dev\Library\win32-debug-mingw\libLibrary.a d:\build\Test3-dev\Library2\win32-debug-mingw\libLibrary2.a -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt -luser32 -lkernel32 -ladvapi32 -lshell32 -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtend.o'
             */
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
                        string extension = null;
                        if (Opus.Core.OSUtilities.IsUnixHosting)
                        {
                            extension = ".so";
                        }
                        else if (Opus.Core.OSUtilities.IsOSXHosting)
                        {
                            extension = ".dylib";
                        }
                        string dynamicLibraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, "lib" + options.OutputName + extension);
                        string importLibraryPathname = dynamicLibraryPathname;
                        options.OutputFilePath = dynamicLibraryPathname;
                        options.StaticImportLibraryFilePath = importLibraryPathname;
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void OutputTypeCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                    if (options.OutputFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-o \"{0}\"", options.OutputFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-o {0}", options.OutputFilePath));
                    }
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    {
                        if (options.OutputFilePath.Contains(" "))
                        {
                            commandLineBuilder.Add(System.String.Format("-o \"{0}\"", options.OutputFilePath));
                        }
                        else
                        {
                            commandLineBuilder.Add(System.String.Format("-o {0}", options.OutputFilePath));
                        }
                        // TODO: this needs more work, re: revisions
                        // see http://tldp.org/HOWTO/Program-Library-HOWTO/shared-libraries.html
                        // see http://www.adp-gmbh.ch/cpp/gcc/create_lib.html
                        // see http://lists.apple.com/archives/unix-porting/2003/Oct/msg00032.html
                        if (Opus.Core.OSUtilities.IsUnixHosting)
                        {
                            if (options.OutputFilePath.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-soname,\"{0}\"", options.OutputFilePath));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-soname,{0}", options.OutputFilePath));
                            }
                        }
                        else if (Opus.Core.OSUtilities.IsOSXHosting)
                        {
                            if (options.OutputFilePath.Contains(" "))
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-dylib_install_name,\"{0}\"", options.OutputFilePath));
                            }
                            else
                            {
                                commandLineBuilder.Add(System.String.Format("-Wl,-dylib_install_name,{0}", options.OutputFilePath));
                            }
                        }
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }

        private static void DebugSymbolsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }

        private static void DoNotAutoIncludeStandardLibrariesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
            }
        }

        private static void DynamicLibraryCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    commandLineBuilder.Add("-shared");
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    commandLineBuilder.Add("-dynamiclib");
                }
            }
        }

        private static void LibraryPathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-L\"{0}\"", libraryPath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-L{0}", libraryPath));
                }
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

        private static void GenerateMapFileCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    if (options.MapFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-Map,\"{0}\"", options.MapFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-Map,{0}", options.MapFilePath));
                    }
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    if (options.MapFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-map,\"{0}\"", options.MapFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,-map,{0}", options.MapFilePath));
                    }
                }
            }
        }

        private static void SixtyFourBitCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }

        private static void CanUseOriginCL(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-z origin");
            }
        }

        private static void RPathCL(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.StringArray> stringsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.StringArray>;
            foreach (string rpath in stringsOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-Wl,rpath,", rpath));
            }
        }

        private static void AdditionalOptionsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
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
