// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public sealed class Toolset :
        Bam.Core.IToolset
    {
        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        bool
        Bam.Core.IToolset.HasTool(
            System.Type toolType)
        {
            return false;
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Bam.Core.ITool
        Bam.Core.IToolset.Tool(
            System.Type toolType)
        {
            throw new System.NotImplementedException();
        }

        System.Type
        Bam.Core.IToolset.ToolOptionType(
            System.Type toolType)
        {
            return typeof(OptionSet);
        }

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return string.Empty;
        }

        #endregion
    }
}
