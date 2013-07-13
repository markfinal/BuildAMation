// <copyright file="BesideModuleAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class BesideModuleAttribute : Opus.Core.BaseTargetFilteredAttribute
    {
        public BesideModuleAttribute(object flag)
            : this(flag, string.Empty)
        {
        }

        public BesideModuleAttribute(object flag, string relativePath)
        {
            if (!flag.GetType().IsEnum)
            {
                throw new Opus.Core.Exception("Object is not of enum type");
            }

            this.OutputFileFlag = flag as System.Enum;
            this.RelativePath = relativePath;
        }

        public System.Enum OutputFileFlag
        {
            get;
            private set;
        }

        public string RelativePath
        {
            get;
            private set;
        }
    }
}
