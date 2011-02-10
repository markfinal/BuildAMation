// <copyright file="DestinationModuleDirectoryAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class DestinationModuleDirectoryAttribute : Opus.Core.DependentModulesAttribute
    {
        public DestinationModuleDirectoryAttribute(object flags)
            : base()
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