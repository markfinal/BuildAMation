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
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePathName);
            return makeFilePathName;
        }
    }

    public sealed class MakeFileTargetDictionary : System.Collections.Generic.Dictionary<System.Enum, string>
    {
        public void Append(MakeFileTargetDictionary dictionary)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }
    }

    public sealed class MakeFileVariableDictionary : System.Collections.Generic.Dictionary<System.Enum, string>
    {
        public void Append(MakeFileVariableDictionary dictionary)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }
    }

    public sealed class MakeFile
    {
        private string RelativePath(string path)
        {
            string relativeDir = Opus.Core.RelativePathUtilities.GetPath(path, this.TopLevelMakeFilePath, "$(CURDIR)");
            return relativeDir;
        }

        public MakeFileTargetDictionary ExportedTargets
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary ExportedVariables
        {
            get;
            private set;
        }

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

#if false
        private Opus.Core.StringArray Rules
        {
            get;
            set;
        }
#endif

#if false
        private System.Collections.Generic.Dictionary<string, string> DirectoriesToCreate
        {
            get;
            set;
        }
#endif

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

        public Opus.Core.Array<MakeFileRule> RuleArray
        {
            get;
            private set;
        }

#if true
        public MakeFile(Opus.Core.DependencyNode node,
                                     string topLevelMakeFilePath)
        {
            this.TopLevelMakeFilePath = topLevelMakeFilePath;
            this.ExportedTargets = new MakeFileTargetDictionary();
            this.ExportedVariables = new MakeFileVariableDictionary();
            this.RuleArray = new Opus.Core.Array<MakeFileRule>();
            this.OutputPaths = node.Module.Options.OutputPaths;
            this.ModulePrefixName = System.String.Format("{0}_{1}", node.UniqueModuleName, node.Target.ToString());

#if false
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
#endif
        }
#else
        public MakeFile(Opus.Core.DependencyNode node,
                                     Opus.Core.StringArray inputFiles,
                                     Opus.Core.StringArray inputVariables,
                                     Opus.Core.StringArray rules,
                                     string topLevelMakeFilePath)
        {
            this.ExportedTargets = new Opus.Core.StringArray();
            this.RuleArray = new Opus.Core.Array<MakeFileRule>();
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
            if (rules != null)
            {
                Opus.Core.Log.DebugMessage("Rules:");
                foreach (string irule in rules)
                {
                    Opus.Core.Log.DebugMessage(irule);
                }
            }
        }
#endif

        public void Write(System.IO.TextWriter writer)
        {
            if (0 == this.RuleArray.Count)
            {
                return;
            }

#if false
            if (this.Includes.Count > 0)
            {
                foreach (string includePath in this.Includes)
                {
                    writer.WriteLine("include {0}", includePath);
                }
                writer.WriteLine("");
            }
#endif

            string mainVariableName = this.ModulePrefixName;
            foreach (MakeFileRule rule in this.RuleArray)
            {
                string outputDirectoriesVariable = null;
                if (null != rule.DirectoriesToCreate)
                {
                    writer.WriteLine("# Define directories to create");
                    string linearizedDirsToCreate = null;
                    foreach (string dir in rule.DirectoriesToCreate)
                    {
                        string relativeDir = this.RelativePath(dir);
                        linearizedDirsToCreate += relativeDir + " ";
                    }
                    outputDirectoriesVariable = System.String.Format("{0}_BuildDirs", mainVariableName);
                    writer.WriteLine("{0} := {1}", outputDirectoriesVariable, linearizedDirsToCreate);
                    writer.WriteLine("builddirs += $({0})", outputDirectoriesVariable);
                    writer.WriteLine("");
                }

                string exportVariableName = System.String.Format("{0}_Variable", mainVariableName);
                writer.WriteLine("# Output variable");
                string relativeOutputPath = this.RelativePath(this.OutputPaths[rule.TargetType]);
                writer.WriteLine(exportVariableName + " := " + relativeOutputPath);
                writer.WriteLine("");
                this.ExportedVariables.Add(rule.TargetType, exportVariableName);

                if (null != outputDirectoriesVariable)
                {
                    writer.WriteLine("# Order-only dependencies on directories to create");
                    writer.WriteLine("$({0}): | $({1})", exportVariableName, outputDirectoriesVariable);
                    writer.WriteLine("");
                }

                string exportTargetName = System.String.Format("{0}_Target", mainVariableName);
                writer.WriteLine("# Output target");
                writer.WriteLine(exportTargetName + ": $(" + exportVariableName + ")");
                writer.WriteLine("");
                this.ExportedTargets.Add(rule.TargetType, exportTargetName);

                System.Text.StringBuilder targetAndPrerequisites = new System.Text.StringBuilder();
                targetAndPrerequisites.AppendFormat("$({0}): ", exportVariableName);
                foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> prerequisite in rule.InputVariables)
                {
                    targetAndPrerequisites.AppendFormat("$({0}) ", prerequisite.Value);
                }
                foreach (string prerequisiteFile in rule.InputFiles)
                {
                    string relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                    targetAndPrerequisites.AppendFormat("{0} ", relativePrerequisiteFile);
                }
                writer.WriteLine("# Rule");
                writer.WriteLine(targetAndPrerequisites.ToString());
                foreach (string recipe in rule.Recipes)
                {
                    writer.WriteLine("\t" + recipe);
                }
                writer.WriteLine("");
            }
        }

        public void Write(System.IO.TextWriter writer, System.Enum mainOutputFileFlag)
        {
            Write(writer, mainOutputFileFlag, null);
        }

        public void Write(System.IO.TextWriter writer, System.Enum mainOutputFileFlag, System.Enum secondaryOutputFileFlag)
        {
            this.MainVariableName = System.String.Format("{0}_{1}", this.ModulePrefixName, mainOutputFileFlag.ToString());

#if false
            if (this.Includes.Count > 0)
            {
                foreach (string includePath in this.Includes)
                {
                    writer.WriteLine("include {0}", includePath);
                }
                writer.WriteLine("");
            }
#endif

#if false
            if (null != this.DirectoriesToCreate)
            {
                writer.WriteLine("# Define directories to create");
                string linearizedDirsToCreate = null;
                foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
                {
                    writer.WriteLine("{0} = {1}", dirToCreate.Key, dirToCreate.Value);
                    linearizedDirsToCreate += System.String.Format("$({0}) ", dirToCreate.Key);
                }
                writer.WriteLine("builddirs += {0}", linearizedDirsToCreate);
                writer.WriteLine("");
            }
#endif

            writer.WriteLine("# Outputs as variables");
            string relativeOutputPath = Opus.Core.RelativePathUtilities.GetPath(this.OutputPaths[mainOutputFileFlag], this.TopLevelMakeFilePath, "$(CURDIR)");
            string variableName = System.String.Format("{0}_{1}", this.ModulePrefixName, mainOutputFileFlag);
            writer.WriteLine("{0} := {1}", variableName, relativeOutputPath);
#if false
            if (null != this.DirectoriesToCreate)
            {
                writer.WriteLine("# Order-only dependencies on directories to create");
                foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
                {
                    writer.WriteLine("$({0}): | $({1})", variableName, dirToCreate.Key);
                }
            }
#endif
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

#if true
            string mainVariableName = this.ModulePrefixName;
            foreach (MakeFileRule rule in this.RuleArray)
            {
                string exportVariableName = System.String.Format("{0}_GOALVARIABLE", mainVariableName);
                writer.WriteLine("# Goal variable");
                writer.WriteLine(exportVariableName + " := " + rule.Target);

                string exportTargetName = System.String.Format("{0}_GOALTARGET", mainVariableName);
                writer.WriteLine("# Goal target");
                writer.WriteLine(exportTargetName + ": $(" + exportVariableName + ")");
                this.ExportedTargets.Add(rule.TargetType, exportTargetName);

                System.Text.StringBuilder targetAndPrerequisites = new System.Text.StringBuilder();
                targetAndPrerequisites.AppendFormat("$({0}): ", exportVariableName);
                foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> prerequisite in rule.InputVariables)
                {
                    targetAndPrerequisites.AppendFormat("$({0}) ", prerequisite.Value);
                }
                writer.WriteLine("# Goal Rule");
                writer.WriteLine(targetAndPrerequisites.ToString());

                foreach (string recipe in rule.Recipes)
                {
                    writer.WriteLine("\t" + recipe);
                }
            }
#else
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
#endif

#if false
            string dirsToClean = null;
            foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
            {
                dirsToClean += System.String.Format("$({0}) ", dirToCreate.Key);
            }
            writer.WriteLine("dirstodelete += {0}", dirsToClean);
            writer.WriteLine("");
#endif
        }
    }
}
