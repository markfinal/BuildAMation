// <copyright file="Data.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileData
    {
        public MakeFileData(string makeFilePath,
                            MakeFileTargetDictionary targetDictionary,
                            MakeFileVariableDictionary variableDictionary,
                            Opus.Core.StringArray environmentPaths)
        {
            this.MakeFilePath = makeFilePath;
            this.TargetDictionary = targetDictionary;
            this.VariableDictionary = variableDictionary;
            this.EnvironmentPaths = environmentPaths;
            this.Included = false;
        }

        public string MakeFilePath
        {
            get;
            private set;
        }

        public MakeFileTargetDictionary TargetDictionary
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary VariableDictionary
        {
            get;
            private set;
        }

        public bool Included
        {
            get;
            set;
        }

        public Opus.Core.StringArray EnvironmentPaths
        {
            get;
            private set;
        }
    }
}