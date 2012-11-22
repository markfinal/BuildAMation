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
            // NEW STYLE
#if true
            // TODO: investigate this - Qt is a ThirdpartyModule, with no toolset
            // what is the best course of action?
            string installPath = Opus.Core.ToolsetFactory.CreateToolset(typeof(Toolset)).InstallPath((Opus.Core.BaseTarget)target);
            //string installPath = target.Toolset.InstallPath((Opus.Core.BaseTarget)target);
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                this.BinPath = installPath;
            }
            else
            {
                this.BinPath = System.IO.Path.Combine(installPath, "bin");
            }
#else
            if (Opus.Core.OSUtilities.IsOSXHosting)
            {
                this.BinPath = installPath;
            }
            else
            {
                this.BinPath = System.IO.Path.Combine(installPath, "bin");
            }
#endif

            this.LibPath = System.IO.Path.Combine(installPath, "lib");
            this.includePaths.Add(System.IO.Path.Combine(installPath, "include"));
        }
    }
}
