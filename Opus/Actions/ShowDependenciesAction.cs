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
                return "Show dependent packages (recursively)";
            }
        }

        private void DisplayDependencies(Core.PackageDefinitionFile definition, int depth)
        {
            foreach (Core.PackageIdentifier id in definition.PackageIdentifiers)
            {
                string platformFilter = Core.Platform.ToString(id.PlatformFilter, '|');

                Core.Log.MessageAll("{0}{1} on {2} in '{3}'", new string('\t', depth), id.ToString("-"), platformFilter, id.Root);

                if ((null != id.Definition) && (id.Definition.PackageIdentifiers.Count > 0))
                {
                    DisplayDependencies(id.Definition, depth + 1);
                }
            }
        }

        public bool Execute()
        {
            Core.PackageUtilities.IdentifyMainAndDependentPackages();
            Core.PackageIdentifier mainPackageId = Core.State.PackageInfo[0].Identifier;
            Core.PackageDefinitionFile definitionFile = mainPackageId.Definition;

            if (definitionFile.PackageIdentifiers.Count > 0)
            {
                Core.Log.MessageAll("Dependencies of package '{0}' are", mainPackageId.ToString());
                this.DisplayDependencies(definitionFile, 1);
            }
            else
            {
                Core.Log.MessageAll("Package '{0}' has no dependencies", mainPackageId.ToString());
            }

            return true;
        }
    }
}