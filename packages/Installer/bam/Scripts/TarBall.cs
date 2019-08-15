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
using Bam.Core;
using System.Linq;
namespace Installer
{
    /// <summary>
    /// Module representing the input file to tar
    /// </summary>
    class TarInputFiles :
        Bam.Core.Module
    {
        private readonly System.Collections.Generic.Dictionary<Bam.Core.Module, string> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
        private readonly System.Collections.Generic.Dictionary<Bam.Core.Module, string> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();

        // TODO: this could be improved
        /// <summary>
        /// Access to Module-pathkey pairs
        /// </summary>
        public System.Collections.Generic.KeyValuePair<string, Bam.Core.Module> ModulePathKeyPair => new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(this.Paths.First().Value, this.Paths.First().Key);

        /// <summary>
        /// Initialize this module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            var parentModule = Bam.Core.Graph.Instance.ModuleStack.Peek();
            this.ScriptPath = this.CreateTokenizedString(
                "$(buildroot)/$(0)/$(config)/tarinput.txt",
                new[] { parentModule.Macros[Bam.Core.ModuleMacroNames.ModuleName] }
            );
        }

        /// <summary>
        /// Get the script path used for writing tars
        /// </summary>
        public Bam.Core.TokenizedString ScriptPath { get; private set; }

        /// <summary>
        /// Add a file from a module
        /// </summary>
        /// <param name="module">Module to add</param>
        /// <param name="key">Path key from Module</param>
        public void
        AddFile(
            Bam.Core.Module module,
            string key)
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        /// <summary>
        /// Add a path (a directory) from a module
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

        /// <summary>
        /// Execute the tool on this module
        /// </summary>
        /// <param name="context">in this context</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var path = this.ScriptPath.ToString();
            var dir = System.IO.Path.GetDirectoryName(path);
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(dir);
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
                        scriptWriter.WriteLine($"-C{fileDir}");
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
                        scriptWriter.WriteLine($"-C{fileDir}");
                    }
                    scriptWriter.WriteLine(".");
                }
            }
        }
    }

    /// <summary>
    /// Prebuilt tool module for tar
    /// </summary>
    public sealed class TarCompiler :
        Bam.Core.PreBuiltTool
    {
        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(TarBallSettings);

        /// <summary>
        /// Executable path to the tar compiler
        /// </summary>
        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("tar").First());
    }

    /// <summary>
    /// Derive from this module to create a tarball of the specified files.
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux | Bam.Core.EPlatform.OSX)]
    public abstract class TarBall :
        Bam.Core.Module
    {
        /// <summary>
        /// Path key to the tarball
        /// </summary>
        public const string TarBallKey = "TarBall Installer";

        private TarInputFiles InputFiles;

        /// <summary>
        /// Initialize the module
        /// </summary>
        protected override void
        Init()
        {
            base.Init();

            this.RegisterGeneratedFile(
                TarBallKey,
                this.CreateTokenizedString("$(buildroot)/$(config)/$(OutputName)$(tarext)")
            );

            this.InputFiles = Bam.Core.Module.Create<TarInputFiles>();
            this.DependsOn(this.InputFiles);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<TarCompiler>();
            this.Requires(this.Tool);
        }

        /// <summary>
        /// Include the specified file into the tarball.
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
            this.InputFiles.AddFile(dependent, key);
        }

        /// <summary>
        /// Include the folder into the tarball, usually one of the results of Publishing collation.
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
            this.InputFiles.AddPath(dependent, key);
        }

        protected sealed override void
        EvaluateInternal()
        {
            // do nothing
        }

        /// <summary>
        /// Execute the tool on this module
        /// </summary>
        /// <param name="context">in this context</param>
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

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    Bam.Core.Log.DebugMessage("Tar not supported on Xcode builds");
                    break;
#endif

                default:
                    throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// Enumerate across all inputs to this module
        /// </summary>
        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return this.InputFiles.ModulePathKeyPair;
            }
        }
    }
}
