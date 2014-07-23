// <copyright file="AddDependencyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddDependencyAction))]

namespace Opus
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
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var packageAndVersions = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageAndVersionArray = new Opus.Core.StringArray(packageAndVersions);
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