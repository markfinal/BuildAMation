// <copyright file="MakeFileRule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileRule
    {
        public MakeFileRule(Opus.Core.BaseModule moduleToBuild,
                            Opus.Core.LocationKey primaryOutputLocationKey,
                            string target,
                            Opus.Core.LocationArray directoriesToCreate,
                            MakeFileVariableDictionary inputVariables,
                            Opus.Core.StringArray inputFiles,
                            Opus.Core.StringArray recipes)
        {
#if true
            this.ModuleToBuild = moduleToBuild;
            this.PrimaryOutputLocationKey = primaryOutputLocationKey;
#else
            this.OutputPaths = outputPaths;
            this.PrimaryOutputType = primaryOutputType;
#endif
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = inputVariables;
            this.InputFiles = inputFiles;
            this.Recipes = recipes;
            this.ExportTarget = true;
            this.ExportVariable = true;
            this.TargetIsPhony = false;
        }

#if true
        public Opus.Core.BaseModule ModuleToBuild
        {
            get;
            private set;
        }

        public Opus.Core.LocationKey PrimaryOutputLocationKey
        {
            get;
            private set;
        }
#else
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
#endif

        public string Target
        {
            get;
            private set;
        }

        public Opus.Core.LocationArray DirectoriesToCreate
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

        public Opus.Core.Array<Opus.Core.LocationKey> OutputLocationKeys
        {
            get;
            set;
        }
    }
}
