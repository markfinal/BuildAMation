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

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddDotNetAssemblyAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddDotNetAssemblyAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-adddotnetassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Adds a DotNet assembly to the package definition (format: assembly-frameworkversion) (separated by " + System.IO.Path.PathSeparator + ")";
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

            foreach (var dotNetAssemblyName in this.DotNetAssemblyNameArray)
            {
                string assemblyName = null;
                string targetVersion = null;
                if (dotNetAssemblyName.Contains("-"))
                {
                    var split = dotNetAssemblyName.Split('-');
                    if (split.Length != 2)
                    {
                        throw new Core.Exception("DotNet assembly name and version is ill-formed: '{0}'", dotNetAssemblyName);
                    }

                    assemblyName = split[0];
                    targetVersion = split[1];
                }
                else
                {
                    assemblyName = dotNetAssemblyName;
                }

                foreach (var desc in xmlFile.DotNetAssemblies)
                {
                    if (desc.Name == assemblyName)
                    {
                        throw new Core.Exception("DotNet assembly '{0}' already referenced by the package", assemblyName);
                    }
                }

                var descToAdd = new Core.DotNetAssemblyDescription(assemblyName);
                if (null != targetVersion)
                {
                    descToAdd.RequiredTargetFramework = targetVersion;
                }
                xmlFile.DotNetAssemblies.Add(descToAdd);

                if (null != targetVersion)
                {
                    Core.Log.MessageAll("Added DotNet assembly '{0}', framework version '{1}', to package '{2}'", assemblyName, targetVersion, mainPackageId.ToString());
                }
                else
                {
                    Core.Log.MessageAll("Added DotNet assembly '{0}' to package '{1}'", assemblyName, mainPackageId.ToString());
                }
            }

            xmlFile.Write();

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