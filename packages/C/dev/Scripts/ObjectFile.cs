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
            string objectFilePath,
            Bam.Core.V2.Module source);
    }

    public interface ILibrarianPolicy
    {
        void
        Archive(
            StaticLibrary sender,
            string libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> inputs);
    }

    public interface ILinkerPolicy
    {
        void
        Link(
            ConsoleApplication sender,
            string executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks);
    }

    // TODO: register a tooltype, e.g. compiler, linker, archiver

    public abstract class CompilerTool :
        Bam.Core.V2.Tool
    { }

    public abstract class LibrarianTool :
        Bam.Core.V2.Tool
    { }

    public abstract class LinkerTool :
        Bam.Core.V2.Tool
    { }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public abstract class ToolRegistration :
        System.Attribute
    {
        protected ToolRegistration(Bam.Core.EPlatform platform)
        {
            this.Platform = platform;
        }

        public Bam.Core.EPlatform Platform
        {
            get;
            private set;
        }
    }

    public sealed class RegisterCompilerAttribute :
        ToolRegistration
    {
        public RegisterCompilerAttribute(Bam.Core.EPlatform platform)
            : base(platform)
        {
        }
    }

    public sealed class RegisterArchiverAttribute :
        ToolRegistration
    {
        public RegisterArchiverAttribute(Bam.Core.EPlatform platform)
            : base(platform)
        {
        }
    }

    public sealed class RegisterLinkerAttribute :
        ToolRegistration
    {
        public RegisterLinkerAttribute(Bam.Core.EPlatform platform)
            : base(platform)
        {
        }
    }

    public static class DefaultToolchain
    {
        private static System.Collections.Generic.List<CompilerTool> Compilers = new System.Collections.Generic.List<CompilerTool>();
        private static System.Collections.Generic.List<LibrarianTool> Archivers = new System.Collections.Generic.List<LibrarianTool>();
        private static System.Collections.Generic.List<LinkerTool> Linkers = new System.Collections.Generic.List<LinkerTool>();

        private static System.Collections.Generic.IEnumerable<System.Type> GetTools<T>()
        {
            foreach (var type in Bam.Core.State.ScriptAssembly.GetTypes())
            {
                var compiler = type.GetCustomAttributes(typeof(RegisterCompilerAttribute), false) as RegisterCompilerAttribute[];
                if (compiler.Length > 0)
                {
                    yield return type;
                }
            }
        }

        static DefaultToolchain()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            foreach (var type in GetTools<RegisterCompilerAttribute>())
            {
                Compilers.Add(graph.MakeModuleOfType(type) as CompilerTool);
            }
            foreach (var type in GetTools<RegisterArchiverAttribute>())
            {
                Archivers.Add(graph.MakeModuleOfType(type) as LibrarianTool);
            }
            foreach (var type in GetTools<RegisterLinkerAttribute>())
            {
                Linkers.Add(graph.MakeModuleOfType(type) as LinkerTool);
            }
        }

        public static CompilerTool Compiler
        {
            get
            {
                if (0 == Compilers.Count)
                {
                    throw new Bam.Core.Exception("No default compilers for this platform");
                }
                if (Compilers.Count > 1)
                {
                    throw new Bam.Core.Exception("There are {0} possible compilers for this platform", Compilers.Count);
                }
                return Compilers[0];
            }
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
                    throw new Bam.Core.Exception("There are {0} possible librarians for this platform", Archivers.Count);
                }
                return Archivers[0];
            }
        }

        public static LinkerTool Linker
        {
            get
            {
                if (0 == Linkers.Count)
                {
                    throw new Bam.Core.Exception("No default linkers for this platform");
                }
                if (Linkers.Count > 1)
                {
                    throw new Bam.Core.Exception("There are {0} possible linkers for this platform", Linkers.Count);
                }
                return Linkers[0];
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
            // do nothing
            // TODO: could do a hash check of the contents
        }

        protected override void ExecuteInternal()
        {
            // do nothing
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
        Bam.Core.V2.Module,
        Bam.Core.V2.IChildModule,
        Bam.Core.V2.IInputPath
    {
        private Bam.Core.V2.Module Parent = null;
        private ICompilationPolicy Policy = null;
        private SourceFile Source = null;

        static public Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Compiled Object File");

        protected override void Init()
        {
            this.RegisterGeneratedFile(Key, new Bam.Core.V2.TokenizedString("$(buildroot)/$(modulename)/$(config)/$basename($(inputpath)).obj", null));

            // TODO: this should be a default, and done through a reflection mechanism
            if (null == this.Compiler)
            {
                //this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<VisualC.Compiler64>();
                this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<Mingw.V2.Compiler32>();
                //this.Compiler = DefaultToolchain.Compiler;

                // TODO: this has to be moved later, in case it's changed
                this.UsePublicPatches(this.Compiler);
            }
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
                    // but this does mean there may be duplicates
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

        protected override void ExecuteInternal()
        {
            var sourceFile = this.Source;
            var objectFile = this.GeneratedPaths[Key].ToString();
            this.Policy.Compile(this, objectFile, sourceFile);
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
            if (sourceWriteTime <= outputWriteTime)
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
