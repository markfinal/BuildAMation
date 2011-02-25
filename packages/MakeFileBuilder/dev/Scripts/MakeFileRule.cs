// <copyright file="MakeFileRule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    // TODO: might need a base class rule, and add from therel
    public sealed class MakeFileRule
    {
        public MakeFileRule(Opus.Core.OutputPaths outputPaths,
                            System.Enum primaryOutputType,
                            string target,
                            Opus.Core.DirectoryCollection directoriesToCreate,
                            MakeFileVariableDictionary inputVariables,
                            Opus.Core.StringArray inputFiles,
                            Opus.Core.StringArray recipes)
        {
            this.OutputPaths = outputPaths;
            this.PrimaryOutputType = primaryOutputType;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = inputVariables;
            this.InputFiles = inputFiles;
            this.Recipes = recipes;
            this.ExportTarget = true;
            this.ExportVariable = true;
        }

        public Opus.Core.OutputPaths OutputPaths
        {
            get;
            private set;
        }

        public System.Enum PrimaryOutputType
        {
            get;
            private set;
        }

        public string Target
        {
            get;
            private set;
        }

        public Opus.Core.DirectoryCollection DirectoriesToCreate
        {
            get;
            private set;
        }

        public Opus.Core.StringArray InputFiles
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary InputVariables
        {
            get;
            private set;
        }

        public Opus.Core.StringArray Recipes
        {
            get;
            private set;
        }

        public bool ExportVariable
        {
            get;
            set;
        }

        public bool ExportTarget
        {
            get;
            set;
        }

        public bool TargetIsPhony
        {
            get;
            set;
        }
    }
}
