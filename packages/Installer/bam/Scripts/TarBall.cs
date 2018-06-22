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
using Bam.Core;
using System.Linq;
namespace Installer
{
    class TarInputFiles :
        Bam.Core.Module
    {
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.ScriptPath = this.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)/$(config)/tarinput.txt");
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
                        scriptWriter.WriteLine("-C{0}", fileDir);
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
                        scriptWriter.WriteLine("-C{0}", fileDir);
                    }
                    scriptWriter.WriteLine(".");
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

    public sealed class TarCompiler :
        Bam.Core.PreBuiltTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new TarBallSettings(module);
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("tar").First());
            }
        }
    }

    /// <summary>
    /// Derive from this module to create a tarball of the specified files.
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Linux | Bam.Core.EPlatform.OSX)]
    public abstract class TarBall :
        Bam.Core.Module
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Installer");

        private TarInputFiles InputFiles;
        private ITarPolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(buildroot)/$(config)/$(OutputName)$(tarext)"));

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
            Bam.Core.PathKey key) where DependentModule : Bam.Core.Module, new()
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
            Bam.Core.PathKey key) where DependentModule : Bam.Core.Module, new()
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

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateTarBall(this, context, this.Tool as Bam.Core.ICommandLineTool, this.InputFiles.ScriptPath, this.GeneratedPaths[Key]);
            }
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Installer." + mode + "TarBall";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<ITarPolicy>.Create(className);
            }
        }
    }
}
