// <copyright file="GccDetailData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public class GccDetailData
    {
        public GccDetailData(string version,
                             Opus.Core.StringArray includePaths,
                             string gxxIncludePath,
                             string target,
                             string libExecDir)
        {
            if (null == version)
            {
                throw new Opus.Core.Exception("Unable to determine Gcc version");
            }
            if (null == target)
            {
                throw new Opus.Core.Exception("Unable to determine Gcc target");
            }

            this.Version = version;
            this.IncludePaths = includePaths;
            this.GxxIncludePath = gxxIncludePath;
            this.Target = target;
            this.LibExecDir = libExecDir;
        }

        public string Version
        {
            get;
            private set;
        }

        public Opus.Core.StringArray IncludePaths
        {
            get;
            private set;
        }

        public string GxxIncludePath
        {
            get;
            private set;
        }

        public string Target
        {
            get;
            private set;
        }

        public string LibExecDir
        {
            get;
            private set;
        }
    }
}
