// <copyright file="RemovePlatformFilterAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.RemovePlatformFilterAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class RemovePlatformFilterAction :
        Core.IActionWithArguments
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
                return "Removes a platform filter from the specified dependent (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var platforms = arguments.Split(System.IO.Path.PathSeparator);
            this.PlatformArray = new Core.StringArray(platforms);
        }

        private Core.StringArray PlatformArray
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            var setDependentActionArray = Core.ActionManager.FindInvokedActionsByType(typeof(SetDependentAction));
            if (0 == setDependentActionArray.Count)
            {
                throw new Core.Exception("Unable to locate SetDependent action");
            }
            if (setDependentActionArray.Count > 1)
            {
                throw new Core.Exception("Multiple SetDependent actions were specified");
            }
            var setDependentAction = setDependentActionArray[0] as SetDependentAction;
            if (null == setDependentAction.DependentPackageAndVersion)
            {
                throw new Core.Exception("Dependent package has not been set");
            }

            Core.Log.DebugMessage("Dependent package is '{0}'", setDependentAction.DependentPackageAndVersion);

            var nameAndVersion = setDependentAction.DependentPackageAndVersion.Split('-');
            if (nameAndVersion.Length != 2)
            {
                throw new Core.Exception("Ill-formed package name-version pair, '{0}'", nameAndVersion);
            }

            Core.PackageUtilities.IdentifyMainPackageOnly();
            var mainPackageId = Core.State.PackageInfo[0].Identifier;

            Core.PackageIdentifier foundId = null;
            foreach (var id in mainPackageId.Definition.PackageIdentifiers)
            {
                if (id.Match(nameAndVersion[0], nameAndVersion[1], false))
                {
                    foundId = id;
                    break;
                }
            }

            if (null == foundId)
            {
                Core.Log.MessageAll("Could not locate package '{0}' as a dependency", setDependentAction.DependentPackageAndVersion);
                return false;
            }

            foreach (var platformFilter in this.PlatformArray)
            {
                var requestedFilter = Core.Platform.FromString(platformFilter);
                if (Core.EPlatform.Invalid == requestedFilter)
                {
                    throw new Core.Exception("Platform filter specified, '{0}', is not recognized", platformFilter);
                }

                var oldFilter = foundId.PlatformFilter;
                if (!Core.Platform.Contains(oldFilter, requestedFilter))
                {
                    throw new Core.Exception("Package '{0}' with dependent '{1}' does not have a platform filter for '{2}'", nameAndVersion[0], setDependentAction.DependentPackageAndVersion, platformFilter);
                }

                var newFilter = oldFilter & ~requestedFilter;
                foundId.PlatformFilter = newFilter;

                Core.Log.MessageAll("Package '{0}' has platform filter '{1}' removed", setDependentAction.DependentPackageAndVersion, platformFilter);
            }

            mainPackageId.Definition.Write();

            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}