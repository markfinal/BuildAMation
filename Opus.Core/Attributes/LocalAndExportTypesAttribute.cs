// <copyright file="LocalAndExportTypesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Interface)]
    public sealed class LocalAndExportTypesAttribute : System.Attribute
    {
        public LocalAndExportTypesAttribute(System.Type localType, System.Type exportType)
        {
            this.LocalType = localType;
            this.ExportType = exportType;
        }

        public System.Type LocalType
        {
            get;
            private set;
        }

        public System.Type ExportType
        {
            get;
            private set;
        }
    }
}
