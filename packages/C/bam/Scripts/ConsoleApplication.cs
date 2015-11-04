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
namespace C
{
    public class ConsoleApplication :
        CModule
    {
        protected Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private Bam.Core.Array<Bam.Core.Module> linkedModules = new Bam.Core.Array<Bam.Core.Module>();
        private ILinkingPolicy Policy = null;

        static public Bam.Core.FileKey Key = Bam.Core.FileKey.Generate("ExecutableFile");
        static public Bam.Core.FileKey PDBKey = Bam.Core.FileKey.Generate("Windows Program DataBase File");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(OutputName)$(exeext)"));
            this.Linker = DefaultToolchain.C_Linker(this.BitDepth);
            this.PrivatePatch(settings =>
            {
                var linker = settings as C.ICommonLinkerSettings;
                linker.OutputType = ELinkerOutput.Executable;
            });
        }

        protected Bam.Core.Module.PrivatePatchDelegate ConsolePreprocessor = settings =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                compiler.PreprocessorDefines.Add("_CONSOLE");
            };

        public virtual CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<CObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        public virtual C.ObjC.ObjectFileCollection
        CreateObjectiveCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<C.ObjC.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
            this.sourceModules.Add(source);
            return source;
        }

        public void
        CompileAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : HeaderLibrary, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source module argument must be passed to {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, this.ToString());
            }

            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
        }

        public void
        LinkAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            this.UsePublicPatches(dependent);
        }

        public void
        RequiredToExist<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
        }

        public void
        CompileAndLinkAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source must be provided");
            }

            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
            this.LinkAllForwardedDependenciesFromLibraries(dependent);
        }

        public void
        CompilePubliclyAndLinkAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            this.CompileAndLinkAgainst<DependentModule>(affectedSources);
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.UsePublicPatches(dependent);
        }

        private void
        LinkAllForwardedDependenciesFromLibraries(
            Bam.Core.Module module)
        {
            var withForwarded = module as IForwardedLibraries;
            if (null == withForwarded)
            {
                return;
            }

            // recursive
            foreach (var forwarded in withForwarded.ForwardedLibraries)
            {
                this.DependsOn(forwarded);
                // some linkers require a specific order of libraries in order to resolve symbols
                // so that if an existing library is later referenced, it needs to be moved later
                if (this.linkedModules.Contains(forwarded))
                {
                    this.linkedModules.Remove(forwarded);
                }
                this.linkedModules.Add(forwarded);
                this.LinkAllForwardedDependenciesFromLibraries(forwarded);
            }
        }

        public LinkerTool Linker
        {
            get
            {
                return this.Tool as LinkerTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var source = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module>(FlattenHierarchicalFileList(this.sourceModules).ToArray());
            var headers = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module>(FlattenHierarchicalFileList(this.headerModules).ToArray());
            var linked = new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module>(this.linkedModules.ToArray());
            var executable = this.GeneratedPaths[Key];
            this.Policy.Link(this, context, executable, source, headers, linked, null);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "C." + mode + "Linker";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<ILinkingPolicy>.Create(className);
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var exists = System.IO.File.Exists(this.GeneratedPaths[Key].ToString());
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            foreach (var source in this.linkedModules)
            {
                if (null != source.EvaluationTask)
                {
                    source.EvaluationTask.Wait();
                }
                if (null != source.ReasonToExecute)
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                    return;
                }
            }
            foreach (var source in this.sourceModules)
            {
                if (null != source.EvaluationTask)
                {
                    source.EvaluationTask.Wait();
                }
                if (null != source.ReasonToExecute)
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                    return;
                }
            }
        }
    }
}
