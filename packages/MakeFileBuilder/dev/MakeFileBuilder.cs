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
        private System.Collections.Generic.Dictionary<string, string> OutputFiles
        {
            get;
            set;
        }

        private Opus.Core.StringArray InputFiles
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
                                     Opus.Core.StringArray rules,
                                     string topLevelMakeFilePath)
        {
            this.TopLevelMakeFilePath = topLevelMakeFilePath;
            this.Includes = new Opus.Core.StringArray();

            Opus.Core.IOutputPaths outputFiles = node.Module.Options as Opus.Core.IOutputPaths;
            this.OutputFiles = outputFiles.GetOutputPaths();
            this.InputFiles = inputFiles;
            this.Rules = rules;

            this.ModulePrefixName = System.String.Format("{0}_{1}", node.UniqueModuleName, node.Target.ToString());
            if (null == node.Parent)
            {
                this.TargetName = this.ModulePrefixName;
            }

            CommandLineProcessor.ICommandLineSupport commandLineOption = node.Module.Options as CommandLineProcessor.ICommandLineSupport;
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

            Opus.Core.Log.MessageAll("Output files:");
            foreach (System.Collections.Generic.KeyValuePair<string, string> outputFile in this.OutputFiles)
            {
                Opus.Core.Log.MessageAll("{0} = {1}", outputFile.Key, outputFile.Value);
            }
            Opus.Core.Log.MessageAll("Input files:");
            foreach (string inputFile in inputFiles)
            {
                Opus.Core.Log.MessageAll(inputFile);
            }
            Opus.Core.Log.MessageAll("Rules:");
            foreach (string irule in rules)
            {
                Opus.Core.Log.MessageAll(irule);
            }
        }

        public void Write(System.IO.TextWriter writer, string mainOutputFile)
        {
            Write(writer, mainOutputFile, null);
        }

        public void Write(System.IO.TextWriter writer, string mainOutputFile, string secondaryOutputFile)
        {
            this.MainVariableName = System.String.Format("{0}_{1}", this.ModulePrefixName, mainOutputFile);

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
            string relativeOutputPath = Opus.Core.RelativePathUtilities.GetPath(this.OutputFiles[mainOutputFile], this.TopLevelMakeFilePath, "$(CURDIR)");
            string variableName = System.String.Format("{0}_{1}", this.ModulePrefixName, mainOutputFile);
            writer.WriteLine("{0} := {1}", variableName, relativeOutputPath);
            if (null != this.DirectoriesToCreate)
            {
                writer.WriteLine("# Dependencies on directories to create");
                foreach (System.Collections.Generic.KeyValuePair<string, string> dirToCreate in this.DirectoriesToCreate)
                {
                    writer.WriteLine("$({0}): $({1})", variableName, dirToCreate.Key);
                }
            }
            writer.WriteLine("");

            if (null != secondaryOutputFile)
            {
                string altRelativeOutputPath = Opus.Core.RelativePathUtilities.GetPath(this.OutputFiles[secondaryOutputFile], this.TopLevelMakeFilePath, "$(CURDIR)");
                if (altRelativeOutputPath != relativeOutputPath)
                {
                    string altVariableName = System.String.Format("{0}_{1}", this.ModulePrefixName, secondaryOutputFile);
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
            foreach (string inputFile in this.InputFiles)
            {
                if (System.IO.File.Exists(inputFile))
                {
                    string relativeInputPath = Opus.Core.RelativePathUtilities.GetPath(inputFile, this.TopLevelMakeFilePath, "$(CURDIR)");
                    prerequisites += System.String.Format("{0} ", relativeInputPath);
                }
                else
                {
                    prerequisites += System.String.Format("$({0}) ", inputFile);
                }
            }
            if (null != this.MainVariableName)
            {
                writer.WriteLine("$({0}): {1}", this.MainVariableName, prerequisites);
            }
            else
            {
                writer.WriteLine("{0}: {1}", this.OutputFiles[mainOutputFile], prerequisites);
            }
            foreach (string rule in this.Rules)
            {
                string refactoredRule = rule;

                refactoredRule = refactoredRule.Replace(this.OutputFiles[mainOutputFile], "$@");
                refactoredRule = refactoredRule.Replace(this.InputFiles[0], "$<");

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
