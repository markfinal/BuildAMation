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
    /// Base class for containers of C files, be they compilable source or header files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    public abstract class CModuleContainer<ChildModuleType> :
        CModule, // TODO: should this be here? it has no headers, nor version number
        Bam.Core.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    {
        protected System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

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
        /// Take a container of source, and clone each of its children and embed them into a container of the same type.
        /// This is a mechanism for essentially embedding the object files that would be in a static library into a dynamic
        /// library in a cross-platform way.
        /// In the clone, private patches are copied both from the container, and also from each child in turn.
        /// No use of any public patches is made here.
        /// </summary>
        /// <param name="otherSource">The container of object files to embed into the current container.</param>
        public void
        ExtendWith(
            CModuleContainer<ChildModuleType> otherSource)
        {
            foreach (var child in otherSource.Children)
            {
                var clonedChild = Bam.Core.Module.CloneWithPrivatePatches(child, this);

                // attach the cloned object file into the container so parentage is clear for macros
                (clonedChild as Bam.Core.IChildModule).Parent = this;
                this.children.Add(clonedChild);
                this.DependsOn(clonedChild);

                // source might be a buildable module (derived from C.SourceFile), or non-buildable module (C.SourceFile), or just a path
                if (clonedChild is IRequiresSourceModule)
                {
                    var sourceOfChild = (child as IRequiresSourceModule).Source;
                    if (sourceOfChild is Bam.Core.ICloneModule)
                    {
                        (sourceOfChild as Bam.Core.ICloneModule).Clone(this, (newModule) =>
                            {
                                // associate the cloned source, to the cloned object file
                                // might need to happen prior to type-specific post-cloning ops
                                (clonedChild as IRequiresSourceModule).Source = newModule as SourceFile;
                            });
                    }
                    else
                    {
                        (clonedChild as IRequiresSourceModule).Source = sourceOfChild;
                    }
                }
                else
                {
                    clonedChild.InputPath.Aliased(child.InputPath);
                }
            }
        }

        // note that this is 'new' to hide the version in Bam.Core.Module
        // C# does not support return type covariance (https://en.wikipedia.org/wiki/Covariant_return_type)
        /// <summary>
        /// Return a read-only collection of the children of this container, using the ChildModuleType generic type for each module in the collection.
        /// </summary>
        public new System.Collections.ObjectModel.ReadOnlyCollection<ChildModuleType> Children
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<ChildModuleType>(base.Children.Select(item => item as ChildModuleType).ToList());
            }
        }

        /// <summary>
        /// Return a list of all child modules whose input path contains the specified filename.
        /// A ForEach function can be applied to the results, to run the same action on each of the child modules.
        /// </summary>
        /// <param name="filename">The filename to match</param>
        /// <returns>List of child modules.</returns>
        public System.Collections.Generic.List<ChildModuleType>
        this[string filename]
        {
            get
            {
                return this.children.Where(child => child.InputPath.Parse().Contains(filename)).ToList();
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
                Bam.Core.TokenizedString lastDeferredEvaluationPath = null;
                foreach (var child in this.children)
                {
                    if (null != child.EvaluationTask)
                    {
                        child.EvaluationTask.Wait();
                    }
                    if (null != child.ReasonToExecute)
                    {
                        switch (child.ReasonToExecute.Reason)
                        {
                            case Bam.Core.ExecuteReasoning.EReason.InputFileIsNewer:
                                {
                                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(child.ReasonToExecute.OutputFilePath, child.ReasonToExecute.OutputFilePath);
                                    return;
                                }

                            case Bam.Core.ExecuteReasoning.EReason.DeferredEvaluation:
                                lastDeferredEvaluationPath = child.ReasonToExecute.OutputFilePath;
                                break;
                        }
                    }
                }
                if (lastDeferredEvaluationPath != null)
                {
                    // deferred evaluation can only be considered when other reasons to execute have been exhausted
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.DeferredUntilBuild(lastDeferredEvaluationPath);
                }
            }
            catch (System.AggregateException exception)
            {
                throw new Bam.Core.Exception(exception, "Failed to evaluate modules");
            }
        }
    }
}
