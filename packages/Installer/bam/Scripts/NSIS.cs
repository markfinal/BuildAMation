#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Module representing the NSIS script
    /// </summary>
    class NSISScript :
        Bam.Core.Module
    {
        private readonly System.Collections.Generic.Dictionary<Bam.Core.Module, string> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
        private readonly System.Collections.Generic.Dictionary<Bam.Core.Module, string> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();

        /// <summary>
        /// Path key to the script for NSIS
        /// </summary>
        public const string ScriptKey = "NSIS script";

        protected override void
        Init()
        {
            base.Init();

            var parentModule = Bam.Core.Graph.Instance.ModuleStack.Peek();
            this.RegisterGeneratedFile(
                ScriptKey,
                this.CreateTokenizedString(
                    "$(buildroot)/$(0)/$(config)/script.nsi",
                    new[] { parentModule.Macros[Bam.Core.ModuleMacroNames.ModuleName] }
                )
            );
        }

        /// <summary>
        /// Add a file to the NSIS installer script
        /// </summary>
        /// <param name="module">Module to add</param>
        /// <param name="key">Path key from module</param>
        public void
        AddFile(
            Bam.Core.Module module,
            string key)
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        /// <summary>
        /// Add a path (a directory) to the NSIS installer script
        /// </summary>
        /// <param name="module">Module to add</param>
        /// <param name="key">Path key from module</param>
        public void
        AddPath(
            Bam.Core.Module module,
            string key)
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
            var path = this.GeneratedPaths[ScriptKey].ToString();
            var dir = System.IO.Path.GetDirectoryName(path);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(dir);
            var outputName = this.EncapsulatingModule.Macros[Bam.Core.ModuleMacroNames.OutputName];
            using (var scriptWriter = new System.IO.StreamWriter(path))
            {
                scriptWriter.WriteLine($"Name \"{outputName.ToString()}\"");
                var installedExePath = this.CreateTokenizedString("$(buildroot)/$(config)/$(0).exe", outputName);
                installedExePath.Parse();
                scriptWriter.WriteLine($"OutFile \"{installedExePath.ToString()}\"");
                scriptWriter.WriteLine($"InstallDir $APPDATA\\{outputName.ToString()}");
                scriptWriter.WriteLine("Page directory");
                scriptWriter.WriteLine("Page instfiles");
                scriptWriter.WriteLine("Section \"\"");
                foreach (var dep in this.Files)
                {
                    scriptWriter.WriteLine("\tSetOutPath $INSTDIR");
                    scriptWriter.WriteLine($"\tFile {dep.Key.GeneratedPaths[dep.Value].ToStringQuoteIfNecessary()}");
                }
                foreach (var dep in this.Paths)
                {
                    scriptWriter.WriteLine("\tSetOutPath $INSTDIR");
                    scriptWriter.WriteLine($"\tFile /r \"{dep.Key.GeneratedPaths[dep.Value].ToString()}\\*.*\"");
                }
                scriptWriter.WriteLine("SectionEnd");
            }
        }
    }

    /// <summary>
    /// NSIS compiler settings
    /// </summary>
    [CommandLineProcessor.InputPaths(NSISScript.ScriptKey, "")]
    public sealed class NSISCompilerSettings :
        Bam.Core.Settings
    {
        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">from this Module</param>
        public NSISCompilerSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Inputs_Outputs_Cmds;
        }
    }

    /// <summary>
    /// NSIS compiler tool
    /// </summary>
    public sealed class NSISCompiler :
        Bam.Core.PreBuiltTool
    {
        protected override void
        Init()
        {
#if D_NUGET_NSIS
            this.Macros.AddVerbatim(
                "toolPath",
                Bam.Core.NuGetUtilities.GetToolExecutablePath("NSIS", this.GetType().Namespace, "makensis.exe")
            );
#endif
            // since the toolPath macro is needed to evaluate the Executable property
            // in the check for existence
            base.Init();
        }

        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(NSISCompilerSettings);

        /// <summary>
        /// Executable path to the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => this.Macros["toolPath"];
    }

    /// <summary>
    /// Derive from this module to create an NSIS installer
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    public abstract class NSISInstaller :
        Bam.Core.Module
    {
        private NSISScript ScriptModule;

        protected override void
        Init()
        {
            base.Init();

            this.ScriptModule = Bam.Core.Module.Create<NSISScript>();
            this.DependsOn(this.ScriptModule);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<NSISCompiler>();
            this.Requires(this.Tool);
        }

        /// <summary>
        /// Include an individual file in the installer.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        Include<DependentModule>(
            string key
        ) where DependentModule : Bam.Core.Module, new()
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
            string key
        ) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
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
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    Bam.Core.Log.DebugMessage("NSIS not supported on VisualStudio builds");
                    break;
#endif

                default:
                    throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// Enumerate across all inputs to this Module
        /// </summary>
        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(NSISScript.ScriptKey, this.ScriptModule);
            }
        }
    }
}
