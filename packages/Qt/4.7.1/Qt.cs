// <copyright file="Qt.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Qt package</summary>
// <author>Mark Final</author>

[assembly:Opus.Core.RegisterToolset("Qt", typeof(Qt.Toolset))]

namespace Qt
{
    public sealed class Qt : QtCommon.QtCommon
    {
        public Qt(Opus.Core.Target target)
        {
            // The target here will have a null IToolset because this module is a Thirdparty
            // module, which has no need for a tool, so no toolset is configured
            // However, we do need to know where Qt was installed, which is in the Toolset
            // so just grab the instance
            Opus.Core.IToolset toolset = Opus.Core.ToolsetFactory.GetInstance(typeof(Toolset));
            string installPath = toolset.InstallPath((Opus.Core.BaseTarget)target);
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                this.BinPath = installPath;
            }
            else
            {
                this.BinPath = System.IO.Path.Combine(installPath, "bin");
            }

            this.LibPath = System.IO.Path.Combine(installPath, "lib");
            this.includePaths.Add(System.IO.Path.Combine(installPath, "include"));
        }
    }
}
