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
        private Bam.Core.IToolset toolset;

        public
        MocTool(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var mocExePath = System.IO.Path.Combine(this.toolset.BinPath(baseTarget), "moc");
            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                mocExePath += ".exe";
            }

            return mocExePath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                MocFile.OutputFile,
                MocFile.OutputDir
                );
            return array;
        }

        #endregion
    }
}
