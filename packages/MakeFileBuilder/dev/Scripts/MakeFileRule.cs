// <copyright file="MakeFileRule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileRule
    {
        public
        MakeFileRule(
            Bam.Core.BaseModule moduleToBuild,
            Bam.Core.LocationKey primaryOutputLocationKey,
            string target,
            Bam.Core.LocationArray directoriesToCreate,
            MakeFileVariableDictionary inputVariables,
            Bam.Core.StringArray inputFiles,
            Bam.Core.StringArray recipes)
        {
            this.ModuleToBuild = moduleToBuild;
            this.PrimaryOutputLocationKey = primaryOutputLocationKey;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = inputVariables;
            this.InputFiles = inputFiles;
            this.Recipes = recipes;
            this.ExportTarget = true;
            this.ExportVariable = true;
            this.TargetIsPhony = false;
        }

        public Bam.Core.BaseModule ModuleToBuild
        {
            get;
            private set;
        }

        public Bam.Core.LocationKey PrimaryOutputLocationKey
        {
            get;
            private set;
        }

        public string Target
        {
            get;
            private set;
        }

        public Bam.Core.LocationArray DirectoriesToCreate
        {
            get;
            private set;
        }

        public Bam.Core.StringArray InputFiles
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary InputVariables
        {
            get;
            private set;
        }

        public Bam.Core.StringArray Recipes
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

        public Bam.Core.Array<Bam.Core.LocationKey> OutputLocationKeys
        {
            get;
            set;
        }
    }
}
