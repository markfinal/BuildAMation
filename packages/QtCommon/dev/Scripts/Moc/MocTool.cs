// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public sealed class MocTool : IMocTool
    {
        private Opus.Core.IToolset toolset;

        public MocTool(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string mocExePath = System.IO.Path.Combine(this.toolset.BinPath((Opus.Core.BaseTarget)target), "moc");
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                mocExePath += ".exe";
            }

            return mocExePath;
        }

        #endregion
    }
}