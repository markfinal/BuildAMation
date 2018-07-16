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
    class NSISScript :
        Bam.Core.Module
    {
#if BAM_V2
        private System.Collections.Generic.Dictionary<Bam.Core.Module, string> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
        private System.Collections.Generic.Dictionary<Bam.Core.Module, string> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
#else
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Files = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Paths = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
#endif

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.ScriptPath = this.CreateTokenizedString("$(buildroot)/$(encapsulatingmodulename)/$(config)/script.nsi");
        }

        public Bam.Core.TokenizedString ScriptPath
        {
            get;
            private set;
        }

        public void
        AddFile(
            Bam.Core.Module module,
#if BAM_V2
            string key)
#else
            Bam.Core.PathKey key)
#endif
        {
            this.DependsOn(module);
            this.Files.Add(module, key);
        }

        public void
        AddPath(
            Bam.Core.Module module,
#if BAM_V2
            string key)
#else
            Bam.Core.PathKey key)
#endif
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
                scriptWriter.WriteLine("Name \"{0}\"", outputName.ToString());
                var installedExePath = this.CreateTokenizedString("$(buildroot)/$(config)/$(0).exe", outputName);
                installedExePath.Parse();
                scriptWriter.WriteLine("OutFile \"{0}\"", installedExePath.ToString());
                scriptWriter.WriteLine("InstallDir $APPDATA\\{0}", outputName.ToString());
                scriptWriter.WriteLine("Page directory");
                scriptWriter.WriteLine("Page instfiles");
                scriptWriter.WriteLine("Section \"\"");
                foreach (var dep in this.Files)
                {
                    scriptWriter.WriteLine("\tSetOutPath $INSTDIR");
                    scriptWriter.WriteLine("\tFile {0}", dep.Key.GeneratedPaths[dep.Value].ToStringQuoteIfNecessary());
                }
                foreach (var dep in this.Paths)
                {
                    scriptWriter.WriteLine("\tSetOutPath $INSTDIR");
                    scriptWriter.WriteLine("\tFile /r \"{0}\\*.*\"", dep.Key.GeneratedPaths[dep.Value].ToString());
                }
                scriptWriter.WriteLine("SectionEnd");
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
        }
    }

    public sealed class NSISCompilerSettings :
        Bam.Core.Settings
    {
    }

    public sealed class NSISCompiler :
        Bam.Core.PreBuiltTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            this.Macros.Add("toolPath", Bam.Core.TokenizedString.Create("$(0)/NSIS/makensis.exe", null, new Bam.Core.TokenizedStringArray(Bam.Core.OSUtilities.WindowsProgramFilesx86Path)));
            // since the toolPath macro is needed to evaluate the Executable property
            // in the check for existence
            base.Init(parent);
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new NSISCompilerSettings();
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
    /// Derive from this module to create an NSIS installer
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.Windows)]
    public abstract class NSISInstaller :
        Bam.Core.Module
    {
        private NSISScript ScriptModule;
        private Bam.Core.PreBuiltTool Compiler;
        private INSISPolicy Policy;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.ScriptModule = Bam.Core.Module.Create<NSISScript>();
            this.DependsOn(this.ScriptModule);

            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<NSISCompiler>();
            this.Requires(this.Compiler);
        }

        /// <summary>
        /// Include an individual file in the installer.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        Include<DependentModule>(
#if BAM_V2
            string key
#else
            Bam.Core.PathKey key
#endif
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
#if BAM_V2
            string key
#else
            Bam.Core.PathKey key
#endif
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
                var className = "Installer." + mode + "NSIS";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<INSISPolicy>.Create(className);
            }
        }
    }
}
