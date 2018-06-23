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
namespace C
{
    /// <summary>
    /// Object file compiled against C.
    /// </summary>
    public class ObjectFile :
        ObjectFileBase
    {
        private ICompilationPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Tool = DefaultToolchain.C_Compiler(this.BitDepth);
        }

        public CompilerTool Compiler
        {
            get
            {
                return this.Tool as CompilerTool;
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
            var sourceFile = this.SourceModule;
            var objectFile = this.GeneratedPaths[Key];
            this.Policy.Compile(this, context, objectFile, sourceFile);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "C." + mode + "Compilation";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<ICompilationPolicy>.Create(className);
        }

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
                    // wait for execution task to be finished
                    var execution_task = (dep as Bam.Core.IModuleExecution).ExecutionTask;
                    if (null == execution_task)
                    {
                        throw new Bam.Core.Exception("No execution task available for dependent {0}, of {1}", dep.ToString(), this.ToString());
                    }
                    execution_task.Wait();
                }
            }

            // does the object file exist?
            var objectFilePath = this.GeneratedPaths[Key].ToString();
            if (!System.IO.File.Exists(objectFilePath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var objectFileWriteTime = System.IO.File.GetLastWriteTime(objectFilePath);

            // has the source file been evaluated to be rebuilt?
            if ((this as IRequiresSourceModule).Source.ReasonToExecute != null)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.InputPath);
                return;
            }

            // is the source file newer than the object file?
            var sourcePath = this.InputPath.ToString();
            var sourceWriteTime = System.IO.File.GetLastWriteTime(sourcePath);
            if (sourceWriteTime > objectFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.InputPath);
                return;
            }

            if (this is WinResource)
            {
                return;
            }

            // are there any headers as explicit dependencies (procedurally generated most likely), which are newer?
            var explicitHeadersUpdated = new Bam.Core.StringArray();
            foreach (var dep in this.Dependents)
            {
                if (!(dep is HeaderFile))
                {
                    continue;
                }
                if (null == dep.ReasonToExecute)
                {
                    continue;
                }
                var headerDep = dep as HeaderFile;
                if (dep.ReasonToExecute.Reason == Bam.Core.ExecuteReasoning.EReason.InputFileIsNewer)
                {
                    explicitHeadersUpdated.AddUnique(headerDep.InputPath.ToString());
                }
            }

            var includeSearchPaths = (this.Settings as C.ICommonCompilerSettings).IncludePaths;
            // implicitly search the same directory as the source path, as this is not needed to be explicitly on the include path list
            var currentDir = this.CreateTokenizedString("@dir($(0))", this.InputPath);
            currentDir.Parse();
            includeSearchPaths.AddUnique(currentDir);

            var filesToSearch = new System.Collections.Generic.Queue<string>();
            filesToSearch.Enqueue(sourcePath);

            var headerPathsFound = new Bam.Core.StringArray();
            while (filesToSearch.Count > 0)
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
                if (0 == matches.Count)
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
                                    this.GeneratedPaths[Key],
                                    Bam.Core.TokenizedString.CreateVerbatim(potentialPath));
                                return;
                            }

                            // found #included header in list of explicitly dependent headers that have been updated
                            if (explicitHeadersUpdated.Contains(potentialPath))
                            {
                                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                                    this.GeneratedPaths[Key],
                                    Bam.Core.TokenizedString.CreateVerbatim(potentialPath));
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
                            Bam.Core.Log.MessageAll("IncludeDependency Exception: Cannot locate '{0}' on '{1}' due to {2}", headerFile, includePath, ex.Message);
                        }
                    }

                    if (!exists)
                    {
#if false
                            Bam.Core.Log.DebugMessage("***** Could not locate '{0}' on any include search path, included from {1}:\n{2}",
                                                        match.Groups[1],
                                                        fileToSearch,
                                                        entry.includePaths.ToString('\n'));
#endif
                    }
                }
            }

            return;
        }
    }
}
