// <copyright file="InstallPathAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCmommon package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(QtCommon.InstallPathAction))]

namespace QtCommon
{
    [Opus.Core.PreambleAction]
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
                return "-Qt.installpath";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Override the Qt installation path";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            if (!System.IO.Directory.Exists(this.InstallPath))
            {
                var absolutePath = System.IO.Path.Combine(Opus.Core.State.WorkingDirectory, this.InstallPath);
                if (!System.IO.Directory.Exists(absolutePath))
                {
                    throw new Opus.Core.Exception("Path '{0}' does not exist and is not relative to the working directory '{1}", this.InstallPath, Opus.Core.State.WorkingDirectory);
                }

                this.InstallPath = absolutePath;
            }

            Opus.Core.State.AddCategory("Qt");
            Opus.Core.State.Set("Qt", "InstallPath", this.InstallPath);

            Opus.Core.Log.DebugMessage("Qt installation path is now '{0}'", this.InstallPath);

            return true;
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
