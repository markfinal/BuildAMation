namespace FileUtilities
{
    [Opus.Core.AssignToolsetProvider("FileUtilities")]
    public interface ICopyFileTool : Opus.Core.ITool
    {
    }

    class CopyFileTool : ICopyFileTool
    {
        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

    [Opus.Core.ModuleToolAssignment(typeof(CopyFileTool))]
    class CopyFile : Opus.Core.BaseModule
    {
    }
}
