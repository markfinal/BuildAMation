// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("moctool", "Qt.Qt.VersionString")]

namespace QtCommon
{
    public sealed class MocTool : Opus.Core.ITool
    {
        public string Executable(Opus.Core.Target target)
        {
            string mocExePath = System.IO.Path.Combine(QtCommon.BinPath, "moc");
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                mocExePath = mocExePath + ".exe";
            }

            return mocExePath;
        }
    }
}