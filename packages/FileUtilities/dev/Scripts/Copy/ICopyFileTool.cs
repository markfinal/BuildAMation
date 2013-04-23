namespace FileUtilities
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalOptionsDelegateAttribute : System.Attribute
    {
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalOptionsDelegateAttribute), typeof(ExportOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("FileUtilities")]
    public interface ICopyFileTool : Opus.Core.ITool
    {
    }
}
