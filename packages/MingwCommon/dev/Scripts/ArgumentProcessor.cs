// <copyright file="InstallPathAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(MingwCommon.InstallPathAction))]

namespace MingwCommon
{
    public sealed class InstallPathAction : Opus.Core.IActionWithArguments
    {
        private string InstallPath
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.InstallPath = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-mingw.installpath";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Override the MinGW installation path";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            if (!System.IO.Directory.Exists(this.InstallPath))
            {
                throw new Opus.Core.Exception(System.String.Format("Path '{0}' does not exist", this.InstallPath), false);
            }

            Opus.Core.State.AddCategory("Mingw");
            Opus.Core.State.Set("Mingw", "InstallPath", this.InstallPath);

            Opus.Core.Log.DebugMessage("Mingw installation path is now '{0}'", this.InstallPath);

            return true;
        }
    }
}
