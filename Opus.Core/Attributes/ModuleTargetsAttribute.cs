// <copyright file="ModuleTargetsAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ModuleTargetsAttribute : BaseTargetFilteredAttribute//System.Attribute, ITargetFilters
    {
#if false
        public ModuleTargetsAttribute()
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
