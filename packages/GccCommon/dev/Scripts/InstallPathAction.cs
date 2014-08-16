// <copyright file="InstallPathAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(GccCommon.InstallPathAction))]

namespace GccCommon
{
    [Bam.Core.PreambleAction]
    public sealed class InstallPathAction :
        Bam.Core.IActionWithArguments
    {
        private string InstallPath
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.InstallPath = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-gcc.installpath";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Override the Gcc installation path";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            if (!System.IO.Directory.Exists(this.InstallPath))
            {
                var absolutePath = System.IO.Path.Combine(Bam.Core.State.WorkingDirectory, this.InstallPath);
                if (!System.IO.Directory.Exists(absolutePath))
                {
                    throw new Bam.Core.Exception("Path '{0}' does not exist and is not relative to the working directory '{1}", this.InstallPath, Bam.Core.State.WorkingDirectory);
                }

                this.InstallPath = absolutePath;
            }

            Bam.Core.State.AddCategory("Gcc");
            Bam.Core.State.Set("Gcc", "InstallPath", this.InstallPath);

            Bam.Core.Log.DebugMessage("Gcc installation path is now '{0}'", this.InstallPath);

            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
