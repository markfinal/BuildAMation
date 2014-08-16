// <copyright file="ModuleGroupAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Opus.Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=false)]
    public sealed class ModuleGroupAttribute :
        System.Attribute
    {
        public
        ModuleGroupAttribute(
            string groupName)
        {
            this.GroupName = groupName;
        }

        public string GroupName
        {
            get;
            private set;
        }
    }
}