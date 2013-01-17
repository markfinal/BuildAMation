// <copyright file="SetDefaultPackageVersionAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetDefaultPackageVersionAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class SetDefaultPackageVersionAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-setdefaultversion";
            }
        }

        public string Description
        {
            get
            {
                return "Set whether the dependent specified is the default version";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            if (arguments == "true")
            {
                this.IsDefaultVersion = true;
            }
            else if (arguments == "false")
            {
                this.IsDefaultVersion = false;
            }
            else
            {
                throw new Core.Exception("Argument is '{0}'. Expecting 'true' or 'false'", arguments);
            }
        }

        private bool IsDefaultVersion
        {
            get;
            set;
        }

        public bool Execute()
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
            SetDependentAction setDependentAction = setDependentActionArray[0] as SetDependentAction;
            if (null == setDependentAction.DependentPackageAndVersion)
            {
                throw new Core.Exception("Dependent package has not been set");
            }

            Core.Log.DebugMessage("Dependent package is '{0}'", setDependentAction.DependentPackageAndVersion);

            string[] nameAndVersion = setDependentAction.DependentPackageAndVersion.Split('-');
            if (nameAndVersion.Length != 2)
            {
                throw new Core.Exception("Ill-formed package name-version pair, '{0}'", nameAndVersion);
            }

            if (!Core.State.HasCategory("PackageDefaultVersions"))
            {
                Core.State.AddCategory("PackageDefaultVersions");
            }
            Core.State.Set("PackageDefaultVersions", nameAndVersion[0], nameAndVersion[1]);

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
                foundId.IsDefaultVersion = this.IsDefaultVersion;

                mainPackageId.Definition.Write();

                Core.Log.MessageAll("Updated dependent package '{0}' so that version '{1}' {2} the default version", foundId.Name, foundId.Version, this.IsDefaultVersion ? "is" : "is not");

                return true;
            }
            else
            {
                Core.Log.MessageAll("Could not locate package '{0}' as a dependency", setDependentAction.DependentPackageAndVersion);
                return false;
            }
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
