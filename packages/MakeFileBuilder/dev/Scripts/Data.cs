// <copyright file="Data.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileData
    {
#if false
        // TODO: instead of Target and Variable, define an enum to class the data
        public MakeFileData(string file,
                            string target,
                            string variable,
                            Opus.Core.StringArray environmentPaths)
        {
            this.MakeFilePath = file;
            this.Target = target;
            this.Variable = variable;
            this.Included = false;
            this.EnvironmentPaths = environmentPaths;
        }
#endif

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

#if true
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
#else
        public string Target
        {
            get;
            private set;
        }

        public string Variable
        {
            get;
            private set;
        }
#endif

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