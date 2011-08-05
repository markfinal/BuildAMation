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

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;

            Core.Log.MessageAll("Explicit dependencies of package '{0}' are", mainPackage.FullName);
            Core.PackageDependencyXmlFile xmlFile = mainPackage.Identifier.Definition;
            foreach (Core.PackageIdentifier id in xmlFile.Packages)
            {
                Core.Log.MessageAll("\t{0} @ '{1}'", id.ToString("-"), id.Root);
            }

            return true;
        }
    }
}