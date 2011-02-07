// <copyright file="DependentModulesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class DependentModulesAttribute : System.Attribute, ITargetFilters
    {
        public DependentModulesAttribute()
        {
            this.TargetFilters = new string[] { ".*-.*-.*" };
        }

        public DependentModulesAttribute(string filter)
        {
            this.TargetFilters = new string[] { filter };
        }

        public DependentModulesAttribute(string[] filters)
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