// <copyright file="RequiredModulesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RequiredModulesAttribute : System.Attribute, ITargetFilters
    {
        public RequiredModulesAttribute()
        {
            this.TargetFilters = new string[] { ".*-.*-.*" };
        }

        public RequiredModulesAttribute(string filter)
        {
            this.TargetFilters = new string[] { filter };
        }

        public RequiredModulesAttribute(string[] filters)
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