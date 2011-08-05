// <copyright file="ChangeDependenciesAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.ChangeDependenciesAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class ChangeDependenciesAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-changedependencies";
            }
        }

        public string Description
        {
            get
            {
                return "Change dependent packages";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.PackagesAndVersions = new Opus.Core.StringArray();
            this.PackagesAndVersions.AddRange(arguments.Split(';'));
        }

        private Core.StringArray PackagesAndVersions
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
                throw new Core.Exception("Working directory is not a package", false);
            }

            Core.PackageDefinitionFile definitionFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isComplete)
            {
                definitionFile.Read();
            }

            int numberOfPackagesChanged = 0;
            foreach (string packageAndVersion in this.PackagesAndVersions)
            {
                string[] packageNameAndVersion = packageAndVersion.Split('-');
                if (packageNameAndVersion.Length != 2)
                {
                    throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", packageAndVersion), false);
                }

                Core.PackageIdentifier id = new Opus.Core.PackageIdentifier(packageNameAndVersion[0], packageNameAndVersion[1]);
                if (definitionFile.UpdatePackage(id))
                {
                    ++numberOfPackagesChanged;
                }
            }

            if (numberOfPackagesChanged > 0)
            {
                definitionFile.Write();
            }

            return true;
        }
    }
}