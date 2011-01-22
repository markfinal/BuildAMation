// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("moctool", "Qt.Qt.VersionString")]

namespace Qt
{
    public sealed class MocTool : Opus.Core.ITool
    {
        public Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        public string Executable(Opus.Core.Target target)
        {
            string mocExePath = System.IO.Path.Combine(Qt.BinPath, "moc");
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                mocExePath = mocExePath + "exe";
            }

            return mocExePath;
        }

        public Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return new Opus.Core.StringArray();
            }
        }
    }
}