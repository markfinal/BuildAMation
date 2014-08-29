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

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddDependencyAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddDependencyAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-adddependency";
            }
        }

        public string Description
        {
            get
            {
                return "Add a dependent package (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var packageAndVersions = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageAndVersionArray = new Core.StringArray(packageAndVersions);
        }

        private Core.StringArray PackageAndVersionArray
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

            foreach (var packageAndVersion in this.PackageAndVersionArray)
            {
                var packageNameAndVersion = packageAndVersion.Split('-');
                string packageName = null;
                string packageVersion = null;
                if (packageNameAndVersion.Length < 2)
                {
                    throw new Core.Exception("Ill-formed package name-version pair, '{0}'", packageAndVersion);
                }
                else if (packageNameAndVersion.Length > 2)
                {
                    packageName = packageNameAndVersion[0];
                    packageVersion = packageNameAndVersion[1];
                    for (int i = 2; i < packageNameAndVersion.Length; ++i)
                    {
                        packageVersion += "-" + packageNameAndVersion[i];
                    }
                }
                else
                {
                    packageName = packageNameAndVersion[0];
                    packageVersion = packageNameAndVersion[1];
                }

                var id = new Core.PackageIdentifier(packageName, packageVersion);
                xmlFile.AddRequiredPackage(id);
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