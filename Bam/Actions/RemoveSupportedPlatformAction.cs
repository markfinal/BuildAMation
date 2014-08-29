#region License
// Copyright 2010-2014 Mark Final
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

[assembly: Bam.Core.RegisterAction(typeof(Bam.RemoveSupportedPlatformAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class RemoveSupportedPlatformAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removesupportedplatform";
            }
        }

        public string Description
        {
            get
            {
                return "Remove a supported platform from the package (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var platforms = arguments.Split(System.IO.Path.PathSeparator);
            this.PlatformArray = new Core.StringArray(platforms);
        }

        private Core.StringArray PlatformArray
        {
            get;
            set;
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
                xmlFile.Read(true);
            }

            var success = false;
            foreach (var supportedPlatform in this.PlatformArray)
            {
                var platform = Core.Platform.FromString(supportedPlatform);

                if (Core.Platform.Contains(xmlFile.SupportedPlatforms, platform))
                {
                    xmlFile.SupportedPlatforms &= ~platform;

                    Core.Log.MessageAll("Removed supported platform '{0}' from package '{1}'", supportedPlatform, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Platform '{0}' was already not supported by package '{1}'", supportedPlatform, mainPackageId.ToString());
                }
            }

            if (success)
            {
                xmlFile.Write();
                return true;
            }
            else
            {
                return false;
            }
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