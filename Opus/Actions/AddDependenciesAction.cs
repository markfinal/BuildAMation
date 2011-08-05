// <copyright file="AddDependenciesAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddDependenciesAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddDependenciesAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-adddependencies";
            }
        }

        public string Description
        {
            get
            {
                return "Add dependent packages";
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
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Working directory is not a package", false);
            }

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;
            Core.PackageDependencyXmlFile xmlFile = mainPackage.Identifier.Definition;

            foreach (string packageAndVersion in this.PackagesAndVersions)
            {
                string[] packageNameAndVersion = packageAndVersion.Split('-');
                if (packageNameAndVersion.Length != 2)
                {
                    throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", packageAndVersion), false);
                }

                Core.PackageIdentifier id = new Opus.Core.PackageIdentifier(packageNameAndVersion[0], packageNameAndVersion[1]);

                xmlFile.AddRequiredPackage(packageNameAndVersion[0], packageNameAndVersion[1]);
            }

            xmlFile.Write();

            return true;
        }
    }
}