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
using System.Linq;
namespace C
{
    /// <summary>
    /// Base class for collections of C files, be they compilable source or header files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    [System.Obsolete("Use CModuleCollection instead", true)]
    abstract class CModuleContainer<ChildModuleType> :
        CModuleCollection<ChildModuleType>
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    { }

    /// <summary>
    /// Base class for collections of C files, be they compilable source or header files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    abstract class CModuleCollection<ChildModuleType> :
        CModule, // TODO: should this be here? it has no headers, nor version number
        Bam.Core.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    {
        /// <summary>
        /// list of child Modules
        /// </summary>
        protected System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        private SourceFileType
        CreateSourceFile<SourceFileType>(
            string path,
            Bam.Core.Module macroModuleOverride,
            bool verbatim)
            where SourceFileType : Bam.Core.Module, Bam.Core.IInputPath, new()
        {
            // explicitly make a source file
            var sourceFile = Bam.Core.Module.Create<SourceFileType>(postInitCallback: (module) =>
                {
                    if (verbatim)
                    {
                        (module as SourceFileType).InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
                    }
                    else
                    {
                        var macroModule = macroModuleOverride ?? this;
                        (module as SourceFileType).InputPath = macroModule.CreateTokenizedString(path);
                    }
                });
            return sourceFile;
        }

        private SourceFileType
        CreateSourceFile<SourceFileType>(
            Bam.Core.TokenizedString path)
            where SourceFileType : Bam.Core.Module, Bam.Core.IInputPath, new()
        {
            // explicitly make a source file
            var sourceFile = Bam.Core.Module.Create<SourceFileType>(postInitCallback: (module) =>
            {
                (module as SourceFileType).InputPath = path;
            });
            return sourceFile;
        }

        /// <summary>
        /// Add a single object file, given the source path, to the collection. Path must resolve to a single file.
        /// If the path contains a wildcard (*) character, an exception is thrown.
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
            if (path.Contains('*'))
            {
                throw new Bam.Core.Exception(
                    $"Single path '{path}' cannot contain a wildcard character. Use AddFiles instead of AddFile"
                );
            }
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);

            if (child is IRequiresSourceModule requiresSourceModule)
            {
                var source = this.CreateSourceFile<SourceFile>(path, macroModuleOverride, verbatim);
                requiresSourceModule.Source = source;
            }
            else
            {
                if (verbatim)
                {
                    child.InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
                }
                else
                {
                    var macroModule = macroModuleOverride ?? this;
                    child.InputPath = macroModule.CreateTokenizedString(path);
                }
            }

            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        /// <summary>
        /// Add a single object file, given the source path, to the collection. Path must resolve to a single file.
        /// </summary>
        /// <returns>The object file module, in order to manage patches.</returns>
        /// <param name="path">Path.</param>
        public ChildModuleType
        AddFile(
            Bam.Core.TokenizedString path)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);

            if (child is IRequiresSourceModule requiresSourceModule)
            {
                var source = this.CreateSourceFile<SourceFile>(path);
                requiresSourceModule.Source = source;
            }
            else
            {
                child.InputPath = path;
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
            if (System.String.IsNullOrEmpty(path))
            {
                throw new Bam.Core.Exception("Cannot add files from an empty path");
            }

            var macroModule = macroModuleOverride ?? this;
            var tokenizedPath = macroModule.CreateTokenizedString(path);
            tokenizedPath.Parse();
            var wildcardPath = tokenizedPath.ToString();

            var dir = System.IO.Path.GetDirectoryName(wildcardPath);
            if (!System.IO.Directory.Exists(dir))
            {
                throw new Bam.Core.Exception($"The directory {dir} does not exist");
            }
            var leafname = System.IO.Path.GetFileName(wildcardPath);
            var option = leafname.Contains("**") ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            var files = System.IO.Directory.GetFiles(dir, leafname, option);
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception($"No files were found that matched the pattern '{wildcardPath}'");
            }
            if (filter != null)
            {
                var filteredFiles = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
                if (0 == filteredFiles.Length)
                {
                    throw new Bam.Core.Exception(
                        $"No files were found that matched the pattern '{wildcardPath}', after applying the regex filter. {files.Count()} were found prior to applying the filter."
                    );
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
        /// Take a collection of source, and clone each of its children and embed them into a collection of the same type.
        /// This is a mechanism for essentially embedding the object files that would be in a static library into a dynamic
        /// library in a cross-platform way.
        /// In the clone, private patches are copied both from the collection, and also from each child in turn.
        /// No use of any public patches is made here.
        /// </summary>
        /// <param name="otherSource">The collection of object files to embed into the current collection.</param>
        public void
        ExtendWith(
            CModuleCollection<ChildModuleType> otherSource)
        {
            foreach (var child in otherSource.Children)
            {
                var clonedChild = Bam.Core.Module.CloneWithPrivatePatches(child, this);

                // attach the cloned object file into the collection so parentage is clear for macros
                (clonedChild as Bam.Core.IChildModule).Parent = this;
                this.children.Add(clonedChild);
                this.DependsOn(clonedChild);

                // source might be a buildable module (derived from C.SourceFile), or non-buildable module (C.SourceFile), or just a path
                if (clonedChild is IRequiresSourceModule clonedChildRequiresSource)
                {
                    var sourceOfChild = (child as IRequiresSourceModule).Source;
                    if (sourceOfChild is Bam.Core.ICloneModule sourceOfChildIsCloned)
                    {
                        sourceOfChildIsCloned.Clone(this, (newModule) =>
                            {
                                // associate the cloned source, to the cloned object file
                                // might need to happen prior to type-specific post-cloning ops
                                clonedChildRequiresSource.Source = newModule as SourceFile;
                            });
                    }
                    else
                    {
                        clonedChildRequiresSource.Source = sourceOfChild;
                    }
                }
                else
                {
                    throw new Bam.Core.Exception(
                        new System.NotImplementedException(),
                        $"Collection does not include objects implementing the interface '{typeof(IRequiresSourceModule).ToString()}'");
                }
            }
        }

        // note that this is 'new' to hide the version in Bam.Core.Module
        // C# does not support return type covariance (https://en.wikipedia.org/wiki/Covariant_return_type)
        /// <summary>
        /// Return an enumerable of the children of this collection, using the ChildModuleType generic type for each module in the collection.
        /// </summary>
        public new System.Collections.Generic.IEnumerable<ChildModuleType> Children => base.Children.Select(item => item as ChildModuleType);

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
                var truePath = filename.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
                var validSources = this.children.Where(child =>
                    {
                        if (!child.InputPath.IsParsed)
                        {
                            child.InputPath.Parse();
                        }
                        return child.InputPath.ToString().Contains(truePath);
                    });
                if (!validSources.Any())
                {
                    var list_of_valid_source = new System.Text.StringBuilder();
                    foreach (var child in this.children)
                    {
                        list_of_valid_source.AppendLine($"\t{child.InputPath.ToString()}");
                    }
                    if (!filename.Equals(truePath, System.StringComparison.Ordinal))
                    {
                        var message = new System.Text.StringBuilder();
                        message.Append($"No source files found matching '{filename} ");
                        message.Append($"(actually checking '{truePath}' after directory slash replacement) ");
                        message.Append($"in module {Bam.Core.Graph.Instance.CommonModuleType.Peek().ToString()}. ");
                        message.AppendLine("Found");
                        message.Append(list_of_valid_source.ToString());
                        throw new Bam.Core.Exception(message.ToString());
                    }
                    else
                    {
                        var message = new System.Text.StringBuilder();
                        message.AppendLine($"No source files found matching '{filename}' in module {Bam.Core.Graph.Instance.CommonModuleType.Peek().ToString()}. Found");
                        message.Append(list_of_valid_source.ToString());
                        throw new Bam.Core.Exception(message.ToString());
                    }
                }
                return validSources.ToList();
            }
        }

        /// <summary>
        /// Execute the module
        /// </summary>
        /// <param name="context">in this context</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // do nothing
        }

        /// <summary>
        /// Evaluate the module to determine if it's up-to-date
        /// </summary>
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            try
            {
                foreach (var child in this.children)
                {
                    child.EvaluationTask?.Wait();
                    if (null != child.ReasonToExecute)
                    {
                        switch (child.ReasonToExecute.Reason)
                        {
                            case Bam.Core.ExecuteReasoning.EReason.FileDoesNotExist:
                            case Bam.Core.ExecuteReasoning.EReason.InputFileIsNewer:
                                {
                                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(child.ReasonToExecute.OutputFilePath, child.ReasonToExecute.OutputFilePath);
                                    return;
                                }

                            default:
                                throw new Bam.Core.Exception($"Unknown reason, {child.ReasonToExecute.Reason.ToString()}");
                        }
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
