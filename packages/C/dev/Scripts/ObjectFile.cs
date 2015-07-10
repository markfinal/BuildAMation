#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace C
{
namespace V2
{
    public interface ICompilationPolicy
    {
        void
        Compile(
            ObjectFile sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString objectFilePath,
            Bam.Core.V2.Module source);
    }

    public interface ILibrarianPolicy
    {
        void
        Archive(
            StaticLibrary sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> inputs);
    }

    public interface ILinkerPolicy
    {
        void
        Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks);
    }

    // TODO: register a tooltype, e.g. compiler, linker, archiver

    public abstract class CompilerTool :
        Bam.Core.V2.Tool
    {
        // TODO: is this needed?
        public virtual void
        CompileAsShared(
            Bam.Core.V2.Settings settings)
        {}
    }

    public abstract class LibrarianTool :
        Bam.Core.V2.Tool
    { }

    public abstract class LinkerTool :
        Bam.Core.V2.Tool
    {
        public abstract bool UseLPrefixLibraryPaths
        {
            get;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
    public abstract class ToolRegistration :
        System.Attribute
    {
        protected ToolRegistration(
            string toolsetName,
            Bam.Core.EPlatform platform,
            EBit bitDepth = EBit.SixtyFour) // TODO: remove default when all tools go this way
        {
            this.ToolsetName = toolsetName;
            this.Platform = platform;
            this.BitDepth = bitDepth;
        }

        public string ToolsetName
        {
            get;
            private set;
        }

        public Bam.Core.EPlatform Platform
        {
            get;
            private set;
        }

        public EBit BitDepth
        {
            get;
            private set;
        }
    }

    public sealed class RegisterCCompilerAttribute :
        ToolRegistration
    {
        public RegisterCCompilerAttribute(
            string toolsetName,
            Bam.Core.EPlatform platform)
            :
            base(toolsetName, platform)
        {
        }
    }

    public sealed class RegisterCxxCompilerAttribute :
        ToolRegistration
    {
        public RegisterCxxCompilerAttribute(
            string toolsetName,
            Bam.Core.EPlatform platform,
            EBit bitDepth)
            :
            base(toolsetName, platform, bitDepth)
        {
        }
    }

    public sealed class RegisterArchiverAttribute :
        ToolRegistration
    {
        public RegisterArchiverAttribute(
            string toolsetName,
            Bam.Core.EPlatform platform)
            :
            base(toolsetName, platform)
        {
        }
    }

    public sealed class RegisterCLinkerAttribute :
        ToolRegistration
    {
        public RegisterCLinkerAttribute(
            string toolsetName,
            Bam.Core.EPlatform platform,
            EBit bitDepth)
            :
            base(toolsetName, platform, bitDepth)
        {
        }
    }

    public sealed class RegisterCxxLinkerAttribute :
        ToolRegistration
    {
        public RegisterCxxLinkerAttribute(
            string toolsetName,
            Bam.Core.EPlatform platform,
            EBit bitDepth)
            :
            base(toolsetName, platform, bitDepth)
        {
        }
    }

    public static class DefaultToolchain
    {
        public class DefaultToolchainCommand :
            Bam.Core.V2.IStringCommandLineArgument
        {
            string Bam.Core.V2.ICommandLineArgument.LongName
            {
                get
                {
                    return "C.toolchain";
                }
            }

            string Bam.Core.V2.ICommandLineArgument.ShortName
            {
                get
                {
                    return null;
                }
            }
        }

        private static System.Collections.Generic.List<CompilerTool> C_Compilers = new System.Collections.Generic.List<CompilerTool>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>> Cxx_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>();
        private static System.Collections.Generic.List<LibrarianTool> Archivers = new System.Collections.Generic.List<LibrarianTool>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LinkerTool>> C_Linkers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LinkerTool>>();
        private static System.Collections.Generic.List<LinkerTool> Cxx_Linkers = new System.Collections.Generic.List<LinkerTool>();
        private static string DefaultToolChain = null;

        private static System.Collections.Generic.IEnumerable<System.Tuple<System.Type,T>> GetTools<T>() where T : ToolRegistration
        {
            foreach (var type in Bam.Core.State.ScriptAssembly.GetTypes())
            {
                var tools = type.GetCustomAttributes(typeof(T), false) as T[];
                if (tools.Length > 0)
                {
                    if (Bam.Core.OSUtilities.CurrentOS == tools[0].Platform)
                    {
                        yield return new System.Tuple<System.Type, T>(type, tools[0]);
                    }
                }
            }
        }

        static DefaultToolchain()
        {
            DefaultToolChain = Bam.Core.V2.CommandLineProcessor.Evaluate(new DefaultToolchainCommand());

            var graph = Bam.Core.V2.Graph.Instance;
            foreach (var toolData in GetTools<RegisterCCompilerAttribute>())
            {
                C_Compilers.Add(graph.MakeModuleOfType<CompilerTool>(toolData.Item1));
            }
            foreach (var toolData in GetTools<RegisterCxxCompilerAttribute>())
            {
                var tool = graph.MakeModuleOfType<CompilerTool>(toolData.Item1);
                var bits = toolData.Item2.BitDepth;
                if (!C_Linkers.ContainsKey(bits))
                {
                    Cxx_Compilers[bits] = new Bam.Core.Array<CompilerTool>(tool);
                }
                else
                {
                    Cxx_Compilers[bits].AddUnique(tool);
                }
            }
            foreach (var toolData in GetTools<RegisterArchiverAttribute>())
            {
                Archivers.Add(graph.MakeModuleOfType<LibrarianTool>(toolData.Item1));
            }
            foreach (var toolData in GetTools<RegisterCLinkerAttribute>())
            {
                var tool = graph.MakeModuleOfType<LinkerTool>(toolData.Item1);
                var bits = toolData.Item2.BitDepth;
                if (!C_Linkers.ContainsKey(bits))
                {
                    C_Linkers[bits] = new Bam.Core.Array<LinkerTool>(tool);
                }
                else
                {
                    C_Linkers[bits].AddUnique(tool);
                }
            }
            foreach (var toolData in GetTools<RegisterCxxLinkerAttribute>())
            {
                Cxx_Linkers.Add(graph.MakeModuleOfType<LinkerTool>(toolData.Item1));
            }
        }

        public static CompilerTool C_Compiler
        {
            get
            {
                if (0 == C_Compilers.Count)
                {
                    throw new Bam.Core.Exception("No default C compilers for this platform");
                }
                if (C_Compilers.Count > 1)
                {
                    if (null != DefaultToolChain)
                    {
                        foreach (var tool in C_Compilers)
                        {
                            var attr = tool.GetType().GetCustomAttributes(false);
                            if ((attr[0] as ToolRegistration).ToolsetName == DefaultToolChain)
                            {
                                return tool;
                            }
                        }
                    }

                    var tooManyCompilers = new System.Text.StringBuilder();
                    tooManyCompilers.AppendFormat("There are {0} possible C compilers for this platform", C_Compilers.Count);
                    tooManyCompilers.AppendLine();
                    foreach (var compiler in C_Compilers)
                    {
                        tooManyCompilers.AppendLine(compiler.Name);
                    }
                    throw new Bam.Core.Exception(tooManyCompilers.ToString());
                }
                var compilerTool = C_Compilers[0];
                var compilerToolset = (compilerTool.GetType().GetCustomAttributes(false)[0] as ToolRegistration).ToolsetName;
                if (compilerToolset != DefaultToolChain)
                {
                    throw new Bam.Core.Exception("C compiler is from toolchain {0}, not the toolchain requested {1}", compilerToolset, DefaultToolChain);
                }
                return compilerTool;
            }
        }

        public static CompilerTool Cxx_Compiler(EBit bitDepth)
        {
            if (!Cxx_Compilers.ContainsKey(bitDepth) || 0 == Cxx_Compilers[bitDepth].Count)
            {
                throw new Bam.Core.Exception("No default C++ compilers for this platform in {0} bits", (int)bitDepth);
            }
            var candidates = Cxx_Compilers[bitDepth];
            if (candidates.Count > 1)
            {
                if (null != DefaultToolChain)
                {
                    foreach (var tool in candidates)
                    {
                        var attr = tool.GetType().GetCustomAttributes(false);
                        if ((attr[0] as ToolRegistration).ToolsetName == DefaultToolChain)
                        {
                            return tool;
                        }
                    }
                }

                var tooManyCompilers = new System.Text.StringBuilder();
                tooManyCompilers.AppendFormat("There are {0} possible C++ compilers for this platform in {0} bits", candidates.Count, bitDepth);
                tooManyCompilers.AppendLine();
                foreach (var compiler in candidates)
                {
                    tooManyCompilers.AppendLine(compiler.Name);
                }
                throw new Bam.Core.Exception(tooManyCompilers.ToString());
            }
            var compilerTool = candidates[0];
            var compilerToolset = (compilerTool.GetType().GetCustomAttributes(false)[0] as ToolRegistration).ToolsetName;
            if (compilerToolset != DefaultToolChain)
            {
                throw new Bam.Core.Exception("C++ compiler is from toolchain {0}, not the toolchain requested {1}", compilerToolset, DefaultToolChain);
            }
            return compilerTool;
        }

        public static LibrarianTool Librarian
        {
            get
            {
                if (0 == Archivers.Count)
                {
                    throw new Bam.Core.Exception("No default librarians for this platform");
                }
                if (Archivers.Count > 1)
                {
                    if (null != DefaultToolChain)
                    {
                        foreach (var tool in Archivers)
                        {
                            var attr = tool.GetType().GetCustomAttributes(false);
                            if ((attr[0] as ToolRegistration).ToolsetName == DefaultToolChain)
                            {
                                return tool;
                            }
                        }
                    }
                    throw new Bam.Core.Exception("There are {0} possible librarians for this platform", Archivers.Count);
                }
                var archiverTool = Archivers[0];
                var archiverToolset = (archiverTool.GetType().GetCustomAttributes(false)[0] as ToolRegistration).ToolsetName;
                if (archiverToolset != DefaultToolChain)
                {
                    throw new Bam.Core.Exception("Librarian is from toolchain {0}, not the toolchain requested {1}", archiverToolset, DefaultToolChain);
                }
                return archiverTool;
            }
        }

        public static LinkerTool C_Linker(EBit bitDepth)
        {
            if (!C_Linkers.ContainsKey(bitDepth) || 0 == C_Linkers[bitDepth].Count)
            {
                throw new Bam.Core.Exception("No default C linkers for this platform in {0} bits", (int)bitDepth);
            }
            var candidates = C_Linkers[bitDepth];
            if (candidates.Count > 1)
            {
                if (null != DefaultToolChain)
                {
                    foreach (var tool in candidates)
                    {
                        var attr = tool.GetType().GetCustomAttributes(false);
                        if ((attr[0] as ToolRegistration).ToolsetName == DefaultToolChain)
                        {
                            return tool;
                        }
                    }
                }
                throw new Bam.Core.Exception("There are {0} possible C linkers for this platform in {0} bits", C_Linkers.Count, (int)bitDepth);
            }
            var linkerTool = candidates[0];
            var linkerToolset = (linkerTool.GetType().GetCustomAttributes(false)[0] as ToolRegistration).ToolsetName;
            if (linkerToolset != DefaultToolChain)
            {
                throw new Bam.Core.Exception("C linker is from toolchain {0}, not the toolchain requested {1}", linkerToolset, DefaultToolChain);
            }
            return linkerTool;
        }

        public static LinkerTool Cxx_Linker
        {
            get
            {
                if (0 == Cxx_Linkers.Count)
                {
                    throw new Bam.Core.Exception("No default C++ linkers for this platform");
                }
                if (Cxx_Linkers.Count > 1)
                {
                    if (null != DefaultToolChain)
                    {
                        foreach (var tool in Cxx_Linkers)
                        {
                            var attr = tool.GetType().GetCustomAttributes(false);
                            if ((attr[0] as ToolRegistration).ToolsetName == DefaultToolChain)
                            {
                                return tool;
                            }
                        }
                    }
                    throw new Bam.Core.Exception("There are {0} possible C++ linkers for this platform", Cxx_Linkers.Count);
                }
                var linkerTool = Cxx_Linkers[0];
                var linkerToolset = (linkerTool.GetType().GetCustomAttributes(false)[0] as ToolRegistration).ToolsetName;
                if (linkerToolset != DefaultToolChain)
                {
                    throw new Bam.Core.Exception("C++ linker is from toolchain {0}, not the toolchain requested {1}", linkerToolset, DefaultToolChain);
                }
                return linkerTool;
            }
        }
    }

    public class SourceFile :
        Bam.Core.V2.Module,
        Bam.Core.V2.IInputPath
    {
        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Source File");

        public override void Evaluate()
        {
            // TODO: could do a hash check of the contents?
            this.IsUpToDate = true;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            // TODO: exception to this is generated source, but there ought to be an override for that
            throw new Bam.Core.Exception("Source files should not require building");
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // there is no execution policy
        }

        public Bam.Core.V2.TokenizedString InputPath
        {
            get
            {
                return this.GeneratedPaths[Key];
            }
            set
            {
                this.GeneratedPaths[Key] = value;
            }
        }
    }

    public class ObjectFile :
        CModule,
        Bam.Core.V2.IChildModule,
        Bam.Core.V2.IInputPath
    {
        private Bam.Core.V2.Module Parent = null;
        private ICompilationPolicy Policy = null;
        private SourceFile Source = null;

        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Compiled Object File");

        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Compiler = DefaultToolchain.C_Compiler;
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/@basename($(inputpath))$(objext)", this));
        }

        public Bam.Core.V2.TokenizedString InputPath
        {
            get
            {
                return this.Source.InputPath;
            }
            set
            {
                if (null == this.Source)
                {
                    // this cannot be a referenced module, since there will be more than one object
                    // of this type (generally)
                    // but this does mean there may be many instances of this 'type' of module
                    // and for multi-configuration builds there may be many instances of the same path
                    this.Source = Bam.Core.V2.Module.Create<SourceFile>();
                    this.DependsOn(this.Source);
                }
                this.Source.InputPath = value;
                this.Macros.Add("inputpath", value);
            }
        }

        Bam.Core.V2.Module Bam.Core.V2.IChildModule.Parent
        {
            get
            {
                return this.Parent;
            }
            set
            {
                this.Parent = value;
            }
        }

        public CompilerTool Compiler
        {
            get
            {
                return this.Tool as CompilerTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            var sourceFile = this.Source;
            var objectFile = this.GeneratedPaths[Key];
            this.Policy.Compile(this, context, objectFile, sourceFile);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "C.V2." + mode + "Compilation";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<ICompilationPolicy>.Create(className);
        }

        public override void Evaluate()
        {
            var exists = System.IO.File.Exists(this.GeneratedPaths[Key].ToString());
            if (!exists)
            {
                return;
            }
            var sourceWriteTime = System.IO.File.GetLastWriteTime(this.InputPath.ToString());
            var outputWriteTime = System.IO.File.GetLastWriteTime(this.GeneratedPaths[Key].ToString());
            if (outputWriteTime < sourceWriteTime)
            {
                return;
            }
            this.IsUpToDate = true;
        }
    }
}
    /// <summary>
    /// C object file
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFile :
        Bam.Core.BaseModule
    {
        private static readonly Bam.Core.LocationKey SourceFile = new Bam.Core.LocationKey("SourceFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("ObjectFileDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("ObjectFile", Bam.Core.ScaffoldLocation.ETypeHint.File);

        public Bam.Core.Location SourceFileLocation
        {
            get
            {
                return this.Locations[SourceFile];
            }

            set
            {
                this.Locations[SourceFile] = value;
            }
        }

        public void
        Include(
            Bam.Core.Location baseLocation,
            string pattern)
        {
            this.SourceFileLocation = new Bam.Core.ScaffoldLocation(baseLocation, pattern, Bam.Core.ScaffoldLocation.ETypeHint.File);
        }
    }
}
