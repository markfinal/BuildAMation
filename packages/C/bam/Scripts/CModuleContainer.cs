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
        where ChildModuleType : Bam.Core.Module, Bam.Core.IChildModule, IRequiresSourceModule, new()
    { }

    /// <summary>
    /// Base class for collections of C files, be they compilable source or header files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    abstract class CModuleCollection<ChildModuleType> :
        CModule, // TODO: should this be here? it has no headers, nor version number
        Bam.Core.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.Module, Bam.Core.IChildModule, new()
    {
        /// <summary>
        /// list of child Modules
        /// </summary>
        protected System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        /// <summary>
        /// Add a single child module, given the source path, to the collection. Path must resolve to a single file.
        /// If the path contains a wildcard (*) character, an exception is thrown.
        /// </summary>
        /// <returns>The child module, in order to manage patches.</returns>
        /// <param name="path">Path to the child file.</param>
        /// <param name="macroModuleOverride">Macro module override.</param>
        /// <param name="verbatim">If set to <c>true</c> verbatim.</param>
        public abstract ChildModuleType
        AddFile(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            bool verbatim = false);

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
                    var requiresSourceModule = child as IRequiresSourceModule;
                    if (!requiresSourceModule.Source.InputPath.IsParsed)
                    {
                        requiresSourceModule.Source.InputPath.Parse();
                    }
                    return requiresSourceModule.Source.InputPath.ToString().Contains(truePath);
                });
                if (!validSources.Any())
                {
                    var list_of_valid_source = new System.Text.StringBuilder();
                    foreach (var child in this.children)
                    {
                        list_of_valid_source.AppendLine($"\t{(child as IRequiresSourceModule).Source.InputPath.ToString()}");
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
