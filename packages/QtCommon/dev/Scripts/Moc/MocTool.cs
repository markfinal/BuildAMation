// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("moctool", "Qt.Qt.VersionString")]
[assembly: QtCommon.RegisterToolchain(typeof(QtCommon.Toolset))]

namespace QtCommon
{
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