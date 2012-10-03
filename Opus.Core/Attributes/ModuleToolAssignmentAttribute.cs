// <copyright file="ModuleToolAssignmentAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class ModuleToolAssignmentAttribute : System.Attribute
    {
        public ModuleToolAssignmentAttribute(System.Type toolchainType)
        {
            this.ToolchainType = toolchainType;
        }

        public System.Type ToolchainType
        {
            get;
            private set;
        }
    }

}
