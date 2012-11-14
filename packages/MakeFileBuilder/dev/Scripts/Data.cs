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
            this.Environment = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
        }

        public MakeFileData(string makeFilePath,
                            MakeFileTargetDictionary targetDictionary,
                            MakeFileVariableDictionary variableDictionary,
                            Opus.Core.StringArray environmentPaths, // TODO: redundant
                            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment)
        {
            this.MakeFilePath = makeFilePath;
            this.TargetDictionary = targetDictionary;
            this.VariableDictionary = variableDictionary;
            this.EnvironmentPaths = null;
            // TODO: better way to do a copy?
            if (null != environment)
            {
                this.Environment = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
                foreach (string key in environment.Keys)
                {
                    this.Environment[key] = environment[key];
                }
            }
            else
            {
                this.Environment = null;
            }
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

        public Opus.Core.StringArray EnvironmentPaths
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Environment
        {
            get;
            private set;
        }
    }
}