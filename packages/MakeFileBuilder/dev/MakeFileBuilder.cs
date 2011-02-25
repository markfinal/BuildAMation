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

    public sealed class MakeFileTargetDictionary : System.Collections.Generic.Dictionary<System.Enum, Opus.Core.StringArray>
    {
        public void Append(MakeFileTargetDictionary dictionary)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }

        public new void Add(System.Enum key, Opus.Core.StringArray value)
        {
            if (this.ContainsKey(key))
            {
                this[key].AddRange(value);
            }
            else
            {
                base.Add(key, value);
            }
        }

        public void Add(System.Enum key, string value)
        {
            if (this.ContainsKey(key))
            {
                this[key].Add(value);
            }
            else
            {
                base.Add(key, new Opus.Core.StringArray(value));
            }
        }
    }

    public sealed class MakeFileVariableDictionary : System.Collections.Generic.Dictionary<System.Enum, Opus.Core.StringArray>
    {
        public void Append(MakeFileVariableDictionary dictionary)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }

        public new void Add(System.Enum key, Opus.Core.StringArray value)
        {
            if (this.ContainsKey(key))
            {
                this[key].AddRange(value);
            }
            else
            {
                base.Add(key, value);
            }
        }

        public Opus.Core.StringArray Variables
        {
            get
            {
                Opus.Core.StringArray variables = new Opus.Core.StringArray();
                foreach (Opus.Core.StringArray item in this.Values)
                {
                    variables.AddRange(item);
                }

                return variables;
            }
        }

        public MakeFileVariableDictionary Filter(System.Enum filterKeys)
        {
            int filterKeysAsInt = System.Convert.ToInt32(filterKeys);

            MakeFileVariableDictionary filtered = new MakeFileVariableDictionary();
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> pair in this)
            {
                int pairKeyAsInt = System.Convert.ToInt32(pair.Key);

                if (pairKeyAsInt == (filterKeysAsInt & pairKeyAsInt))
                {
                    filtered.Add(pair.Key, pair.Value);
                }
            }

            if (0 == filtered.Count)
            {
                throw new Opus.Core.Exception(System.String.Format("No matching variable types were found for '{0}'", filterKeys.ToString()), false);
            }

            return filtered;
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

        public static string InstanceName(Opus.Core.DependencyNode node)
        {
            string instanceName = System.String.Format("{0}_{1}", node.UniqueModuleName, node.Target.ToString());
            return instanceName;
        }

        public MakeFile(Opus.Core.DependencyNode node,
                        string topLevelMakeFilePath)
        {
            this.TopLevelMakeFilePath = topLevelMakeFilePath;
            this.ExportedTargets = new MakeFileTargetDictionary();
            this.ExportedVariables = new MakeFileVariableDictionary();
            this.RuleArray = new Opus.Core.Array<MakeFileRule>();
            this.ModulePrefixName = InstanceName(node);
        }

        public void Write(System.IO.TextWriter writer)
        {
            if (0 == this.RuleArray.Count)
            {
                return;
            }

            int ruleCount = this.RuleArray.Count;
            int ruleIndex = 0;
            foreach (MakeFileRule rule in this.RuleArray)
            {
                string mainVariableName;
                if (ruleCount > 1)
                {
                    mainVariableName = System.String.Format("{0}{1}", this.ModulePrefixName, ruleIndex++);
                }
                else
                {
                    mainVariableName = this.ModulePrefixName;
                }

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

                string mainExportVariableName = null;
                if (rule.ExportVariable)
                {
                    if (null == rule.OutputPaths)
                    {
                        writer.WriteLine("# Output variable (collection)");
                        string exportVariableName = System.String.Format("{0}_{1}_Variable", mainVariableName, rule.PrimaryOutputType.ToString());
                        mainExportVariableName = exportVariableName;

                        System.Text.StringBuilder variableAndValue = new System.Text.StringBuilder();
                        variableAndValue.AppendFormat("{0} := ", exportVariableName);
                        foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> prerequisite in rule.InputVariables)
                        {
                            foreach (string pre in prerequisite.Value)
                            {
                                variableAndValue.AppendFormat("$({0}) ", pre);
                            }
                        }
                        if (null != rule.InputFiles)
                        {
                            foreach (string prerequisiteFile in rule.InputFiles)
                            {
                                string relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                            }
                        }

                        writer.WriteLine(variableAndValue.ToString());
                        writer.WriteLine("");

                        Opus.Core.StringArray exportedVariables = new Opus.Core.StringArray();
                        exportedVariables.Add(exportVariableName);
                        this.ExportedVariables.Add(rule.PrimaryOutputType, exportedVariables);
                    }
                    else
                    {
                        foreach (System.Enum outputType in rule.OutputPaths.Types)
                        {
                            string exportVariableName = System.String.Format("{0}_{1}_Variable", mainVariableName, outputType.ToString());
                            if (outputType.Equals(rule.PrimaryOutputType))
                            {
                                mainExportVariableName = exportVariableName;
                            }
                            writer.WriteLine("# Output variable: '{0}'", outputType.ToString());

                            if (rule.ExportTarget)
                            {
                                string relativeOutputPath = this.RelativePath(rule.OutputPaths[outputType]);
                                writer.WriteLine(exportVariableName + " := " + relativeOutputPath);
                                writer.WriteLine("");
                            }
                            else
                            {
                                System.Text.StringBuilder variableAndValue = new System.Text.StringBuilder();
                                variableAndValue.AppendFormat("{0} := ", exportVariableName);
                                foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> prerequisite in rule.InputVariables)
                                {
                                    foreach (string pre in prerequisite.Value)
                                    {
                                        variableAndValue.AppendFormat("$({0}) ", pre);
                                    }
                                }
                                if (null != rule.InputFiles)
                                {
                                    foreach (string prerequisiteFile in rule.InputFiles)
                                    {
                                        string relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                                        variableAndValue.AppendFormat("{0} ", relativePrerequisiteFile);
                                    }
                                }

                                writer.WriteLine(variableAndValue.ToString());
                                writer.WriteLine("");
                            }

                            Opus.Core.StringArray exportedVariables = new Opus.Core.StringArray();
                            exportedVariables.Add(exportVariableName);
                            this.ExportedVariables.Add(outputType, exportedVariables);

                            if (null != outputDirectoriesVariable)
                            {
                                writer.WriteLine("# Order-only dependencies on directories to create");
                                writer.WriteLine("$({0}): | $({1})", exportVariableName, outputDirectoriesVariable);
                                writer.WriteLine("");
                            }
                        }
                    }
                }

                if (rule.ExportTarget && null != mainExportVariableName)
                {
                    string exportTargetName = System.String.Format("{0}_{1}_Target", mainVariableName, rule.PrimaryOutputType.ToString());
                    writer.WriteLine("# Output target");
                    if (rule.TargetIsPhony)
                    {
                        writer.WriteLine(".PHONY: {0}", exportTargetName);
                    }
                    writer.WriteLine(exportTargetName + ": $(" + mainExportVariableName + ")");
                    writer.WriteLine("");
                    this.ExportedTargets.Add(rule.PrimaryOutputType, exportTargetName);

                    if (null == rule.Recipes)
                    {
                        continue;
                    }

                    System.Text.StringBuilder targetAndPrerequisites = new System.Text.StringBuilder();
                    if (null != mainExportVariableName)
                    {
                        targetAndPrerequisites.AppendFormat("$({0}): ", mainExportVariableName);
                    }
                    else
                    {
                        string targetName = System.String.Format("{0}_{1}_Target", mainVariableName, rule.PrimaryOutputType.ToString());
                        if (rule.TargetIsPhony)
                        {
                            writer.WriteLine(".PHONY: {0}", targetName);
                        }
                        targetAndPrerequisites.AppendFormat("{0}: ", targetName);
                        if (rule.ExportTarget)
                        {
                            this.ExportedTargets.Add(rule.PrimaryOutputType, targetName);
                        }
                    }
                    foreach (System.Collections.Generic.KeyValuePair<System.Enum, Opus.Core.StringArray> prerequisite in rule.InputVariables)
                    {
                        foreach (string pre in prerequisite.Value)
                        {
                            targetAndPrerequisites.AppendFormat("$({0}) ", pre);
                        }
                    }
                    if (null != rule.InputFiles)
                    {
                        foreach (string prerequisiteFile in rule.InputFiles)
                        {
                            string relativePrerequisiteFile = this.RelativePath(prerequisiteFile);
                            targetAndPrerequisites.AppendFormat("{0} ", relativePrerequisiteFile);
                        }
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
        }
    }
}
