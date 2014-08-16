// <copyright file="IPublishProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    static class ToolsetProvider
    {
        static string GetToolsetName(System.Type toolType)
        {
            return "Publish";
        }
    }

    public class ExportPublishOptionsDelegateAttribute :
        System.Attribute
    {}

    public class LocalPublishOptionsDelegateAttribute :
        System.Attribute
    {}

    [Bam.Core.LocalAndExportTypes(typeof(LocalPublishOptionsDelegateAttribute),
                                   typeof(ExportPublishOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetToolsetName")]
    public interface IPublishProductTool
    {}
}
