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
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a package", Core.State.WorkingDirectory), false);
            }
            if (!isComplete)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory), false);
            }

            Core.PackageDefinitionFile definitionFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            definitionFile.Read();

            if (definitionFile.PackageIdentifiers.Count > 0)
            {
                Core.Log.MessageAll("Explicit dependencies of package '{0}' are", mainPackageId.ToString());
                foreach (Core.PackageIdentifier id in definitionFile.PackageIdentifiers)
                {
                    string platformFilter = Core.Platform.ToString(id.PlatformFilter, '|');

                    Core.Log.MessageAll("\t{0} on {1} in '{2}'", id.ToString("-"), platformFilter, id.Root);
                }
            }
            else
            {
                Core.Log.MessageAll("Package '{0}' has no explicit dependencies", mainPackageId.ToString());
            }

            return true;
        }
    }
}