// <copyright file="MocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
#if false
    [Opus.Core.LocalAndExportTypes(typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(ExportMocOptionsDelegateAttribute))]
#endif
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
            // NEW STYLE
#if true
            string mocExePath = System.IO.Path.Combine(this.toolset.BinPath((Opus.Core.BaseTarget)target), "moc");
            if (target.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                mocExePath += ".exe";
            }

            return mocExePath;
#else
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
#endif
        }

        #endregion
    }
}