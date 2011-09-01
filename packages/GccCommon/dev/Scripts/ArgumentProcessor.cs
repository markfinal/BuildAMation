// <copyright file="InstallPathAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(GccCommon.InstallPathAction))]

namespace GccCommon
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
                return "-gcc.installpath";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Override the Gcc installation path";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            if (!System.IO.Directory.Exists(this.InstallPath))
            {
                throw new Opus.Core.Exception(System.String.Format("Path '{0}' does not exist", this.InstallPath), false);
            }

            Opus.Core.State.AddCategory("Gcc");
            Opus.Core.State.Set("Gcc", "InstallPath", this.InstallPath);

            Opus.Core.Log.DebugMessage("Gcc installation path is now '{0}'", this.InstallPath);

            return true;
        }
    }
}
