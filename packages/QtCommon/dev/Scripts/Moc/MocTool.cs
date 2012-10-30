// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("moctool", "Qt.Qt.VersionString")]
[assembly: QtCommon.RegisterToolchain(typeof(QtCommon.MocToolset))]

namespace QtCommon
{
    public sealed class MocToolset : Opus.Core.IToolset
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

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            // TODO:
            return "dev";
        }

        #endregion
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(ExportMocOptionsDelegateAttribute))]
    public sealed class MocTool : Opus.Core.ITool
    {
        public string Executable(Opus.Core.Target target)
        {
            Qt.Qt thirdPartyModule =
                Opus.Core.ModuleUtilities.GetModuleNoToolchain(typeof(Qt.Qt), target) as Qt.Qt;
            if (null == thirdPartyModule)
            {
                throw new Opus.Core.Exception("Cannot locate Qt module instance", false);
            }

            string mocExePath = System.IO.Path.Combine(thirdPartyModule.BinPath, "moc");
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                mocExePath = mocExePath + ".exe";
            }

            return mocExePath;
        }
    }
}