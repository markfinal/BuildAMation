// <copyright file="MakeFileBuilder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.DeclareBuilder("MakeFile", typeof(MakeFileBuilder.MakeFileBuilder))]

namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder : Opus.Core.IBuilder
    {
        public static string GetMakeFilePathName(Opus.Core.DependencyNode node)
        {
            string makeFileDirectory = System.IO.Path.Combine(node.GetModuleBuildDirectory(), "Makefiles");
            string makeFilePathName = System.IO.Path.Combine(makeFileDirectory, System.String.Format("{0}_{1}.mak", node.UniqueModuleName, node.Target));
            return makeFilePathName;
        }
    }

    public sealed class MakeFileBuilderRecipe
    {
        private Opus.Core.OutputPaths OutputPaths
        {
            get;
            set;
        }

        private Opus.Core.StringArray InputFiles
        {
            get;
            set;
        }

        private Opus.Core.StringArray InputVariables
        {
            get;
            set;
        }

        private Opus.Core.StringArray Rules
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<string, string> DirectoriesToCreate
        {
            get;
            set;
        }

        private string ModulePrefixName
        {
            get;
            set;
        }

        public string VariableName
        {
            get
            {
                if (null == this.SecondaryVariableName)
                {
                    return this.MainVariableName;
                }
                else
                {
                    return this.SecondaryVariableName;
                }
            }
        }

        private string MainVariableName
        {
            get;
            set;
        }

        public string SecondaryVariableName
        {
            get;
            private set;
        }

        public string TargetName
        {
            get;
            private set;
        }

        private string TopLevelMakeFilePath
        {
            get;
            set;
        }

        public Opus.Core.StringArray Includes
        {
            get;
            private set;
        }

        public MakeFileBuilderRecipe(Opus.Core.DependencyNode node,
                                     Opus.Core.StringArray inputFiles,
                                     Opus.Core.StringArray inputVariables,
                                     Opus.Core.StringArray rules,
                                     string topLevelMakeFilePath)
        {
            this.TopLevelMakeFilePath = topLevelMakeFilePath;
            this.Includes = new Opus.Core.StringArray();

            this.OutputPaths = node.Module.Options.OutputPaths;
            this.InputFiles = inputFiles;
            this.InputVariables = inputVariables;
            this.Rules = rules;

            this.ModulePrefixName = System.String.Format("{0}_{1}", node.UniqueModuleName, node.Target.ToString());
            if (null == node.Parent)
            {
                this.TargetName = this.ModulePrefixName;
            }

            CommandLineProcessor.ICommandLineSupport commandLineOption = node.Module.Options as CommandLineProcessor.ICommandLineSupport;
            if (null == commandLineOption)
            {
                throw new Opus.Core.Exception(System.String.Format("OptionCollection '{0}' does not implement the CommandLineProcessor.ICommandLineSupport interface", node.Module.Options.GetType().ToString()), false);
            }
            System.Collections.Generic.Dictionary<string, string> dirsToCreateMap = new System.Collections.Generic.Dictionary<string, string>();
            int dirCount = 0;
            foreach (string dir in commandLineOption.DirectoriesToCreate())
            {
                string relativeDir = Opus.Core.RelativePathUtilities.GetPath(dir, topLevelMakeFilePath, "$(CURDIR)");
                string variableName = System.String.Format("{0}_BuildDirectory{1}", this.ModulePrefixName, dirCount);
                dirsToCreateMap.Add(variableName, relativeDir);
                ++dirCount;
            }
            if (dirsToCreateMap.Count > 0)
            {
                this.DirectoriesToCreate = dirsToCreateMap;
            }

            Opus.Core.Log.DebugMessage("Output files:");
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> outputFile in this.OutputPaths)
            {
                Opus.Core.Log.DebugMessage("{0} = {1}", outputFile.Key, outputFile.Value);
            }
            if (null != inputFiles)
            {
                Opus.Core.Log.DebugMessage("Input files:");
                foreach (string inputFile in inputFiles)
                {
                    Opus.Core.Log.DebugMessage(inputFile);
                }
            }
            else
            {
                Opus.Core.Log.DebugMessage("There were no input files");
            }
            if (null != inputVariables)
            {
                Opus.Core.Log.DebugMessage("Input variables:");
                foreach (string inputVariable in inputVariables)
                {
                    Opus.Core.Log.DebugMessage(inputVariable);
                }
            }
            else
            {
                Opus.Core.Log.DebugMessage("There were no input variables");
            }
            Opus.Core.Log.DebugMessage("Rules:");
            foreach (string irule in rules)
            {
                Opus.Core.Log.DebugMessage(irule);
            }
        }

        public void Write(System.IO.TextWriter writer, System.Enum mainOutputFileFlag)
        {
            Write(writer, mainOutputFileFlag, null);
        }

        public void Write(System.IO.TextWriter writer, System.Enum mainOutputFileFlag, System.Enum secondaryOutputFileFlag)
        {
            this.MainVariableName = System.String.Format("{0}_{1}", this.ModulePrefixName, mainOutputFileFlag.ToString());

            if (this.Includes.Count > 0)
            {
                foreach (string includePath in this.Includes)
                {
                    writer.WriteLine("include {0}", includePath);
                }
                writer.WriteLine("");
            }

            if (null != this.DirectoriesToCreate)
            {
                writer.WriteLine("# Define directories to create");
                string linearizedDirsToCreate = null;
                foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
                {
                    writer.WriteLine("{0} = {1}", dirToCreate.Key, dirToCreate.Value);
                    linearizedDirsToCreate += System.String.Format("$({0}) ", dirToCreate.Key);
                }
                writer.WriteLine("dirstomake += {0}", linearizedDirsToCreate);
                writer.WriteLine("");
            }

            writer.WriteLine("# Outputs as variables");
            string relativeOutputPath = Opus.Core.RelativePathUtilities.GetPath(this.OutputPaths[mainOutputFileFlag], this.TopLevelMakeFilePath, "$(CURDIR)");
            string variableName = System.String.Format("{0}_{1}", this.ModulePrefixName, mainOutputFileFlag);
            writer.WriteLine("{0} := {1}", variableName, relativeOutputPath);
            if (null != this.DirectoriesToCreate)
            {
                writer.WriteLine("# Order-only dependencies on directories to create");
                foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
                {
                    writer.WriteLine("$({0}): | $({1})", variableName, dirToCreate.Key);
                }
            }
            writer.WriteLine("");

            if (null != secondaryOutputFileFlag)
            {
                string altRelativeOutputPath = Opus.Core.RelativePathUtilities.GetPath(this.OutputPaths[secondaryOutputFileFlag], this.TopLevelMakeFilePath, "$(CURDIR)");
                if (altRelativeOutputPath != relativeOutputPath)
                {
                    string altVariableName = System.String.Format("{0}_{1}", this.ModulePrefixName, secondaryOutputFileFlag.ToString());
                    writer.WriteLine("{0} := {1}", altVariableName, altRelativeOutputPath);
                    writer.WriteLine("# add dependency on main output");
                    writer.WriteLine("$({0}): $({1})", altVariableName, this.MainVariableName);
                    this.SecondaryVariableName = altVariableName;
                    writer.WriteLine("");
                }
            }

            if (null != this.TargetName)
            {
                if (null == this.MainVariableName)
                {
                    throw new Opus.Core.Exception("Oops, no make variable");
                }

                writer.WriteLine("# Target so that this output can be created");
                writer.WriteLine("{0}: $({1})", this.TargetName, this.MainVariableName);
                writer.WriteLine("");
            }

            writer.WriteLine("# Rules");
            string prerequisites = null;
            if (null != this.InputFiles)
            {
                foreach (string inputFile in this.InputFiles)
                {
                    string relativeInputPath = Opus.Core.RelativePathUtilities.GetPath(inputFile, this.TopLevelMakeFilePath, "$(CURDIR)");
                    prerequisites += System.String.Format("{0} ", relativeInputPath);
                }
            }
            if (null != this.InputVariables)
            {
                foreach (string inputVariable in this.InputVariables)
                {
                    prerequisites += System.String.Format("$({0}) ", inputVariable);
                }
            }
            if (null != this.MainVariableName)
            {
                writer.WriteLine("$({0}): {1}", this.MainVariableName, prerequisites);
            }
            else
            {
                writer.WriteLine("{0}: {1}", this.OutputPaths[mainOutputFileFlag], prerequisites);
            }
            foreach (string rule in this.Rules)
            {
                string refactoredRule = rule;

                refactoredRule = refactoredRule.Replace(this.OutputPaths[mainOutputFileFlag], "$@");
                if (null != this.InputFiles)
                {
                    string inputFile = this.InputFiles[0];
                    string quotedInputFile = System.String.Format("\"{0}\"", inputFile);
                    if (refactoredRule.Contains(quotedInputFile))
                    {
                        string fileExtension = System.IO.Path.GetExtension(inputFile);
                        refactoredRule = refactoredRule.Replace(quotedInputFile, System.String.Format("$(filter %{0},$^)", fileExtension));
                    }
                }
                else if (null != this.InputVariables)
                {
                    refactoredRule = refactoredRule.Replace(this.InputVariables[0], "$<");
                }

                writer.WriteLine("\t{0}", refactoredRule);
            }
            writer.WriteLine("");

            string dirsToClean = null;
            foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
            {
                dirsToClean += System.String.Format("$({0}) ", dirToCreate.Key);
            }
            writer.WriteLine("dirstodelete += {0}", dirsToClean);
            writer.WriteLine("");
        }
    }
}
