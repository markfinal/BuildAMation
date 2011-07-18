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
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Working directory is not a package", false);
            }

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;
#if true
            Core.PackageDependencyXmlFile xmlFile = mainPackage.PackageDefinition;
#else
            string dependencyXMLPathName = mainPackage.DependencyFile;

            Core.PackageDependencyXmlFile xmlFile = new Core.PackageDependencyXmlFile(dependencyXMLPathName, true);
            xmlFile.Read();
#endif

            int packageChangeCount = 0;
            foreach (string packageAndVersion in this.PackagesAndVersions)
            {
                string[] packageNameAndVersion = packageAndVersion.Split('-');
                if (packageNameAndVersion.Length != 2)
                {
                    throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", packageAndVersion), false);
                }

                if (xmlFile.UpdatePackage(packageNameAndVersion[0], packageNameAndVersion[1]))
                {
                    ++packageChangeCount;
                }
            }

            if (packageChangeCount > 0)
            {
                xmlFile.Write();
            }

            return true;
        }
    }
}