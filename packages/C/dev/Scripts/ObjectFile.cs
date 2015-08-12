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
    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void Defaults(this C.V2.ICOnlyCompilerOptions settings, Bam.Core.V2.Module module)
            {
            }
        }
    }

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
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> inputs,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers);
    }

    public interface ILinkerPolicy
    {
        void
        Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers,
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
            EBit bitDepth)
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
            Bam.Core.EPlatform platform,
            EBit bitDepth)
            :
            base(toolsetName, platform, bitDepth)
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
            Bam.Core.EPlatform platform,
            EBit bitDepth)
            :
            base(toolsetName, platform, bitDepth)
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

    public sealed class RegisterObjectiveCCompilerAttribute :
        ToolRegistration
    {
        public RegisterObjectiveCCompilerAttribute(
            string toolsetName,
            Bam.Core.EPlatform platform,
            EBit bitDepth)
            :
            base(toolsetName, platform, bitDepth)
        {
        }
    }

    public sealed class RegisterObjectiveCxxCompilerAttribute :
        ToolRegistration
    {
        public RegisterObjectiveCxxCompilerAttribute(
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

        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>  C_Compilers   = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>  Cxx_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LibrarianTool>> Archivers     = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LibrarianTool>>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LinkerTool>>    C_Linkers     = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LinkerTool>>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LinkerTool>>    Cxx_Linkers   = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<LinkerTool>>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>  ObjectiveC_Compilers   = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>  ObjectiveCxx_Compilers   = new System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<CompilerTool>>();
        private static string DefaultToolChain = null;

        private static System.Collections.Generic.IEnumerable<System.Tuple<System.Type,T>>
        GetToolsFromMetaData<T>()
            where T : ToolRegistration
        {
            foreach (var type in Bam.Core.State.ScriptAssembly.GetTypes())
            {
                var tools = type.GetCustomAttributes(typeof(T), false) as T[];
                if (0 == tools.Length)
                {
                    continue;
                }
                foreach (var tool in tools)
                {
                    if (Bam.Core.OSUtilities.CurrentOS != tool.Platform)
                    {
                        continue;
                    }
                    yield return new System.Tuple<System.Type, T>(type, tool);
                }
            }
        }

        private static void
        FindTools<AttributeType, ToolType>(
            System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<ToolType>> collection)
            where AttributeType : ToolRegistration
            where ToolType : Bam.Core.V2.Tool
        {
            var graph = Bam.Core.V2.Graph.Instance;
            foreach (var toolData in GetToolsFromMetaData<AttributeType>())
            {
                var tool = graph.MakeModuleOfType<ToolType>(toolData.Item1);
                var bits = toolData.Item2.BitDepth;
                if (!collection.ContainsKey(bits))
                {
                    collection[bits] = new Bam.Core.Array<ToolType>(tool);
                }
                else
                {
                    collection[bits].AddUnique(tool);
                }
            }
        }

        static DefaultToolchain()
        {
            DefaultToolChain = Bam.Core.V2.CommandLineProcessor.Evaluate(new DefaultToolchainCommand());
            FindTools<RegisterCCompilerAttribute, CompilerTool>(C_Compilers);
            FindTools<RegisterCxxCompilerAttribute, CompilerTool>(Cxx_Compilers);
            FindTools<RegisterArchiverAttribute, LibrarianTool>(Archivers);
            FindTools<RegisterCLinkerAttribute, LinkerTool>(C_Linkers);
            FindTools<RegisterCxxLinkerAttribute, LinkerTool>(Cxx_Linkers);
            FindTools<RegisterObjectiveCCompilerAttribute, CompilerTool>(ObjectiveC_Compilers);
            FindTools<RegisterObjectiveCxxCompilerAttribute, CompilerTool>(ObjectiveCxx_Compilers);
        }

        private static ToolType
        GetTool<ToolType>(
            System.Collections.Generic.Dictionary<EBit, Bam.Core.Array<ToolType>> collection,
            EBit bitDepth,
            string toolDescription)
            where ToolType : Bam.Core.V2.Tool
        {
            if (!collection.ContainsKey(bitDepth) || 0 == collection[bitDepth].Count)
            {
                throw new Bam.Core.Exception("No default {0}s for this platform in {1} bits", toolDescription, (int)bitDepth);
            }
            var candidates = collection[bitDepth];
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

                var tooManyInstance = new System.Text.StringBuilder();
                tooManyInstance.AppendFormat("There are {0} possible {1}s for this platform in {2} bits", candidates.Count, toolDescription, bitDepth);
                tooManyInstance.AppendLine();
                foreach (var tool in candidates)
                {
                    tooManyInstance.AppendLine(tool.Name);
                }
                throw new Bam.Core.Exception(tooManyInstance.ToString());
            }
            var toolToUse = candidates[0];
            var toolToolSet = (toolToUse.GetType().GetCustomAttributes(false)[0] as ToolRegistration).ToolsetName;
            if (toolToolSet != DefaultToolChain)
            {
                throw new Bam.Core.Exception("{0} is from toolchain {1}, not the toolchain requested {2}", toolDescription, toolToolSet, DefaultToolChain);
            }
            return toolToUse;
        }

        public static CompilerTool C_Compiler(EBit bitDepth)
        {
            return GetTool<CompilerTool>(C_Compilers, bitDepth, "C compiler");
        }

        public static CompilerTool Cxx_Compiler(EBit bitDepth)
        {
            return GetTool<CompilerTool>(Cxx_Compilers, bitDepth, "C++ compiler");
        }

        public static LibrarianTool Librarian(EBit bitDepth)
        {
            return GetTool<LibrarianTool>(Archivers, bitDepth, "librarian");
        }

        public static LinkerTool C_Linker(EBit bitDepth)
        {
            return GetTool<LinkerTool>(C_Linkers, bitDepth, "C linker");
        }

        public static LinkerTool Cxx_Linker(EBit bitDepth)
        {
            return GetTool<LinkerTool>(Cxx_Linkers, bitDepth, "C++ linker");
        }

        public static CompilerTool ObjectiveC_Compiler(EBit bitDepth)
        {
            return GetTool<CompilerTool>(ObjectiveC_Compilers, bitDepth, "Objective C compiler");
        }

        public static CompilerTool ObjectiveCxx_Compiler(EBit bitDepth)
        {
            return GetTool<CompilerTool>(ObjectiveCxx_Compilers, bitDepth, "Objective C++ compiler");
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

    public sealed class HeaderFile :
        Bam.Core.V2.Module,
        Bam.Core.V2.IInputPath,
        Bam.Core.V2.IChildModule
    {
        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Header File");

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
            throw new Bam.Core.Exception("Header files should not require building");
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

        Bam.Core.V2.Module Bam.Core.V2.IChildModule.Parent
        {
            get;
            set;
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
            this.Compiler = DefaultToolchain.C_Compiler(this.BitDepth);
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
