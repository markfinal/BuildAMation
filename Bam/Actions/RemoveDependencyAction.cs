// <copyright file="RemoveDependencyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.RemoveDependencyAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class RemoveDependencyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removedependency";
            }
        }

        public string Description
        {
            get
            {
                return "Remove dependent package (separated by " + System.IO.Path.PathSeparator + ")";
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

            // don't validate locations, in case packages have been deleted from disk
            var validatePackageLocations = false;

            var definitionFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isWellDefined)
            {
                definitionFile.Read(true, validatePackageLocations);
            }

            var success = true;
            foreach (var packageAndVersion in this.PackageAndVersionArray)
            {
                var packageNameAndVersion = packageAndVersion.Split('-');
                if (packageNameAndVersion.Length != 2)
                {
                    throw new Core.Exception("Ill-formed package name-version pair, '{0}'", packageAndVersion);
                }

                var id = new Core.PackageIdentifier(packageNameAndVersion[0], packageNameAndVersion[1], validatePackageLocations);
                if (definitionFile.RemovePackage(id))
                {
                    success = true;
                }
            }

            if (success)
            {
                definitionFile.Write();
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