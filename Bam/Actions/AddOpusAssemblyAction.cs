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

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddBuildAMationAssemblyAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddBuildAMationAssemblyAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Adds an BuildAMation assembly to the package definition (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var assemblyNames = arguments.Split(System.IO.Path.PathSeparator);
            this.AssemblyNameArray = new Core.StringArray(assemblyNames);
        }

        private Core.StringArray AssemblyNameArray
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

            bool success = false;
            foreach (var assemblyName in this.AssemblyNameArray)
            {
                if (!xmlFile.OpusAssemblies.Contains(assemblyName))
                {
                    xmlFile.OpusAssemblies.Add(assemblyName);

                    Core.Log.MessageAll("Added BuildAMation assembly '{0}' to package '{1}'", assemblyName, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("BuildAMation assembly '{0}' already used by package '{1}'", assemblyName, mainPackageId.ToString());
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