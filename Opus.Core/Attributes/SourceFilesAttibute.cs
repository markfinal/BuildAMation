// <copyright file="SourceFilesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SourceFilesAttribute : BaseTargetFilteredAttribute
    {
#if false
        public SourceFilesAttribute()
        {
            this.Platform = EPlatform.All;
            this.Configuration = EConfiguration.All;
            this.Toolchains = new string[] { ".*" };
        }

        public EPlatform Platform
        {
            get;
            set;
        }

        public EConfiguration Configuration
        {
            get;
            set;
        }

        public string[] Toolchains
        {
            get;
            set;
        }
#endif
    }
}