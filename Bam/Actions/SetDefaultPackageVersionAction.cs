#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetDefaultPackageVersionAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class SetDefaultPackageVersionAction :
        Core.IActionWithArguments
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

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
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

            if (!Core.State.HasCategory("PackageDefaultVersions"))
            {
                Core.State.AddCategory("PackageDefaultVersions");
            }
            Core.State.Set("PackageDefaultVersions", nameAndVersion[0], nameAndVersion[1]);

            Core.PackageUtilities.IdentifyMainPackageOnly();
            var mainPackageId = Core.State.PackageInfo[0].Identifier;

            var nameToMatch = nameAndVersion[0];
            var versionToMatch = nameAndVersion[1];

            Core.PackageIdentifier foundId = null;
            Core.PackageIdentifier foundDefaultId = null;
            foreach (var id in mainPackageId.Definition.PackageIdentifiers)
            {
                var matchNameAndVersion = id.Match(nameToMatch, versionToMatch, false);

                // if we're trying to set a new default, make sure existing packages matching
                // by name are not already marked as default
                if (this.IsDefaultVersion && id.MatchName(nameToMatch, false))
                {
                    if (id.IsDefaultVersion)
                    {
                        if (matchNameAndVersion)
                        {
                            Core.Log.MessageAll("Package {0}-{1} is already the default", id.Name, id.Version);
                            return true;
                        }
                        else
                        {
                            foundDefaultId = id;
                        }
                    }
                }

                if (matchNameAndVersion)
                {
                    foundId = id;
                }
            }

            if (null == foundId)
            {
                Core.Log.MessageAll("Could not locate package '{0}' as a dependency", setDependentAction.DependentPackageAndVersion);
                return false;
            }

            if (null != foundDefaultId)
            {
                Core.Log.MessageAll("Default version is currently set to {0}-{1}", foundDefaultId.Name, foundDefaultId.Version);
                return false;
            }

            if (!this.IsDefaultVersion && !foundId.IsDefaultVersion)
            {
                Core.Log.MessageAll("Package {0}-{1} was not previously the default", foundId.Name, foundId.Version);
                return false;
            }

            foundId.IsDefaultVersion = this.IsDefaultVersion;

            mainPackageId.Definition.Write();

            Core.Log.MessageAll("Updated dependent package '{0}' so that version '{1}' {2} the default version", foundId.Name, foundId.Version, this.IsDefaultVersion ? "is" : "is not");

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
