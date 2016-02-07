#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// <summary>
    /// Base class for containers of homogenous object files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    public abstract class CModuleContainer<ChildModuleType> :
        CModule, // TODO: should this be here? it has no headers, nor version number
        Bam.Core.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    {
        private System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        private SourceFileType
        CreateSourceFile<SourceFileType>(
            string path,
            Bam.Core.Module macroModuleOverride,
            bool verbatim)
            where SourceFileType : Bam.Core.Module, Bam.Core.IInputPath, new()
        {
            // explicitly make a source file
            var sourceFile = Bam.Core.Module.Create<SourceFileType>();
            if (verbatim)
            {
                sourceFile.InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
            }
            else
            {
                var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
                sourceFile.InputPath = macroModule.CreateTokenizedString(path);
            }
            return sourceFile;
        }

        /// <summary>
        /// Add a single object file, given the source path, to the container. Path must resolve to a single file.
        /// </summary>
        /// <returns>The object file module, in order to manage patches.</returns>
        /// <param name="path">Path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="verbatim">If set to <c>true</c> verbatim.</param>
        public ChildModuleType
        AddFile(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);

            if (child is IRequiresSourceModule)
            {
                var source = this.CreateSourceFile<SourceFile>(path, macroModuleOverride, verbatim);
                (child as IRequiresSourceModule).Source = source;
            }
            else
            {
                if (verbatim)
                {
                    child.InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
                }
                else
                {
                    var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
                    child.InputPath = macroModule.CreateTokenizedString(path);
                }
            }

            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        /// <summary>
        /// Add multiple object files, given a wildcarded source path.
        /// Allow filtering on the expanded paths, so that only matching paths are included.
        /// </summary>
        /// <returns>The files.</returns>
        /// <param name="path">Path.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="filter">Filter.</param>
        public Bam.Core.Array<Bam.Core.Module>
        AddFiles(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            var wildcardPath = macroModule.CreateTokenizedString(path).Parse();

            var dir = System.IO.Path.GetDirectoryName(wildcardPath);
            if (!System.IO.Directory.Exists(dir))
            {
                throw new Bam.Core.Exception("The directory {0} does not exist", dir);
            }
            var leafname = System.IO.Path.GetFileName(wildcardPath);
            var option = leafname.Contains("**") ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            var files = System.IO.Directory.GetFiles(dir, leafname, option);
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception("No files were found that matched the pattern '{0}'", wildcardPath);
            }
            if (filter != null)
            {
                var filteredFiles = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
                if (0 == filteredFiles.Length)
                {
                    throw new Bam.Core.Exception("No files were found that matched the pattern '{0}', after applying the regex filter. {1} were found prior to applying the filter.", wildcardPath, files.Count());
                }
                files = filteredFiles;
            }
            var modulesCreated = new Bam.Core.Array<Bam.Core.Module>();
            foreach (var filepath in files)
            {
                modulesCreated.Add(this.AddFile(filepath, verbatim: true));
            }
            return modulesCreated;
        }

        /// <summary>
        /// Add an object file from an existing SourceFile module.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="sourceModule">Source module.</param>
        /// <param name="preInitDlg">Pre init dlg.</param>
        public ChildModuleType
        AddFile(
            SourceFile sourceModule,
            Bam.Core.Module.PreInitDelegate<ChildModuleType> preInitDlg = null)
        {
            var child = Bam.Core.Module.Create<ChildModuleType>(this, preInitCallback: preInitDlg);
            var requiresSource = child as IRequiresSourceModule;
            if (null == requiresSource)
            {
                throw new Bam.Core.Exception("Module type {0} does not implement the interface {1}", typeof(ChildModuleType).FullName, typeof(IRequiresSourceModule).FullName);
            }
            requiresSource.Source = sourceModule;
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        // note that this is 'new' to hide the version in Bam.Core.Module
        // C# does not support return type covariance (https://en.wikipedia.org/wiki/Covariant_return_type)
        public new System.Collections.ObjectModel.ReadOnlyCollection<ChildModuleType> Children
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<ChildModuleType>(base.Children.Select(item => item as ChildModuleType).ToList());
            }
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
            try
            {
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
            catch (System.AggregateException exception)
            {
                throw new Bam.Core.Exception(exception, "Failed to evaluate modules");
            }
        }
    }
}
