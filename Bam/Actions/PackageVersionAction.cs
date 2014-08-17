// <copyright file="PackageVesionAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.PackageVersionAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class PackageVersionAction :
        Core.IActionWithArguments,
        Core.IActionCommandComparison
    {
        private string NameChosen
        {
            get;
            set;
        }

        private string VersionChosen
        {
            get;
            set;
        }

        #region IActionWithArguments Members

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.VersionChosen = arguments;
        }

        #endregion

        #region IAction Members

        string Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-<packagename>.version";
            }
        }

        string Core.IAction.Description
        {
            get
            {
                return "Generic package version selection.";
            }
        }

        bool
        Core.IAction.Execute()
        {
            if (!Core.State.HasCategory("PackageDefaultVersions"))
            {
                Core.State.AddCategory("PackageDefaultVersions");
            }

            Core.State.Add<string>("PackageDefaultVersions", this.NameChosen, this.VersionChosen);

            return true;
        }

        #endregion

        #region IActionCommandComparison Members

        bool
        Core.IActionCommandComparison.Compare(
            string command1,
            string command2)
        {
            if (command2.EndsWith(".version"))
            {
                this.NameChosen = command2.Split('.')[0].TrimStart('-');
                return true;
            }

            return false;
        }

        #endregion

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}