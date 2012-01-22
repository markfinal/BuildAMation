// <copyright file="RemoveDependencyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveDependencyAction))]

namespace Opus
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
                return "Remove dependent package (semi-colon separated)";
            }
        }

        public void AssignArguments(string arguments)
        {
            string[] packageAndVersions = arguments.Split(';');
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

            Core.PackageDefinitionFile definitionFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isComplete)
            {
                definitionFile.Read();
            }

            bool success = true;
            foreach (string packageAndVersion in this.PackageAndVersionArray)
            {
                string[] packageNameAndVersion = packageAndVersion.Split('-');
                if (packageNameAndVersion.Length != 2)
                {
                    throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", packageAndVersion), false);
                }

                Core.PackageIdentifier id = new Core.PackageIdentifier(packageNameAndVersion[0], packageNameAndVersion[1]);
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
    }
}