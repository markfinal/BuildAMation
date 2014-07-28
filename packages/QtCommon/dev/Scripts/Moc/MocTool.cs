// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public sealed class MocTool :
        IMocTool
    {
        private Opus.Core.IToolset toolset;

        public
        MocTool(
            Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string
        Opus.Core.ITool.Executable(
            Opus.Core.BaseTarget baseTarget)
        {
            var mocExePath = System.IO.Path.Combine(this.toolset.BinPath(baseTarget), "moc");
            if (baseTarget.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                mocExePath += ".exe";
            }

            return mocExePath;
        }

        Opus.Core.Array<Opus.Core.LocationKey>
        Opus.Core.ITool.OutputLocationKeys(
            Opus.Core.BaseModule module)
        {
            var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                MocFile.OutputFile,
                MocFile.OutputDir
                );
            return array;
        }

        #endregion
    }
}
