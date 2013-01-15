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
            if (null == toolType)
            {
                throw new Exception("Tool type for module cannot be null");
            }

            this.ToolType = toolType;
        }

        public System.Type ToolType
        {
            get;
            private set;
        }
    }

}
