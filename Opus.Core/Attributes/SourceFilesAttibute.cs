// <copyright file="SourceFilesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SourceFilesAttribute : System.Attribute, ITargetFilters
    {
        public SourceFilesAttribute()
        {
            this.TargetFilters = new string[] { ".*-.*-.*" };
        }

        public SourceFilesAttribute(string filter)
        {
            this.TargetFilters = new string[] { filter };
        }

        public SourceFilesAttribute(string[] filters)
        {
            this.TargetFilters = filters;
        }

        public string[] TargetFilters
        {
            get;
            set;
        }
    }
}