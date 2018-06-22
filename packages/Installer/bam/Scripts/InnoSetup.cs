#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace Installer
{
    class InnoSetupScript :
        Bam.Core.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.ScriptPath = this.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)/$(config)/script.iss");
        }

        public Bam.Core.TokenizedString ScriptPath
        {
            get;
            private set;
        }

        public void
        AddFile(
            Bam.Core.Module module,
            Bam.Core.PathKey key)
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        public void
        AddPath(
            Bam.Core.Module module,
            Bam.Core.PathKey key)
        {
            this.DependsOn(module);
            this.Paths.Add(module, key);
        }

        protected override void
        EvaluateInternal()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var path = this.ScriptPath.ToString();
            var dir = System.IO.Path.GetDirectoryName(path);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(dir);
            var outputName = this.GetEncapsulatingReferencedModule().Macros["OutputName"];
            using (var scriptWriter = new System.IO.StreamWriter(path))
            {
                scriptWriter.WriteLine("[Setup]");
                scriptWriter.WriteLine("OutputBaseFilename={0}", outputName.ToStringQuoteIfNecessary());
                var installedExePath = this.CreateTokenizedString("@dir($(buildroot)/$(config)/$(0).exe)", outputName);
                installedExePath.Parse();
                scriptWriter.WriteLine("OutputDir={0}", installedExePath.ToStringQuoteIfNecessary());
                // create the output directory in-advance, so that multiple InnoSetup processes, writing to the same folder
                // do not hit a race condition in creating this folder
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(installedExePath.ToStringQuoteIfNecessary());
                scriptWriter.WriteLine("AppName={0}", outputName.ToString());
                var productDef = Bam.Core.Graph.Instance.ProductDefinition;
                if (null != productDef)
                {
                    scriptWriter.WriteLine(
                        "AppVersion={0}",
                        System.String.Format(
                            "{0}.{1}.{2}",
                            productDef.MajorVersion.HasValue ? productDef.MajorVersion.Value : 1,
                            productDef.MinorVersion.HasValue ? productDef.MinorVersion.Value : 0,
                            productDef.PatchVersion.HasValue ? productDef.PatchVersion.Value : 0
                        )
                    );
                }
                else
                {
                    scriptWriter.WriteLine("AppVersion={0}", "1.0.0");
                }
                scriptWriter.WriteLine("DefaultDirName={{userappdata}}\\{0}", outputName.ToString());
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

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
        }
    }

    public sealed class InnoSetupCompilerSettings :
        Bam.Core.Settings
    {
    }

    public sealed class InnoSetupCompiler :
        Bam.Core.PreBuiltTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            this.Macros.Add("toolPath", Bam.Core.TokenizedString.Create("$(0)/Inno Setup 5/ISCC.exe", null, new Bam.Core.TokenizedStringArray(Bam.Core.OSUtilities.WindowsProgramFilesx86Path)));
            // since the toolPath macro is needed to evaluate the Executable property
            // in the check for existence
            base.Init(parent);
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new InnoSetupCompilerSettings();
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["toolPath"];
            }
        }
    }

    /// <summary>
    /// Derive from this module to create an InnoSetup installer of the specified files.
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    public abstract class InnoSetupInstaller :
        Bam.Core.Module
    {
        private InnoSetupScript ScriptModule;
        private Bam.Core.PreBuiltTool Compiler;
        private IInnoSetupPolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.ScriptModule = Bam.Core.Module.Create<InnoSetupScript>();
            this.DependsOn(this.ScriptModule);

            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<InnoSetupCompiler>();
            this.Requires(this.Compiler);
        }

        /// <summary>
        /// Include an individual file in the installer.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        Include<DependentModule>(
            Bam.Core.PathKey key) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.ScriptModule.AddFile(dependent, key);
        }

        /// <summary>
        /// Include a folder in the installer, such as the output from one of the Publisher collation steps.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        SourceFolder<DependentModule>(
            Bam.Core.PathKey key) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.ScriptModule.AddPath(dependent, key);
        }

        protected sealed override void
        EvaluateInternal()
        {
            // do nothing
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateInstaller(this, context, this.Compiler, this.ScriptModule.ScriptPath);
            }
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Installer." + mode + "InnoSetup";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<IInnoSetupPolicy>.Create(className);
            }
        }
    }
}
