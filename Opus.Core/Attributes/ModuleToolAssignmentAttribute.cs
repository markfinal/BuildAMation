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
        public ModuleToolAssignmentAttribute(System.Type toolType)
        {
            this.ToolType = toolType;
        }

        public System.Type ToolType
        {
            get;
            private set;
        }
    }

}
