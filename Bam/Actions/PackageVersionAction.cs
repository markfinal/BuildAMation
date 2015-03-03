#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License

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