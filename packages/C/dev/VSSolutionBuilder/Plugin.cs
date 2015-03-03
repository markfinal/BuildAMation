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
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            C.Plugin moduleToBuild,
            out bool success)
        {
            var asDynamicLibrary = moduleToBuild as C.DynamicLibrary;
            var result = Build(asDynamicLibrary, out success);
            if (!success)
            {
                return result;
            }

            var node = moduleToBuild.OwningNode;
            var target = node.Target;
            var moduleName = node.ModuleName;

            IProject projectData = null;
            // TODO: want to remove this
            lock (this.solutionFile.ProjectDictionary)
            {
                projectData = this.solutionFile.ProjectDictionary[moduleName];
            }

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);
            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                configuration = projectData.Configurations[configurationName];
            }

            // indicate that the import library for this dynamic library should be ignored
            // by any module that depends on this
            // this allows having the plugin as a dependent (for build order) and yet don't add it's import library
            // to the library list to link against
            configuration.Properties.Add("IgnoreImportLibrary", "true");

            return result;
        }
    }
}
