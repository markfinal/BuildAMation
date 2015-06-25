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

[assembly: Bam.Core.RegisterToolset("Publish", typeof(Publisher.Toolset))]

namespace Publisher
{
namespace V2
{
    public interface IPackagePolicy
    {
        void
        Package(
            Package sender,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.TokenizedString, string> packageObjects);
    }

    public abstract class Package :
        Bam.Core.V2.Module
    {
        private Bam.Core.Array<Bam.Core.V2.Module> dependents = new Bam.Core.Array<Bam.Core.V2.Module>();
        private System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, string> paths = new System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, string>();
        private IPackagePolicy Policy = null;

        public void
        Include<DependentModule>(
            Bam.Core.V2.FileKey key,
            string subdir) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.Requires(dependent);
            this.dependents.AddUnique(dependent);

            this.paths[dependent.GeneratedPaths[key]] = subdir;
        }

        public void
        IncludeFiles<DependentModule>(
            string parameterizedFilePath,
            string subdir) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.Requires(dependent);
            this.dependents.AddUnique(dependent);

            var tokenString = Bam.Core.V2.TokenizedString.Create(parameterizedFilePath, dependent);
            this.paths[tokenString] = subdir;
        }

        public override void Evaluate()
        {
            // TODO: should this do at least a timestamp check?
            // do nothing
        }

        protected override void ExecuteInternal()
        {
            var paths = new System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.TokenizedString, string>(this.paths);
            var executable = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)", this);
            this.Policy.Package(this, executable, paths);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "Publisher.V2." + mode + "Packager";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<IPackagePolicy>.Create(className);
        }
    }
}
namespace V2
{
    class InnoSetupScript :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();

        public InnoSetupScript()
        {
            this.ScriptPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)/script.iss", this);
        }

        public Bam.Core.V2.TokenizedString ScriptPath
        {
            get;
            private set;
        }

        public void
        AddFile(
            Bam.Core.V2.Module module,
            Bam.Core.V2.FileKey key)
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void ExecuteInternal()
        {
            var path = this.ScriptPath.Parse();
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            using (var scriptWriter = new System.IO.StreamWriter(path))
            {
                scriptWriter.WriteLine("[Setup]");
                scriptWriter.WriteLine("AppName={0}", this.GetType().ToString());
                scriptWriter.WriteLine("AppVersion=1.0");
                scriptWriter.WriteLine("DefaultDirName={{sd}}\\{0}", this.GetType().ToString());
                scriptWriter.WriteLine("ArchitecturesAllowed=x64");
                scriptWriter.WriteLine("ArchitecturesInstallIn64BitMode=x64");
                scriptWriter.WriteLine("[Files]");
                foreach (var dep in this.Files)
                {
                    scriptWriter.Write(System.String.Format("Source: \"{0}\"; ", dep.Key.GeneratedPaths[dep.Value]));
                    scriptWriter.Write("DestDir: \"{app}\"; ");
                    scriptWriter.Write("DestName: \"Test\"");
                }
            }
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    public sealed class InnoSetupCompilerSettings :
        Bam.Core.V2.Settings
    {
    }

    public sealed class InnoSetupCompiler :
        Bam.Core.V2.Tool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new InnoSetupCompilerSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create(@"C:\Program Files (x86)\Inno Setup 5\ISCC.exe", null);
            }
        }
    }

    public abstract class InnoSetupInstaller :
        Bam.Core.V2.Module
    {
        private InnoSetupScript ScriptModule;
        private Bam.Core.V2.Tool Compiler;

        public InnoSetupInstaller()
        {
            // TODO: this actually needs to be a new class each time, otherwise multiple installers won't work
            // need to find a way to instantiate a non-abstract instance of an abstract class
            // looks like emit is needed
            this.ScriptModule = Bam.Core.V2.Graph.Instance.FindReferencedModule<InnoSetupScript>();
            this.DependsOn(this.ScriptModule);

            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<InnoSetupCompiler>();
            this.Requires(this.Compiler);
        }

        public void
        Include<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.ScriptModule.AddFile(dependent, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void ExecuteInternal()
        {
            var args = new Bam.Core.StringArray();
            args.Add(this.ScriptModule.ScriptPath.Parse());
            CommandLineProcessor.V2.Processor.Execute(this.Compiler, args);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }
}
namespace V2
{
    class NSISScript :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();

        public NSISScript()
        {
            this.ScriptPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)/script.nsi", this);
        }

        public Bam.Core.V2.TokenizedString ScriptPath
        {
            get;
            private set;
        }

        public void
        AddFile(
            Bam.Core.V2.Module module,
            Bam.Core.V2.FileKey key)
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void ExecuteInternal()
        {
            var path = this.ScriptPath.Parse();
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            using (var scriptWriter = new System.IO.StreamWriter(path))
            {
                scriptWriter.WriteLine("Name \"{0}\"", this.GetType().ToString());
                scriptWriter.WriteLine("OutFile \"{0}\"", "Installer.exe");
                scriptWriter.WriteLine("InstallDir $PROGRAMFILES64\\{0}", this.GetType().ToString());
                scriptWriter.WriteLine("Page directory");
                scriptWriter.WriteLine("Page instfiles");
                scriptWriter.WriteLine("Section \"\"");
                foreach (var dep in this.Files)
                {
                    scriptWriter.WriteLine("\tSetOutPath $INSTDIR");
                    scriptWriter.WriteLine("\tFile {0}", dep.Key.GeneratedPaths[dep.Value]);
                }
                scriptWriter.WriteLine("SectionEnd");
            }
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    public sealed class NSISCompilerSettings :
        Bam.Core.V2.Settings
    {
    }

    public sealed class NSISCompiler :
        Bam.Core.V2.Tool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new NSISCompilerSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create(@"C:\Program Files (x86)\NSIS\makensis.exe", null);
            }
        }
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    public abstract class NSISInstaller :
        Bam.Core.V2.Module
    {
        private NSISScript ScriptModule;
        private Bam.Core.V2.Tool Compiler;

        public NSISInstaller()
        {
            // TODO: this actually needs to be a new class each time, otherwise multiple installers won't work
            // need to find a way to instantiate a non-abstract instance of an abstract class
            // looks like emit is needed
            this.ScriptModule = Bam.Core.V2.Graph.Instance.FindReferencedModule<NSISScript>();
            this.DependsOn(this.ScriptModule);

            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<NSISCompiler>();
            this.Requires(this.Compiler);
        }

        public void
        Include<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.ScriptModule.AddFile(dependent, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void ExecuteInternal()
        {
            var args = new Bam.Core.StringArray();
            args.Add(this.ScriptModule.ScriptPath.Parse());
            CommandLineProcessor.V2.Processor.Execute(this.Compiler, args);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }
}
    // Add modules here
}
