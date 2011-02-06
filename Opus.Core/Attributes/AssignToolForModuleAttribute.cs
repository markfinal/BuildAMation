// <copyright file="AssignToolForModuleAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=false)]
    public class AssignToolForModuleAttribute : System.Attribute
    {
        public AssignToolForModuleAttribute()
        {
            // do nothing - all properties are null
        }

        public AssignToolForModuleAttribute(System.Type toolType,
                                            System.Type exportAttributeType,
                                            System.Type localAttributeType,
                                            string toolOptionsClassName)
        {
            this.ToolType = toolType;
            this.ExportType = exportAttributeType;
            this.LocalType = localAttributeType;
            this.ClassName = toolOptionsClassName;
            this.OptionsType = null;
        }

        public AssignToolForModuleAttribute(System.Type toolType,
                                            System.Type exportAttributeType,
                                            System.Type localAttributeType,
                                            System.Type optionsType)
        {
            this.ToolType = toolType;
            this.ExportType = exportAttributeType;
            this.LocalType = localAttributeType;
            this.ClassName = null;
            this.OptionsType = optionsType;
        }

        public System.Type ToolType
        {
            get;
            private set;
        }

        public System.Type ExportType
        {
            get;
            private set;
        }

        public System.Type LocalType
        {
            get;
            private set;
        }

        public string ClassName
        {
            get;
            private set;
        }

        public System.Type OptionsType
        {
            get;
            private set;
        }
    }
}