// <copyright file="DependentModulesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class DependentModulesAttribute : BaseTargetFilteredAttribute//System.Attribute, ITargetFilters
    {
#if false
        public DependentModulesAttribute()
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