// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        private class UniquePathCollection
        {
            private Opus.Core.StringArray pathList = new Opus.Core.StringArray();

            public void Add(string path)
            {
                if (!System.String.IsNullOrEmpty(path) && !pathList.Contains(path))
                {
                    pathList.Add(path);
                }
            }

            public System.Collections.Generic.IEnumerator<string> GetEnumerator()
            {
                return this.pathList.GetEnumerator();
            }

            public override string ToString()
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                foreach (string environmentPath in this.pathList)
                {
                    builder.AppendFormat("{0};", environmentPath);
                }
                return builder.ToString();
            }
        }

        public void PostExecute(Opus.Core.DependencyNodeCollection nodeCollection)
        {
            Opus.Core.Log.DebugMessage("PostExecute for MakeFiles");

            string targetList = null;
            UniquePathCollection environmentPaths = new UniquePathCollection();
            foreach (Opus.Core.DependencyNode node in nodeCollection)
            {
                MakeFileData data = node.Data as MakeFileData;
                if (data != null)
                {
                    foreach (Opus.Core.StringArray targetNames in data.TargetDictionary.Values)
                    {
                        foreach (string targetName in targetNames)
                        {
                            targetList += targetName + " ";
                        }
                    }

                    if (data.EnvironmentPaths != null)
                    {
                        foreach (string environmentPath in data.EnvironmentPaths)
                        {
                            environmentPaths.Add(environmentPath);
                        }
                    }
                }
            }

            // write top level Makefile
            {
                Opus.Core.Log.DebugMessage("Makefile : '{0}'", this.topLevelMakeFilePath);
                using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(this.topLevelMakeFilePath))
                {
                    makeFileWriter.WriteLine("# Record the current directory");
                    if (Opus.Core.OSUtilities.IsWindowsHosting)
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
                        makeFileWriter.WriteLine("PATH := {0}$(INITIALPATH)", environmentPaths.ToString());
                        makeFileWriter.WriteLine("");
                    }

                    makeFileWriter.WriteLine("# include all sub-makefiles");
                    foreach (Opus.Core.DependencyNode node in nodeCollection)
                    {
                        MakeFileData data = node.Data as MakeFileData;
                        if (data != null)
                        {
                            //Opus.Core.Log.DebugMessage("\t'{0}' - target '{1}'", data.MakeFilePath, data.Target);
                            //if (!data.Included)
                            {
                                string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.MakeFilePath, this.topLevelMakeFilePath, "$(CURDIR)");
                                makeFileWriter.WriteLine("include {0}", relativeDataFile);
                            }
                        }
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Create any directories necessary");
                    makeFileWriter.WriteLine("$(sort $(builddirs)):");
                    if (Opus.Core.OSUtilities.IsWindowsHosting)
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
                    if (Opus.Core.OSUtilities.IsWindowsHosting)
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
        }
    }
}