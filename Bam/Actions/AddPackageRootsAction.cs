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

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddPackageRootsAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddPackageRootsAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addpackageroots";
            }
        }

        public string Description
        {
            get
            {
                return "Add package roots (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        private Core.StringArray PackageRoots
        {
            get;
            set;
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var roots = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageRoots = new Core.StringArray(roots);
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

            var success = false;
            foreach (var packageRoot in this.PackageRoots)
            {
                var standardPackageRoot = packageRoot.Replace('\\', '/');
                if (!xmlFile.PackageRoots.Contains(standardPackageRoot))
                {
                    // note: adding the relative path, so this package can be moved around
                    xmlFile.PackageRoots.Add(standardPackageRoot);
                    Core.Log.MessageAll("Added package root '{0}' to package '{1}'", standardPackageRoot, mainPackageId.ToString());
                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Package root '{0}' already used by package '{1}'", standardPackageRoot, mainPackageId.ToString());
                }

                var absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(standardPackageRoot);
                Core.State.PackageRoots.Add(Core.DirectoryLocation.Get(absolutePackageRoot));
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