// <copyright file="MingwDetailData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public class MingwDetailData
    {
        public
        MingwDetailData(
            string version,
            Opus.Core.StringArray includePaths,
            string gxxIncludePath,
            string target,
            string libExecDir)
        {
            if (null == version)
            {
                throw new Opus.Core.Exception("Unable to determine Mingw version");
            }
            if (null == target)
            {
                throw new Opus.Core.Exception("Unable to determine Mingw target");
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

        // TODO: currently unused
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
