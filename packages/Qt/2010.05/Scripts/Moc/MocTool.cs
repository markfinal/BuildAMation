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
            string QtBinPath = Qt.BinPath;
            string mocExePath = System.IO.Path.Combine(QtBinPath, "moc.exe");
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