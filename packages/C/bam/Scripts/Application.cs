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
using System.Linq;
namespace C
{
    public sealed class DefaultBitDepth :
        Bam.Core.IIntegerCommandLineArgument
    {
        string Bam.Core.ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Change the default bit depth of the builds. Default is 64.";
            }
        }

        string Bam.Core.ICommandLineArgument.LongName
        {
            get
            {
                return "--C.bitdepth";
            }
        }

        string Bam.Core.ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        int Bam.Core.ICommandLineArgumentDefault<int>.Default
        {
            get
            {
                return 64;
            }
        }
    }

    public abstract class CModule :
        Bam.Core.Module
    {
        protected Bam.Core.Array<Bam.Core.Module> headerModules = new Bam.Core.Array<Bam.Core.Module>();

        public CModule()
        {
            this.Macros.Add("OutputName", this.Macros["modulename"]);
            // default bit depth
            this.BitDepth = (EBit)Bam.Core.CommandLineProcessor.Evaluate(new DefaultBitDepth());
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // if there is a parent from which this module is created, inherit bitdepth
            if (null != parent)
            {
                this.BitDepth = (parent as CModule).BitDepth;
            }
        }

        public EBit BitDepth
        {
            get;
            set;
        }

        protected static Bam.Core.Array<Bam.Core.Module>
        FlattenHierarchicalFileList(
            Bam.Core.Array<Bam.Core.Module> files)
        {
            var list = new Bam.Core.Array<Bam.Core.Module>();
            foreach (var input in files)
            {
                if (input is Bam.Core.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        list.Add(child);
                    }
                }
                else
                {
                    list.Add(input);
                }
            }
            return list;
        }

        protected T
        InternalCreateContainer<T>(
            bool requires,
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null,
            Bam.Core.Module.PrivatePatchDelegate privatePatch = null)
            where T : CModule, new()
        {
            var source = Bam.Core.Module.Create<T>(this);
            if (null != privatePatch)
            {
                source.PrivatePatch(privatePatch);
            }

            if (requires)
            {
                this.Requires(source);
            }
            else
            {
                this.DependsOn(source);
            }

            if (null != wildcardPath)
            {
                (source as IAddFiles).AddFiles(wildcardPath, macroModuleOverride: macroModuleOverride, filter: filter);
            }

            return source;
        }

        public HeaderFileCollection
        CreateHeaderContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var headers = this.InternalCreateContainer<HeaderFileCollection>(true, wildcardPath, macroModuleOverride, filter);
            this.headerModules.Add(headers);
            return headers;
        }
    }

    interface IAddFiles
    {
        Bam.Core.Array<Bam.Core.Module>
        AddFiles(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null);
    }

    public abstract class CModuleContainer<ChildModuleType> :
        CModule,
        Bam.Core.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    {
        private System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        public ChildModuleType
        AddFile(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            child.InputPath = Bam.Core.TokenizedString.Create(path, macroModule, verbatim);
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        public Bam.Core.Array<Bam.Core.Module>
        AddFiles(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            var wildcardPath = Bam.Core.TokenizedString.Create(path, macroModule).Parse();

            var dir = System.IO.Path.GetDirectoryName(wildcardPath);
            var leafname = System.IO.Path.GetFileName(wildcardPath);
            var files = System.IO.Directory.GetFiles(dir, leafname, System.IO.SearchOption.TopDirectoryOnly);
            if (filter != null)
            {
                files = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
            }
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception("No files were found that matched the pattern '{0}'", wildcardPath);
            }
            var modulesCreated = new Bam.Core.Array<Bam.Core.Module>();
            foreach (var filepath in files)
            {
                var fp = filepath;
                modulesCreated.Add(this.AddFile(fp, verbatim: true));
            }
            return modulesCreated;
        }

        public ChildModuleType
        AddFile(
            Bam.Core.FileKey generatedFileKey,
            Bam.Core.Module module,
            Bam.Core.Module.ModulePreInitDelegate preInitDlg = null)
        {
            if (!module.GeneratedPaths.ContainsKey(generatedFileKey))
            {
                throw new System.Exception(System.String.Format("No generated path found with key '{0}'", generatedFileKey.Id));
            }
            var child = Bam.Core.Module.Create<ChildModuleType>(this, preInitCallback: preInitDlg);
            child.InputPath = module.GeneratedPaths[generatedFileKey];
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            child.DependsOn(module);
            return child;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // do nothing
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
            // TODO: might have to get the policy, for the sharing settings
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            foreach (var child in this.children)
            {
                if (null != child.EvaluationTask)
                {
                    child.EvaluationTask.Wait();
                }
                if (null != child.ReasonToExecute)
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(child.ReasonToExecute.OutputFilePath, child.ReasonToExecute.OutputFilePath);
                    return;
                }
            }
        }
    }

    public abstract class CSDKModule :
        CModule
    { }

    public abstract class ExternalFramework :
        CModule
    { }

    public class ConsoleApplication :
        CModule
    {
        protected Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private Bam.Core.Array<Bam.Core.Module> linkedModules = new Bam.Core.Array<Bam.Core.Module>();
        private ILinkerPolicy Policy = null;

        static public Bam.Core.FileKey Key = Bam.Core.FileKey.Generate("ExecutableFile");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.RegisterGeneratedFile(Key, Bam.Core.TokenizedString.Create("$(pkgbuilddir)/$(moduleoutputdir)/$(OutputName)$(exeext)", this));
            this.Linker = DefaultToolchain.C_Linker(this.BitDepth);
            this.PrivatePatch(settings =>
            {
                var linker = settings as C.ICommonLinkerOptions;
                linker.OutputType = ELinkerOutput.Executable;
            });
        }

        private Bam.Core.Module.PrivatePatchDelegate ConsolePreprocessor = settings =>
            {
                var compiler = settings as C.ICommonCompilerOptions;
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

        public virtual Cxx.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<Cxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
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

        public virtual C.ObjCxx.ObjectFileCollection
        CreateObjectiveCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<C.ObjCxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter, this.ConsolePreprocessor);
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

        public void LinkAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.linkedModules.Add(dependent);
            this.UsePublicPatches(dependent);
        }

        public void RequiredToExist<DependentModule>() where DependentModule : CModule, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.Requires(dependent);
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

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "C." + mode + "Linker";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<ILinkerPolicy>.Create(className);
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
    namespace Cxx
    {
        public class ConsoleApplication :
            C.ConsoleApplication
        {
            protected override void
            Init(
                Bam.Core.Module parent)
            {
                base.Init(parent);
                this.Linker = C.DefaultToolchain.Cxx_Linker(this.BitDepth);
            }
        }
    }
}
