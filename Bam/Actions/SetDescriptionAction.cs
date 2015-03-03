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

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetDescriptionAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class SetDescriptionAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-setdescription";
            }
        }

        public string Description
        {
            get
            {
                return "Set a package description (leave empty to remove)";
            }
        }

        private string DescriptionText
        {
            get;
            set;
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.DescriptionText = arguments;
        }

        public bool
        Execute()
        {
            bool isWellDefined;
            var mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isWellDefined);
            if (null == mainPackageId)
            {
                throw new Core.Exception("Working directory, '{0}', is not a package", Core.State.WorkingDirectory);
            }
            if (!isWellDefined)
            {
                throw new Core.Exception("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory);
            }

            var xmlFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isWellDefined)
            {
                // note: do not validate package locations, as the package roots may not yet be set up
                xmlFile.Read(true, false);
            }

            xmlFile.Description = this.DescriptionText;
            if (string.IsNullOrEmpty(this.DescriptionText))
            {
                Core.Log.MessageAll("Removed package description for {0}-{1}", mainPackageId.Name, mainPackageId.Version);
            }
            else
            {
                Core.Log.MessageAll("Set description of package {0}-{1} to '{2}'", mainPackageId.Name, mainPackageId.Version, this.DescriptionText);
            }

            xmlFile.Write();
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