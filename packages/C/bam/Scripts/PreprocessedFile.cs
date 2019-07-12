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
    class PreprocessedFile :
        CModule,
        Bam.Core.IInputPath,
        IRequiresSourceModule
    {
        public const string PreprocessedFileKey = "Preprocessed file";

        protected SourceFile SourceModule;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Tool = C.DefaultToolchain.Preprocessor(this.BitDepth);
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
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

            // does the preprocessed file exist?
            var preprocessedFilePath = this.GeneratedPaths[PreprocessedFileKey].ToString();
            if (!System.IO.File.Exists(preprocessedFilePath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[PreprocessedFileKey]);
                return;
            }
            var preprocessedFileWriteTime = System.IO.File.GetLastWriteTime(preprocessedFilePath);

            // has the source file been evaluated to be rebuilt?
            if ((this as IRequiresSourceModule).Source.ReasonToExecute != null)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[PreprocessedFileKey],
                    this.InputPath
                );
                return;
            }

            // is the source file newer than the preprocessed file?
            var sourcePath = this.InputPath.ToString();
            var sourceWriteTime = System.IO.File.GetLastWriteTime(sourcePath);
            if (sourceWriteTime > preprocessedFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[PreprocessedFileKey],
                    this.InputPath
                );
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

                            // early out - header is newer than generated preprocessed file
                            if (headerWriteTime > preprocessedFileWriteTime)
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[PreprocessedFileKey],
                                    Bam.Core.TokenizedString.CreateVerbatim(potentialPath)
                                );
                                return;
                            }

                            // found #included header in list of explicitly dependent headers that have been updated
                            if (explicitHeadersUpdated.Contains(potentialPath))
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[PreprocessedFileKey],
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

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(
                        this,
                        redirectOutputToFile: this.GeneratedPaths[PreprocessedFileKey]
                    );
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        NativeBuilder.Support.RunCommandLineTool(this, context);
                        NativeBuilder.Support.SendCapturedOutputToFile(
                            this,
                            context,
                            PreprocessedFileKey
                        );
                    }
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionSupport.GenerateFileFromToolStandardOutput(
                        this,
                        PreprocessedFileKey,
                        includeEnvironmentVariables: false // since it's running the preprocessor in the IDE, no environment variables necessary
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    XcodeSupport.GenerateFileFromToolStandardOutput(
                        this,
                        PreprocessedFileKey
                    );
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

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
                        $"Source module already set on this preprocessed file, to '{this.SourceModule.InputPath.ToString()}'"
                    );
                }
                this.SourceModule = value;
                this.DependsOn(value);
                this.RegisterGeneratedFile(
                    PreprocessedFileKey,
                    this.CreateTokenizedString(
                        "$(packagebuilddir)/$(moduleoutputdir)/@changeextension(@isrelative(@trimstart(@relativeto($(0),$(packagedir)),../),@filename($(0))),.c)",
                        value.GeneratedPaths[SourceFile.SourceFileKey]
                    )
                );
            }
        }

        public Bam.Core.TokenizedString InputPath
        {
            get
            {
                if (null == this.SourceModule)
                {
                    throw new Bam.Core.Exception("Source module not yet set on this preprocessed file");
                }
                return this.SourceModule.InputPath;
            }
            set
            {
                if (null != this.SourceModule)
                {
                    this.SourceModule.InputPath.Parse();
                    throw new Bam.Core.Exception(
                        $"Source module already set on this preprocessed file, to '{this.SourceModule.InputPath.ToString()}'"
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

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(SourceFile.SourceFileKey, this.SourceModule);
            }
        }
    }
}
