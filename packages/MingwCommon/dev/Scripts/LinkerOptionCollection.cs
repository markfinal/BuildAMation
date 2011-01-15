// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public partial class LinkerOptionCollection : C.LinkerOptionCollection, C.ILinkerOptions, Opus.Core.IOutputPaths
    {
        private enum EOutputFile
        {
            OutputFile = 0,
            StaticImportLibraryFile,
            MapFile
        }

        private System.Collections.Generic.Dictionary<EOutputFile, string> outputFileMap = new System.Collections.Generic.Dictionary<EOutputFile, string>();
        System.Collections.Generic.Dictionary<string, string> Opus.Core.IOutputPaths.GetOutputPaths()
        {
            System.Collections.Generic.Dictionary<string, string> pathMap = new System.Collections.Generic.Dictionary<string, string>();
            foreach (System.Collections.Generic.KeyValuePair<EOutputFile, string> file in this.outputFileMap)
            {
                pathMap.Add(file.Key.ToString(), file.Value);
            }
            return pathMap;
        }

        private void SetDelegates(Opus.Core.Target target)
        {
            // common linker options
            this["ToolchainOptionCollection"].PrivateData = new PrivateData(null);
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLine);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLine);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLine);
            this["IgnoreStandardLibraries"].PrivateData = new PrivateData(IgnoreStandardLibrariesCommandLine);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLine);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLine);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLine);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLine);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLine);

            // linker specific options
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            this.IgnoreStandardLibraries = false; // TODO: fix this - requires a bunch of stuff to be added to the command line

            Opus.Core.Target target = node.Target;

            /*
             This is an example link line using gcc with -v
             
Linker Error: ' C:/MinGW/bin/../libexec/gcc/mingw32/3.4.5/collect2.exe -Bdynamic -o d:\build\Test2-dev\Application\win32-debug-mingw\Application.exe C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../crt2.o C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtbegin.o -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5 -LC:/MinGW/bin/../lib/gcc -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../../../mingw32/lib -LC:/MinGW/bin/../lib/gcc/mingw32/3.4.5/../../.. --subsystem console d:\build\Test2-dev\Application\win32-debug-mingw\application.o d:\build\Test2-dev\Library\win32-debug-mingw\libLibrary.a d:\build\Test3-dev\Library2\win32-debug-mingw\libLibrary2.a -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt -luser32 -lkernel32 -ladvapi32 -lshell32 -lmingw32 -lgcc -lmoldname -lmingwex -lmsvcrt C:/MinGW/bin/../lib/gcc/mingw32/3.4.5/crtend.o'
             */

            SetDelegates(target);
        }

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override string OutputFilePath
        {
            get
            {
                if (this.outputFileMap.ContainsKey(EOutputFile.OutputFile))
                {
                    return this.outputFileMap[EOutputFile.OutputFile];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.outputFileMap[EOutputFile.OutputFile] = value;
                }
                else if (this.outputFileMap.ContainsKey(EOutputFile.OutputFile))
                {
                    this.outputFileMap.Remove(EOutputFile.OutputFile);
                }
            }
        }

        public override string StaticImportLibraryFilePath
        {
            get
            {
                if (this.outputFileMap.ContainsKey(EOutputFile.StaticImportLibraryFile))
                {
                    return this.outputFileMap[EOutputFile.StaticImportLibraryFile];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.outputFileMap[EOutputFile.StaticImportLibraryFile] = value;
                }
                else if (this.outputFileMap.ContainsKey(EOutputFile.StaticImportLibraryFile))
                {
                    this.outputFileMap.Remove(EOutputFile.StaticImportLibraryFile);
                }
            }
        }

        public override string MapFilePath
        {
            get
            {
                if (this.outputFileMap.ContainsKey(EOutputFile.MapFile))
                {
                    return this.outputFileMap[EOutputFile.MapFile];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.outputFileMap[EOutputFile.MapFile] = value;
                }
                else if (this.outputFileMap.ContainsKey(EOutputFile.MapFile))
                {
                    this.outputFileMap.Remove(EOutputFile.MapFile);
                }
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
                        string executablePathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName + ".exe");
                        options.OutputFilePath = executablePathname;
                    }
                    break;

                case C.ELinkerOutput.DynamicLibrary:
                    {
                        string dynamicLibraryPathname = System.IO.Path.Combine(options.OutputDirectoryPath, options.OutputName) + ".dll";
                        string importLibraryPathname = System.IO.Path.Combine(options.LibraryDirectoryPath, "lib" + options.OutputName) + ".a";
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
                case C.ELinkerOutput.DynamicLibrary:
                    commandLineBuilder.Append(System.String.Format("-o \"{0}\" ", options.OutputFilePath));
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

        // TODO: do these actually work?
        private static void SubSystemCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;

                case C.ESubsystem.Console:
                    commandLineBuilder.Append("-Wl,--subsystem,console ");
                    break;

                case C.ESubsystem.Windows:
                    commandLineBuilder.Append("-Wl,--subsystem,windows ");
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
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
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (null != options.StaticImportLibraryFilePath)
                {
                    commandLineBuilder.AppendFormat("-Wl,--out-implib,\"{0}\" ", options.StaticImportLibraryFilePath);
                }
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
            commandLineBuilder.Append("--end-group ");
        }

        private static void LibrariesCommandLine(object sender, System.Text.StringBuilder commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            commandLineBuilder.Append("--start-group ");
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

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.OutputFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.OutputFilePath), false);
            }
            if (null != this.StaticImportLibraryFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.StaticImportLibraryFilePath), false);
            }

            return directoriesToCreate;
        }
    }
}