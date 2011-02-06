// <copyright file="SourceModulesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class SourceModulesAttribute : System.Attribute
    {
        public SourceModulesAttribute(object flags)
        {
            if (!flags.GetType().IsEnum)
            {
                throw new Opus.Core.Exception("Object is not of enum type");
            }

            this.OutputFlags = flags as System.Enum;
        }

        public System.Enum OutputFlags
        {
            get;
            private set;
        }
    }
}