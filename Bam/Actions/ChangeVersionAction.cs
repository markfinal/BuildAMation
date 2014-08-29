#region License
// Copyright 2010-2014 Mark Final
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