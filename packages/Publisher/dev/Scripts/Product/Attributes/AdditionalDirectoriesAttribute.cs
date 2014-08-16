// <copyright file="AdditionalDirectoriesAttribute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class AdditionalDirectoriesAttribute :
        Bam.Core.BaseTargetFilteredAttribute,
        IPublishBaseAttribute
    {
        public
        AdditionalDirectoriesAttribute() : this(string.Empty)
        {}

        public AdditionalDirectoriesAttribute(
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
