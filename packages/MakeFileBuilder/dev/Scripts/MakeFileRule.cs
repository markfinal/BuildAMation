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
        public MakeFileRule(string target,
                            Opus.Core.StringArray prerequisiteVariables)
        {
            this.Target = target;
            this.InputFiles = prerequisiteVariables;
        }

        public MakeFileRule(System.Enum targetType,
                            string target,
                            Opus.Core.DirectoryCollection directoriesToCreate,
                            Opus.Core.StringArray inputFiles,
                            Opus.Core.StringArray recipes)
        {
            this.TargetType = targetType;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = new MakeFileVariableDictionary();
            this.InputFiles = inputFiles;
            this.Recipes = recipes;
        }

        public MakeFileRule(System.Enum targetType,
                            string target,
                            Opus.Core.DirectoryCollection directoriesToCreate,
                            MakeFileVariableDictionary inputVariables,
                            Opus.Core.StringArray recipes)
        {
            this.TargetType = targetType;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = inputVariables;
            this.Recipes = recipes;
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
    }
}
