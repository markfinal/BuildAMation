#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core.V2;

[assembly: Bam.Core.RegisterToolset("Publish", typeof(Publisher.Toolset))]

namespace Publisher
{
namespace V2
{
    public static class DefaultExtensions
    {
        public static void
        Defaults(
            this ICopyFileSettings settings,
            Bam.Core.V2.Module module)
        {
            settings.Force = true;
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this ICopyFileSettings settings,
            Module module,
            Bam.Core.StringArray commandLine)
        {
            if (settings.Force)
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    commandLine.Add("/Y");
                }
                else
                {
                    commandLine.Add("-f");
                }
            }
        }
    }

    public abstract class CopyFileTool :
        Bam.Core.V2.PreBuiltTool
    {
        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(DefaultExtensions))]
    public interface ICopyFileSettings :
        ISettingsBase
    {
        bool Force
        {
            get;
            set;
        }
    }

    public sealed class CopyFileSettings :
        Bam.Core.V2.Settings,
        ICopyFileSettings,
        CommandLineProcessor.V2.IConvertToCommandLine
    {
        public CopyFileSettings()
        {}

        public CopyFileSettings(
            Bam.Core.V2.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        bool ICopyFileSettings.Force
        {
            get;
            set;
        }

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Module module,
            Bam.Core.StringArray commandLine)
        {
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                commandLine.Add("/C");
                commandLine.Add("copy");
            }
            else
            {
                commandLine.Add("-v");
            }
            (this as ICopyFileSettings).Convert(module, commandLine);
        }
    }

    public sealed class CopyFilePosix :
        CopyFileTool
    {
        public override Bam.Core.V2.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new CopyFileSettings(module);
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create("cp", null, verbatim: true);
            }
        }
    }

    public sealed class CopyFileWin :
        CopyFileTool
    {
        public override Bam.Core.V2.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new CopyFileSettings(module);
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create("cmd", null, verbatim: true);
            }
        }
    }

    public sealed class PackageReference
    {
        public PackageReference(
            Bam.Core.V2.Module module,
            string subdirectory,
            Bam.Core.Array<PackageReference> references)
        {
            this.Module = module;
            this.SubDirectory = subdirectory;
            this.References = references;
        }

        public bool IsMarker
        {
            get
            {
                return (null == this.References);
            }
        }

        public Bam.Core.V2.Module Module
        {
            get;
            private set;
        }

        public string SubDirectory
        {
            get;
            private set;
        }

        public Bam.Core.Array<PackageReference> References
        {
            get;
            private set;
        }

        public string DestinationDir
        {
            get;
            set;
        }
    }

    public interface IPackagePolicy
    {
        void
        Package(
            Package sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, PackageReference>> packageObjects);
    }

    public abstract class Package :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, PackageReference>> dependents = new System.Collections.Generic.Dictionary<Module, System.Collections.Generic.Dictionary<TokenizedString, PackageReference>>();
        private IPackagePolicy Policy = null;
        public static Bam.Core.V2.FileKey PackageRoot = Bam.Core.V2.FileKey.Generate("Package Root");

        protected Package()
        {
            this.RegisterGeneratedFile(PackageRoot, Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)-$(config)", this));
        }

        protected override void Init(Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Tool = Bam.Core.V2.Graph.Instance.FindReferencedModule<CopyFileWin>();
            }
            else
            {
                this.Tool = Bam.Core.V2.Graph.Instance.FindReferencedModule<CopyFilePosix>();
            }
        }

        public enum EPublishingType
        {
            ConsoleApplication,
            WindowedApplication
        }

        private string
        PublishingPath(
            Bam.Core.V2.Module module,
            EPublishingType type)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                switch (type)
                {
                case EPublishingType.ConsoleApplication:
                    return null;

                case EPublishingType.WindowedApplication:
                    return Bam.Core.V2.TokenizedString.Create("$(OutputName).app/Contents/MacOS", module).Parse();

                default:
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public PackageReference
        Include<DependentModule>(
            Bam.Core.V2.FileKey key,
            EPublishingType type,
            string subdir = null) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }
            this.Requires(dependent);
            if (!this.dependents.ContainsKey(dependent))
            {
                this.dependents.Add(dependent, new System.Collections.Generic.Dictionary<TokenizedString, PackageReference>());
            }
            var path = this.PublishingPath(dependent, type);
            string destSubDir;
            if (null == path)
            {
                destSubDir = subdir;
            }
            else
            {
                if (null != subdir)
                {
                    destSubDir = System.IO.Path.Combine(path, subdir);
                }
                else
                {
                    destSubDir = path;
                }
            }
            var packaging = new PackageReference(dependent, destSubDir, null);
            this.dependents[dependent].Add(dependent.GeneratedPaths[key], packaging);
            return packaging;
        }

        public void
        Include<DependentModule>(
            Bam.Core.V2.FileKey key,
            string subdir,
            PackageReference reference,
            params PackageReference[] additionalReferences) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
            if (!this.dependents.ContainsKey(dependent))
            {
                this.dependents.Add(dependent, new System.Collections.Generic.Dictionary<TokenizedString, PackageReference>());
            }
            var refs = new Bam.Core.Array<PackageReference>(reference);
            refs.AddRangeUnique(new Bam.Core.Array<PackageReference>(additionalReferences));
            var packaging = new PackageReference(dependent, subdir, refs);
            this.dependents[dependent].Add(dependent.GeneratedPaths[key], packaging);
        }

        public void
        IncludeFiles<DependentModule>(
            string parameterizedFilePath,
            string subdir,
            PackageReference reference,
            params PackageReference[] additionalReferences) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
            if (!this.dependents.ContainsKey(dependent))
            {
                this.dependents.Add(dependent, new System.Collections.Generic.Dictionary<TokenizedString, PackageReference>());
            }
            var refs = new Bam.Core.Array<PackageReference>(reference);
            refs.AddRangeUnique(new Bam.Core.Array<PackageReference>(additionalReferences));
            var tokenString = Bam.Core.V2.TokenizedString.Create(parameterizedFilePath, dependent);
            var packaging = new PackageReference(dependent, subdir, refs);
            this.dependents[dependent].Add(tokenString, packaging);
        }

        public override void
        Evaluate()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            // TODO: the nested dictionary is not readonly - not sure how to construct this
            var packageObjects = new System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.Module, System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, PackageReference>>(this.dependents);
            this.Policy.Package(this, context, this.GeneratedPaths[PackageRoot], packageObjects);
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
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();

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

        public void
        AddPath(
            Bam.Core.V2.Module module,
            Bam.Core.V2.FileKey key)
        {
            this.DependsOn(module);
            this.Paths.Add(module, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
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
                scriptWriter.WriteLine("Uninstallable=No");
                scriptWriter.WriteLine("[Files]");
                foreach (var dep in this.Files)
                {
                    scriptWriter.Write(System.String.Format("Source: \"{0}\"; ", dep.Key.GeneratedPaths[dep.Value]));
                    scriptWriter.Write("DestDir: \"{app}\"; ");
                    scriptWriter.Write("DestName: \"Test\"");
                }
                foreach (var dep in this.Paths)
                {
                    scriptWriter.Write(System.String.Format("Source: \"{0}\\*.*\"; ", dep.Key.GeneratedPaths[dep.Value]));
                    scriptWriter.Write("DestDir: \"{app}\"; ");
                    scriptWriter.Write("Flags: recursesubdirs");
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
        Bam.Core.V2.PreBuiltTool
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

    public interface IInnoSetupPolicy
    {
        void CreateInstaller(
            InnoSetupInstaller sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool compiler,
            Bam.Core.V2.TokenizedString scriptPath);
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    public abstract class InnoSetupInstaller :
        Bam.Core.V2.Module
    {
        private InnoSetupScript ScriptModule;
        private Bam.Core.V2.PreBuiltTool Compiler;
        private IInnoSetupPolicy Policy;

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

        public void
        SourceFolder<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.ScriptModule.AddPath(dependent, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateInstaller(this, context, this.Compiler, this.ScriptModule.ScriptPath);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Publisher.V2." + mode + "InnoSetup";
                this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<IInnoSetupPolicy>.Create(className);
            }
        }
    }
}
namespace V2
{
    class NSISScript :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();

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

        public void
        AddPath(
            Bam.Core.V2.Module module,
            Bam.Core.V2.FileKey key)
        {
            this.DependsOn(module);
            this.Paths.Add(module, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
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
                foreach (var dep in this.Paths)
                {
                    scriptWriter.WriteLine("\tSetOutPath $INSTDIR");
                    scriptWriter.WriteLine("\tFile /r {0}\\*.*", dep.Key.GeneratedPaths[dep.Value]);
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
        Bam.Core.V2.PreBuiltTool
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

    public interface INSISPolicy
    {
        void CreateInstaller(
            NSISInstaller sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool compiler,
            Bam.Core.V2.TokenizedString scriptPath);
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    public abstract class NSISInstaller :
        Bam.Core.V2.Module
    {
        private NSISScript ScriptModule;
        private Bam.Core.V2.PreBuiltTool Compiler;
        private INSISPolicy Policy;

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

        public void
        SourceFolder<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.ScriptModule.AddPath(dependent, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateInstaller(this, context, this.Compiler, this.ScriptModule.ScriptPath);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Publisher.V2." + mode + "NSIS";
                this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<INSISPolicy>.Create(className);
            }
        }
    }
}
namespace V2
{
    class TarInputFiles :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();

        public TarInputFiles()
        {
            this.ScriptPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)/tarinput.txt", this);
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

        public void
        AddPath(
            Bam.Core.V2.Module module,
            Bam.Core.V2.FileKey key)
        {
            this.DependsOn(module);
            this.Paths.Add(module, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            var path = this.ScriptPath.Parse();
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            using (var scriptWriter = new System.IO.StreamWriter(path))
            {
                foreach (var dep in this.Files)
                {
                    var filePath = dep.Key.GeneratedPaths[dep.Value].ToString();
                    var fileDir = System.IO.Path.GetDirectoryName(filePath);
                    // TODO: this should probably be a setting
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                    {
                        scriptWriter.WriteLine("-C");
                        scriptWriter.WriteLine(fileDir);
                    }
                    else
                    {
                        scriptWriter.WriteLine("-C {0}", fileDir);
                    }
                    scriptWriter.WriteLine(System.IO.Path.GetFileName(filePath));
                }
                foreach (var dep in this.Paths)
                {
                    var fileDir = dep.Key.GeneratedPaths[dep.Value].ToString();
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                    {
                        scriptWriter.WriteLine("-C");
                        scriptWriter.WriteLine(fileDir);
                    }
                    else
                    {
                        scriptWriter.WriteLine("-C {0}", fileDir);
                    }
                    scriptWriter.WriteLine(".");
                }
            }
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    public sealed class TarSettings :
        Bam.Core.V2.Settings
    {
    }

    public sealed class TarCompiler :
        Bam.Core.V2.PreBuiltTool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new TarSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create("tar", null);
            }
        }
    }

    public interface ITarPolicy
    {
        void CreateTarBall(
            TarBall sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool compiler,
            Bam.Core.V2.TokenizedString scriptPath,
            Bam.Core.V2.TokenizedString outputPath);
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Unix | Bam.Core.EPlatform.OSX)]
    public abstract class TarBall :
        Bam.Core.V2.Module
    {
        public static Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Installer");

        private TarInputFiles InputFiles;
        private Bam.Core.V2.PreBuiltTool Compiler;
        private ITarPolicy Policy;

        public TarBall()
        {
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(buildroot)/installer.tar", this));

            // TODO: this actually needs to be a new class each time, otherwise multiple installers won't work
            // need to find a way to instantiate a non-abstract instance of an abstract class
            // looks like emit is needed
            this.InputFiles = Bam.Core.V2.Graph.Instance.FindReferencedModule<TarInputFiles>();
            this.DependsOn(this.InputFiles);

            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<TarCompiler>();
            this.Requires(this.Compiler);
        }

        public void
        Include<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.InputFiles.AddFile(dependent, key);
        }

        public void
        SourceFolder<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.InputFiles.AddPath(dependent, key);
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateTarBall(this, context, this.Compiler, this.InputFiles.ScriptPath, this.GeneratedPaths[Key]);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Publisher.V2." + mode + "TarBall";
                this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<ITarPolicy>.Create(className);
            }
        }
    }
}
namespace V2
{
    public sealed class DiskImageSettings :
        Bam.Core.V2.Settings
    {
    }

    public sealed class DiskImageCompiler :
        Bam.Core.V2.PreBuiltTool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            return new TarSettings();
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return Bam.Core.V2.TokenizedString.Create("hdiutil", null);
            }
        }
    }

    public interface IDiskImagePolicy
    {
        void CreateDMG(
            DiskImage sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool compiler,
            Bam.Core.V2.TokenizedString scriptPath,
            Bam.Core.V2.TokenizedString outputPath);
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.OSX)]
    public abstract class DiskImage :
        Bam.Core.V2.Module
    {
        public static Bam.Core.V2.FileKey Key = Bam.Core.V2.FileKey.Generate("Installer");

        private Bam.Core.V2.TokenizedString SourceFolderPath;
        private Bam.Core.V2.PreBuiltTool Compiler;
        private IDiskImagePolicy Policy;

        public DiskImage()
        {
            this.RegisterGeneratedFile(Key, Bam.Core.V2.TokenizedString.Create("$(buildroot)/installer.dmg", this));

            this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<DiskImageCompiler>();
            this.Requires(this.Compiler);
        }

        public void
        SourceFolder<DependentModule>(
            Bam.Core.V2.FileKey key) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.SourceFolderPath = dependent.GeneratedPaths[key];
        }

        public override void Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.V2.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateDMG(this, context, this.Compiler, this.SourceFolderPath, this.GeneratedPaths[Key]);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Publisher.V2." + mode + "DMG";
                this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<IDiskImagePolicy>.Create(className);
            }
        }
    }
}
    // Add modules here
}
