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

[assembly: Bam.Core.RegisterAction(typeof(Bam.RemoveDotNetAssemblyAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class RemoveDotNetAssemblyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removedotnetassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Removes a DotNet assembly from the package definition (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var assemblyNames = arguments.Split(System.IO.Path.PathSeparator);
            this.DotNetAssemblyNameArray = new Core.StringArray(assemblyNames);
        }

        private Core.StringArray DotNetAssemblyNameArray
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            bool isWellDefined;
            var mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isWellDefined);
            if (null == mainPackageId)
            {
                throw new Core.Exception("Working directory, '{0}', is not a package", Core.State.WorkingDirectory);
            }
            if (!isWellDefined)
            {
                throw new Core.Exception("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory);
            }

            var xmlFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isWellDefined)
            {
                xmlFile.Read(true);
            }

            var success = false;
            foreach (var dotNetAssemblyName in this.DotNetAssemblyNameArray)
            {
                Core.DotNetAssemblyDescription foundDesc = null;
                foreach (var desc in xmlFile.DotNetAssemblies)
                {
                    if (desc.Name == dotNetAssemblyName)
                    {
                        foundDesc = desc;
                    }
                }

                if (null != foundDesc)
                {
                    xmlFile.DotNetAssemblies.Remove(foundDesc);

                    Core.Log.MessageAll("Removed DotNet assembly '{0}' from package '{1}'", dotNetAssemblyName, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Could not find DotNet assembly '{0}' in package '{1}'", dotNetAssemblyName, mainPackageId.ToString());
                }
            }

            if (success)
            {
                xmlFile.Write();
                return true;
            }
            else
            {
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