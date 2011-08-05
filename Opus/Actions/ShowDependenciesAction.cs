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
            bool isComplete;
            Core.PackageIdentifier mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isComplete);
            if (null == mainPackageId)
            {
                throw new Core.Exception("Working directory is not a package", false);
            }
            if (!isComplete)
            {
                throw new Core.Exception(System.String.Format("Unable to locate all of the package files in '{0}'", Core.State.WorkingDirectory), false);
            }

            Core.PackageDefinitionFile definitionFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            definitionFile.Read();

            Core.Log.MessageAll("Explicit dependencies of package '{0}' are", mainPackageId.ToString());
            foreach (Core.PackageIdentifier id in definitionFile.PackageIdentifiers)
            {
                Core.Log.MessageAll("\t{0} in '{1}'", id.ToString("-"), id.Root);
            }

            return true;
        }
    }
}