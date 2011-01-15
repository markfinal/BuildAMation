// <copyright file="ModuleTargetsAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ModuleTargetsAttribute : System.Attribute, ITargetFilters
    {
        public ModuleTargetsAttribute()
        {
            this.TargetFilters = new string[] { ".*-.*-.*" };
        }

        public ModuleTargetsAttribute(string filter)
        {
            this.TargetFilters = new string[] { filter };
        }

        public ModuleTargetsAttribute(string[] filters)
        {
            this.TargetFilters = filters;
        }

        public string[] TargetFilters
        {
            get;
            set;
        }
    }
}
