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
                            System.Enum targetType,
                            string target,
                            MakeFileVariableDictionary prerequisiteVariables,
                            Opus.Core.StringArray recipes)
        {
            this.OutputPaths = outputPaths;
            this.TargetType = targetType;
            this.Target = target;
            this.InputVariables = prerequisiteVariables;
            this.Recipes = recipes;
        }

        public MakeFileRule(Opus.Core.OutputPaths outputPaths, 
                            System.Enum targetType,
                            string target,
                            Opus.Core.DirectoryCollection directoriesToCreate,
                            Opus.Core.StringArray inputFiles,
                            Opus.Core.StringArray recipes)
        {
            this.OutputPaths = outputPaths;
            this.TargetType = targetType;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = new MakeFileVariableDictionary();
            this.InputFiles = inputFiles;
            this.Recipes = recipes;
            this.ExportTarget = true;
            this.ExportVariable = true;
        }

        public MakeFileRule(Opus.Core.OutputPaths outputPaths, 
                            System.Enum targetType,
                            string target,
                            Opus.Core.DirectoryCollection directoriesToCreate,
                            MakeFileVariableDictionary inputVariables,
                            Opus.Core.StringArray recipes)
        {
            this.OutputPaths = outputPaths;
            this.TargetType = targetType;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = inputVariables;
            this.Recipes = recipes;
            this.ExportTarget = true;
            this.ExportVariable = true;
        }

        public Opus.Core.OutputPaths OutputPaths
        {
            get;
            private set;
        }

        public System.Enum TargetType
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
