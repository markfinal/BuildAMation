// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public sealed class Toolset : Opus.Core.IToolset
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

        bool Opus.Core.IToolset.HasTool(System.Type toolType)
        {
            return false;
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
            return typeof(OptionSet);
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return string.Empty;
        }

        #endregion
    }
}
