// <copyright file="CopyFileLocationsAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class CopyFileLocationsAttribute :
        Opus.Core.BaseTargetFilteredAttribute,
        IPublishBaseAttribute
    {
        public
        CopyFileLocationsAttribute() : this(string.Empty)
        {}

        public CopyFileLocationsAttribute(
            string commonSubDirectory)
        {
            this.CommonSubDirectory = commonSubDirectory;
        }

        public string CommonSubDirectory
        {
            get;
            private set;
        }
    }
}
