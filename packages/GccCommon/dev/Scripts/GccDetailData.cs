// <copyright file="GccDetailData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public class GccDetailData
    {
        public GccDetailData(string version,
                             string gxxIncludePath,
                             string target)
        {
            if (null == version)
            {
                throw new Opus.Core.Exception("Unable to determine Gcc version", false);
            }
            if (null == target)
            {
                throw new Opus.Core.Exception("Unable to determine Gcc target", false);
            }

            this.Version = version;
            this.GxxIncludePath = gxxIncludePath;
            this.Target = target;
        }

        public string Version
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
    }
}
