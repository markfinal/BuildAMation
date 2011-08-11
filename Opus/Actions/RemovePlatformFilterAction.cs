// <copyright file="RemovePlatformFilterAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemovePlatformFilterAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class RemovePlatformFilterAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removeplatformfilter";
            }
        }

        public string Description
        {
            get
            {
                return "Removes a platform filter from the specified dependent";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Platform = arguments;
        }

        private string Platform
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.EPlatform requestedFilter = Opus.Core.EPlatform.Invalid;
            switch (this.Platform)
            {
                case "win*":
                    requestedFilter = Opus.Core.EPlatform.Windows;
                    break;

                case "unix*":
                    requestedFilter = Opus.Core.EPlatform.Unix;
                    break;

                case "osx*":
                    requestedFilter = Opus.Core.EPlatform.OSX;
                    break;

                default:
                    throw new Core.Exception(System.String.Format("Platform filter specified, '{0}', is not recognized", this.Platform), false);
            }

            SetDependentAction setDependentAction = Core.ActionManager.FindByType(typeof(SetDependentAction)) as SetDependentAction;
            if (null == setDependentAction)
            {
                throw new Core.Exception("Unable to locate SetDependent action", false);
            }
            if (null == setDependentAction.DependentPackageAndVersion)
            {
                throw new Core.Exception("Dependent package has not been set", false);
            }

            Core.Log.DebugMessage("Dependent package is '{0}'", setDependentAction.DependentPackageAndVersion);

            string[] nameAndVersion = setDependentAction.DependentPackageAndVersion.Split('-');
            if (nameAndVersion.Length != 2)
            {
                throw new Core.Exception(System.String.Format("Ill-formed package name-version pair, '{0}'", nameAndVersion), false);
            }

            Core.PackageUtilities.IdentifyMainPackageOnly();
            Core.PackageIdentifier mainPackageId = Core.State.PackageInfo[0].Identifier;

            Core.PackageIdentifier foundId = null;
            foreach (Core.PackageIdentifier id in mainPackageId.Definition.PackageIdentifiers)
            {
                if (id.Match(nameAndVersion[0], nameAndVersion[1], false))
                {
                    foundId = id;
                    break;
                }
            }

            if (null != foundId)
            {
                Core.EPlatform oldFilter = foundId.PlatformFilter;
                if (!Core.Platform.Contains(oldFilter, requestedFilter))
                {
                    throw new Core.Exception(System.String.Format("Package '{0}' with dependent '{1}' does not have a platform filter for '{2}'", nameAndVersion[0], setDependentAction.DependentPackageAndVersion, this.Platform), false);
                }

                Core.EPlatform newFilter = oldFilter & ~requestedFilter;
                foundId.PlatformFilter = newFilter;

                mainPackageId.Definition.Write();

                Core.Log.MessageAll("Package '{0}' has platform filter '{1}' removed", setDependentAction.DependentPackageAndVersion, this.Platform);

                return true;
            }
            else
            {
                Core.Log.MessageAll("Could not locate package '{0}' as a dependency", setDependentAction.DependentPackageAndVersion);
                return false;
            }
        }
    }
}