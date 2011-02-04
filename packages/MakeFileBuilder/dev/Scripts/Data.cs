// <copyright file="Data.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed class MakeFileData
    {
        // TODO: instead of Target and Variable, define an enum to class the data
        public MakeFileData(string file,
                            string target,
                            string variable,
                            Opus.Core.StringArray environmentPaths)
        {
            this.File = file;
            this.Target = target;
            this.Variable = variable;
            this.Included = false;
            this.EnvironmentPaths = environmentPaths;
        }

        public string File
        {
            get;
            private set;
        }

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