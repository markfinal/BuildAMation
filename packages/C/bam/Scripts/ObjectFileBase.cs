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
    /// Generic object file.
    /// </summary>
    public abstract class ObjectFileBase :
        CModule,
        Bam.Core.IChildModule,
        Bam.Core.IInputPath,
        IRequiresSourceModule
    {
        /// <summary>
        /// Path key for this module type
        /// </summary>
        public const string ObjectFileKey = "Compiled/assembled object file";

        private Bam.Core.Module Parent = null;
        /// <summary>
        /// Source file used to generate this object file
        /// </summary>
        protected SourceFile SourceModule;

        /// <summary>
        /// Initialize this object file
        /// </summary>
        protected override void
        Init()
        {
            base.Init();
            this.PerformCompilation = true;
        }

        /// <summary>
        /// Customize the output subdirectory for object files.
        /// </summary>
        public override string CustomOutputSubDirectory => "obj";

        SourceFile IRequiresSourceModule.Source
        {
            get
            {
                return this.SourceModule;
            }

            set
            {
                if (null != this.SourceModule)
                {
                    this.SourceModule.InputPath.Parse();
                    throw new Bam.Core.Exception(
                        $"Source module already set on this object file, to '{this.SourceModule.InputPath.ToString()}'"
                    );
                }
                this.SourceModule = value;
                this.DependsOn(value);
                this.RegisterGeneratedFile(
                    ObjectFileKey,
                    this.CreateTokenizedString(
                        "$(packagebuilddir)/$(moduleoutputdir)/@changeextension(@isrelative(@trimstart(@relativeto($(0),$(packagedir)),../),@filename($(0))),$(objext))",
                        value.GeneratedPaths[SourceFile.SourceFileKey]
                    )
                );
            }
        }

        /// <summary>
        /// Set the input path to compile.
        /// </summary>
        public Bam.Core.TokenizedString InputPath
        {
            get
            {
                if (null == this.SourceModule)
                {
                    throw new Bam.Core.Exception("Source module not yet set on this object file");
                }
                return this.SourceModule.InputPath;
            }
            set
            {
                if (null != this.SourceModule)
                {
                    this.SourceModule.InputPath.Parse();
                    throw new Bam.Core.Exception(
                        $"Source module already set on this object file, to '{this.SourceModule.InputPath.ToString()}'"
                    );
                }

                // this cannot be a referenced module, since there will be more than one object
                // of this type (generally)
                // but this does mean there may be many instances of this 'type' of module
                // and for multi-configuration builds there may be many instances of the same path
                var source = Bam.Core.Module.Create<SourceFile>();
                source.InputPath = value;
                (this as IRequiresSourceModule).Source = source;
            }
        }

        Bam.Core.Module Bam.Core.IChildModule.Parent
        {
            get
            {
                return this.Parent;
            }
            set
            {
                this.Parent = value;
            }
        }

        /// <summary>
        /// Get or set whether this object file is compiled.
        /// </summary>
        public bool PerformCompilation { get; set; }

        /// <summary>
        /// Get whether header evaluation is needed.
        /// </summary>
        protected abstract bool RequiresHeaderEvaluation { get; }

        /// <summary>
        /// Execute the build on this module.
        /// </summary>
        /// <param name="context">in this context</param>
        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    {
                        if (!this.PerformCompilation)
                        {
                            return;
                        }
                        MakeFileBuilder.Support.Add(this);
                    }
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        if (!this.PerformCompilation)
                        {
                            return;
                        }
                        NativeBuilder.Support.RunCommandLineTool(this, context);
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.Compile(this);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.Compile(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Determine whether this module needs updating.
        /// </summary>
        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            if (!this.PerformCompilation)
            {
                return;
            }
            foreach (var dep in this.Dependents)
            {
                if (!(dep is SourceFile) && !dep.Executed)
                {
                    // TODO: need to revisit this
                    // it's odd to spinlock on the ExecutionTask, and then do an additional double check
                    // on whether it's got the task (when the while loop says it must)
                    // I *thought* I had coded it so that the dependencies of execution tasks were
                    // satisfied in the core
                    var as_module_execution = dep as Bam.Core.IModuleExecution;
                    while (null == as_module_execution.ExecutionTask)
                    {
                        Bam.Core.Log.DebugMessage($"******** Waiting for {dep.ToString()} to have an execution task assigned");
                        System.Threading.Thread.Yield();
                    }
                    // wait for execution task to be finished
                    var execution_task = as_module_execution.ExecutionTask;
                    if (null == execution_task)
                    {
                        throw new Bam.Core.Exception(
                            $"No execution task available for dependent {dep.ToString()}, of {this.ToString()}"
                        );
                    }
                    execution_task.Wait();
                }
            }

            // does the object file exist?
            var objectFilePath = this.GeneratedPaths[ObjectFileKey].ToString();
            if (!System.IO.File.Exists(objectFilePath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[ObjectFileKey]);
                return;
            }
            var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);

            // has the source file been evaluated to be rebuilt?
            if ((this as IRequiresSourceModule).Source.ReasonToExecute != null)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[ObjectFileKey],
                    this.InputPath
                );
                return;
            }

            // is the source file newer than the object file?
            var sourcePath = this.InputPath.ToString();
            var sourceWriteTime = System.IO.File.GetLastWriteTime(sourcePath);
            if (sourceWriteTime > objectFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[ObjectFileKey],
                    this.InputPath
                );
                return;
            }

            if (!this.RequiresHeaderEvaluation)
            {
                return;
            }

            // are there any headers as explicit dependencies (procedurally generated most likely), which are newer?
            var explicitHeadersUpdated = new Bam.Core.StringArray();
            foreach (var dep in this.Dependents)
            {
                if (dep is HeaderFile headerDep)
                {
                    if (null == dep.ReasonToExecute)
                    {
                        continue;
                    }
                    if (dep.ReasonToExecute.Reason == Bam.Core.ExecuteReasoning.EReason.InputFileIsNewer)
                    {
                        explicitHeadersUpdated.AddUnique(headerDep.InputPath.ToString());
                    }
                }
            }

            var includeSearchPaths = (this.Settings as C.ICommonPreprocessorSettings).IncludePaths;
            // implicitly search the same directory as the source path, as this is not needed to be explicitly on the include path list
            var currentDir = this.CreateTokenizedString("@dir($(0))", this.InputPath);
            currentDir.Parse();
            includeSearchPaths.AddUnique(currentDir);

            var filesToSearch = new System.Collections.Generic.Queue<string>();
            filesToSearch.Enqueue(sourcePath);

            var headerPathsFound = new Bam.Core.StringArray();
            while (filesToSearch.Any())
            {
                var fileToSearch = filesToSearch.Dequeue();

                string fileContents = null;
                using (System.IO.TextReader reader = new System.IO.StreamReader(fileToSearch))
                {
                    fileContents = reader.ReadToEnd();
                }

                // never know if developers are consistent with #include "header.h" or #include <header.h> so look for both
                // nor the amount of whitespace after #include
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    fileContents,
                    "^\\s*#include\\s*[\"<]([^\\s]*)[\">]",
                    System.Text.RegularExpressions.RegexOptions.Multiline);
                if (!matches.Any())
                {
                    // no #includes
                    return;
                }

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var headerFile = match.Groups[1].Value;
                    bool exists = false;
                    // search for the file on the include paths the compiler uses
                    foreach (var includePath in includeSearchPaths)
                    {
                        try
                        {
                            var potentialPath = System.IO.Path.Combine(includePath.ToString(), headerFile);
                            if (!System.IO.File.Exists(potentialPath))
                            {
                                continue;
                            }
                            potentialPath = System.IO.Path.GetFullPath(potentialPath);
                            var headerWriteTime = System.IO.File.GetLastWriteTime(potentialPath);

                            // early out - header is newer than generated object file
                            if (headerWriteTime > objectFileWriteTime)
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[ObjectFileKey],
                                    Bam.Core.TokenizedString.CreateVerbatim(potentialPath)
                                );
                                return;
                            }

                            // found #included header in list of explicitly dependent headers that have been updated
                            if (explicitHeadersUpdated.Contains(potentialPath))
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[ObjectFileKey],
                                    Bam.Core.TokenizedString.CreateVerbatim(potentialPath)
                                );
                                return;
                            }

                            if (!headerPathsFound.Contains(potentialPath))
                            {
                                headerPathsFound.Add(potentialPath);
                                filesToSearch.Enqueue(potentialPath);
                            }

                            exists = true;
                            break;
                        }
                        catch (System.Exception ex)
                        {
                            Bam.Core.Log.MessageAll(
                                $"IncludeDependency Exception: Cannot locate '{headerFile}' on '{includePath}' due to {ex.Message}"
                            );
                        }
                    }

                    if (!exists)
                    {
#if false
                        Bam.Core.Log.DebugMessage($"***** Could not locate '{match.Groups[1]}' on any include search path, included from {fileToSearch}:\n{entry.includePaths.ToString('\n')}");
#endif
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Enumerate across input modules.
        /// </summary>
        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(C.SourceFile.SourceFileKey, this.SourceModule);
            }
        }
    }
}
