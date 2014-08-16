// <copyright file="InstallPathAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(MingwCommon.InstallPathAction))]

namespace MingwCommon
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
                return "-mingw.installpath";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Override the MinGW installation path";
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

            Bam.Core.State.AddCategory("Mingw");
            Bam.Core.State.Set("Mingw", "InstallPath", this.InstallPath);

            Bam.Core.Log.DebugMessage("Mingw installation path is now '{0}'", this.InstallPath);

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
