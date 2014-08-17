// <copyright file="ChangeVersionAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.ChangeVersionAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class ChangeVersionAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-changeversion";
            }
        }

        public string Description
        {
            get
            {
                return "Change the version of a dependent package";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.NewVersion = arguments;
        }

        private string NewVersion
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

            if (null != foundId)
            {
                var newId = new Core.PackageIdentifier(nameAndVersion[0], this.NewVersion);

                if (mainPackageId.Definition.PackageIdentifiers.Contains(newId))
                {
                    throw new Core.Exception("Package '{0}' already exists as a dependency. Cannot change the version of package '{1}' to '{2}'", newId.ToString(), foundId.ToString(), this.NewVersion);
                }

                mainPackageId.Definition.PackageIdentifiers.Remove(foundId);
                mainPackageId.Definition.PackageIdentifiers.Add(newId);

                mainPackageId.Definition.Write();

                Core.Log.MessageAll("Updated dependent package '{0}' from version '{1}' to '{2}'", nameAndVersion[0], foundId.Version, this.NewVersion);

                return true;
            }
            else
            {
                Core.Log.MessageAll("Could not locate package '{0}' as a dependency", setDependentAction.DependentPackageAndVersion);
                return false;
            }
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