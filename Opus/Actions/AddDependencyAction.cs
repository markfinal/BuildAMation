// <copyright file="AddDependencyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddDependencyAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddDependencyAction : Core.IActionWithArguments
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
                return "Add a dependent package (semi-colon separated)";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            string[] packageAndVersions = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageAndVersionArray = new Opus.Core.StringArray(packageAndVersions);
        }

        private Core.StringArray PackageAndVersionArray
        {
            get;
            set;
        }

        public bool Execute()
        {
            bool isComplete;
            Core.PackageIdentifier mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isComplete);
            if (null == mainPackageId)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a package", Core.State.WorkingDirectory), false);
            }
            if (!isComplete)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory), false);
            }

            Core.PackageDefinitionFile xmlFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isComplete)
            {
                xmlFile.Read(true);
            }

            foreach (string packageAndVersion in this.PackageAndVersionArray)
            {
                string[] packageNameAndVersion = packageAndVersion.Split('-');
                string packageName = null;
                string packageVersion = null;
                if (packageNameAndVersion.Length < 2)
                {
                    throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", packageAndVersion), false);
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

                Core.PackageIdentifier id = new Core.PackageIdentifier(packageName, packageVersion);
                xmlFile.AddRequiredPackage(id);
            }

            xmlFile.Write();

            return true;
        }
    }
}