// <copyright file="ShowDependenciesAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.ShowDependenciesAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class ShowDependenciesAction : Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-showdependencies";
            }
        }

        public string Description
        {
            get
            {
                return "Show dependent packages";
            }
        }

        public bool Execute()
        {
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Working directory is not a package", false);
            }

            Core.PackageInformation mainPackage = Core.State.PackageInfo[0];
#if true
            Core.PackageDependencyXmlFile xmlFile = mainPackage.PackageDefinition;
#else
            string dependencyXMLPathName = mainPackage.DependencyFile;

            Core.PackageDependencyXmlFile xmlFile = new Core.PackageDependencyXmlFile(dependencyXMLPathName, true);
            xmlFile.Read();
#endif

            Core.Log.MessageAll("Explicit dependencies of package '{0}' are", mainPackage.FullName);
            foreach (Core.PackageInformation package in xmlFile.Packages)
            {
                if (package.Root != null)
                {
                    Core.Log.MessageAll("\t{0} @ '{1}'", package.FullName, package.Root);
                }
                else
                {
                    Core.Log.MessageAll("\t{0} (not found in any root)", package.FullName);
                }
            }

            return true;
        }
    }
}