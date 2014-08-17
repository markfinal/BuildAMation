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
#endregion

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddPlatformFilterAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddPlatformFilterAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addplatformfilter";
            }
        }

        public string Description
        {
            get
            {
                return "Adds a platform filter to the specified dependent (separated by " + System.IO.Path.PathSeparator + ")";
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
                if (Core.Platform.Contains(oldFilter, requestedFilter))
                {
                    throw new Core.Exception("Package '{0}' with dependent '{1}' already has a platform filter for '{2}'", nameAndVersion[0], setDependentAction.DependentPackageAndVersion, platformFilter);
                }

                var newFilter = oldFilter | requestedFilter;
                foundId.PlatformFilter = newFilter;

                Core.Log.MessageAll("Package '{0}' has platform filter '{1}' added", setDependentAction.DependentPackageAndVersion, platformFilter);
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