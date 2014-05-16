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
#if true
            throw new System.NotSupportedException();
#else
            if (!flag.GetType().IsEnum)
            {
                throw new Opus.Core.Exception("Flag is not of type enum");
            }
            this.RelativePath = relativePath;
#endif
        }

        public Opus.Core.LocationKey OutputFileLocation
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
