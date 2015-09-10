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
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder :
        Bam.Core.IBuilderPostExecute
    {
        private class UniquePathCollection
        {
            private Bam.Core.StringArray pathList = new Bam.Core.StringArray();

            public void
            Add(
                string path)
            {
                if (!System.String.IsNullOrEmpty(path) && !pathList.Contains(path))
                {
                    pathList.Add(path);
                }
            }

            public System.Collections.Generic.IEnumerator<string>
            GetEnumerator()
            {
                return this.pathList.GetEnumerator();
            }

            public override string
            ToString()
            {
                // TODO: check whether the separator needs to be different on Linux and OSX?

                var builder = new System.Text.StringBuilder();
                foreach (var environmentPath in this.pathList)
                {
                    builder.AppendFormat("{0};", environmentPath);
                }
                return builder.ToString();
            }
        }

        #region IBuilderPostExecute Members

        void
        Bam.Core.IBuilderPostExecute.PostExecute(
            Bam.Core.DependencyNodeCollection executedNodes)
        {
            Bam.Core.Log.DebugMessage("PostExecute for MakeFiles");

            if (0 == executedNodes.Count)
            {
                Bam.Core.Log.Info("No MakeFile written as there were no targets generated");
                return;
            }

            string targetList = null;
            var environmentPaths = new UniquePathCollection(); // TODO: redundant
            var environment = new System.Collections.Generic.Dictionary<string, UniquePathCollection>();
            foreach (var node in executedNodes)
            {
                var data = node.Data as MakeFileData;
                if (data != null)
                {
                    // if a node has no parent, then it is a good choice for exposing the
                    // target for in the main Makefile. However, some false-positives can
                    // arise from this for orphaned modules that are dependents from child
                    // modules (e.g. code generators)
                    if (null == node.Parent)
                    {
                        foreach (var targetNames in data.TargetDictionary.Values)
                        {
                            foreach (var targetName in targetNames)
                            {
                                targetList += targetName + " ";
                            }
                        }
                    }

                    if (data.Environment != null)
                    {
                        foreach (var key in data.Environment.Keys)
                        {
                            if (!environment.ContainsKey(key))
                            {
                                environment[key] = new UniquePathCollection();
                            }

                            foreach (var path in data.Environment[key])
                            {
                                environment[key].Add(path);
                            }
                        }
                    }
                }
            }

            // write top level Makefile
            {
                Bam.Core.Log.DebugMessage("Makefile : '{0}'", this.topLevelMakeFilePath);
                using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(this.topLevelMakeFilePath))
                {
                    makeFileWriter.WriteLine("# Record the current directory");
                    if (Bam.Core.OSUtilities.IsWindowsHosting)
                    {
                        makeFileWriter.WriteLine("CURDIR := $(subst /,\\,$(realpath .))");
                    }
                    else
                    {
                        makeFileWriter.WriteLine("CURDIR := $(realpath .)");
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Default goal");
                    makeFileWriter.WriteLine(".PHONY: all");
                    makeFileWriter.WriteLine("all: {0}", targetList);
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Remove all implicit rule suffixes");
                    makeFileWriter.WriteLine(".SUFFIXES:");
                    makeFileWriter.WriteLine("");

                    if (null != environmentPaths)
                    {
                        makeFileWriter.WriteLine("# Environment PATH for all tools");
                        makeFileWriter.WriteLine("INITIALPATH := $(PATH)");
                        makeFileWriter.WriteLine("export PATH := {0}$(INITIALPATH)", environmentPaths.ToString());
                        makeFileWriter.WriteLine("");
                    }

                    if (null != environment && environment.Count > 0)
                    {
                        makeFileWriter.WriteLine("# Environment variables for all tools");
                        foreach (var key in environment.Keys)
                        {
                            makeFileWriter.WriteLine("INITIAL{0} := $({0})", key);
                            makeFileWriter.WriteLine("export {0} := {1}$(INITIAL{0})", key, environment[key].ToString());
                        }
                        makeFileWriter.WriteLine("");
                    }

                    makeFileWriter.WriteLine("# include all sub-makefiles");
                    foreach (var node in executedNodes)
                    {
                        var data = node.Data as MakeFileData;
                        if (data != null)
                        {
                            var relativeDataFile = Bam.Core.RelativePathUtilities.GetPath(data.MakeFilePath, this.topLevelMakeFilePath, "$(CURDIR)");
                            makeFileWriter.WriteLine("include {0}", relativeDataFile);
                        }
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Create any directories necessary");
                    makeFileWriter.WriteLine("$(sort $(builddirs)):");
                    if (Bam.Core.OSUtilities.IsWindowsHosting)
                    {
                        makeFileWriter.WriteLine("\t-mkdir $@");
                    }
                    else
                    {
                        makeFileWriter.WriteLine("\t-mkdir -p $@");
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Delete generated files");
                    makeFileWriter.WriteLine(".PHONY: clean");
                    makeFileWriter.WriteLine("clean:");
                    if (Bam.Core.OSUtilities.IsWindowsHosting)
                    {
                        makeFileWriter.WriteLine("\t-rmdir /S /Q $(sort $(builddirs)) 2>nul");
                    }
                    else
                    {
                        makeFileWriter.WriteLine("\t-rm -f -r $(sort $(builddirs)) >nul");
                    }
                    makeFileWriter.WriteLine("");
                }
            }

#if false
            Bam.Core.Log.Info("Successfully created MakeFile for package '{0}'\n\t{1}", Bam.Core.State.PackageInfo[0].Name, this.topLevelMakeFilePath);
#endif
        }

        #endregion
    }
}
