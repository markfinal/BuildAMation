[assembly:Opus.Core.RegisterToolset("FileUtilities", typeof(FileUtilities.Toolset))]

namespace FileUtilities
{
    class Toolset : Opus.Core.IToolset
    {
        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            throw new System.NotImplementedException();
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
