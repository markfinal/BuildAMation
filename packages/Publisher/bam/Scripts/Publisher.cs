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
using Bam.Core;
namespace Publisher
{
    public static class DefaultExtensions
    {
        public static void
        Defaults(
            this ICopyFileSettings settings,
            Bam.Core.Module module)
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
        Bam.Core.PreBuiltTool
    {
        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }
    }

    [Bam.Core.SettingsExtensions(typeof(DefaultExtensions))]
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
        Bam.Core.Settings,
        ICopyFileSettings,
        CommandLineProcessor.IConvertToCommandLine
    {
        public CopyFileSettings()
        {}

        public CopyFileSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        bool ICopyFileSettings.Force
        {
            get;
            set;
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
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
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new CopyFileSettings(module);
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.Create("cp", null, verbatim: true);
            }
        }
    }

    public sealed class CopyFileWin :
        CopyFileTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new CopyFileSettings(module);
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.Create("cmd", null, verbatim: true);
            }
        }
    }

    public sealed class PackageReference
    {
        public PackageReference(
            Bam.Core.Module module,
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

        public Bam.Core.Module Module
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
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.Module, System.Collections.Generic.Dictionary<Bam.Core.TokenizedString, PackageReference>> packageObjects);
    }

    public abstract class Package :
        Bam.Core.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.Module, System.Collections.Generic.Dictionary<Bam.Core.TokenizedString, PackageReference>> dependents = new System.Collections.Generic.Dictionary<Module, System.Collections.Generic.Dictionary<TokenizedString, PackageReference>>();
        private IPackagePolicy Policy = null;
        public static Bam.Core.FileKey PackageRoot = Bam.Core.FileKey.Generate("Package Root");

        protected Package()
        {
            this.RegisterGeneratedFile(PackageRoot, Bam.Core.TokenizedString.Create("$(buildroot)/$(modulename)-$(config)", this));
        }

        protected override void Init(Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<CopyFileWin>();
            }
            else
            {
                this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<CopyFilePosix>();
            }
        }

        public enum EPublishingType
        {
            ConsoleApplication,
            WindowedApplication
        }

        private string
        PublishingPath(
            Bam.Core.Module module,
            EPublishingType type)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                switch (type)
                {
                case EPublishingType.ConsoleApplication:
                    return null;

                case EPublishingType.WindowedApplication:
                    return Bam.Core.TokenizedString.Create("$(OutputName).app/Contents/MacOS", module).Parse();

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
            Bam.Core.FileKey key,
            EPublishingType type,
            string subdir = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
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
            Bam.Core.FileKey key,
            string subdir,
            PackageReference reference,
            params PackageReference[] additionalReferences) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
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
            params PackageReference[] additionalReferences) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
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
            var tokenString = Bam.Core.TokenizedString.Create(parameterizedFilePath, dependent);
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
            Bam.Core.ExecutionContext context)
        {
            // TODO: the nested dictionary is not readonly - not sure how to construct this
            var packageObjects = new System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.Module, System.Collections.Generic.Dictionary<Bam.Core.TokenizedString, PackageReference>>(this.dependents);
            this.Policy.Package(this, context, this.GeneratedPaths[PackageRoot], packageObjects);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "Publisher." + mode + "Packager";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IPackagePolicy>.Create(className);
        }
    }
}
