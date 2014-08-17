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
namespace Bam.Core
{
    public static class BuilderUtilities
    {
        public static bool
        IsBuilderPackage(
            string packageName)
        {
            return packageName.EndsWith("Builder");
        }

        public static void
        SetBuilderPackage()
        {
            if (null == State.BuilderName)
            {
                return;
            }

            var builderPackageName = System.String.Format("{0}Builder", State.BuilderName);
            foreach (var package in State.PackageInfo)
            {
                if (builderPackageName == package.Name)
                {
                    package.IsBuilder = true;
                    State.BuilderPackage = package;
                    return;
                }
            }

            throw new Exception("Builder package '{0}' was not specified as a dependency", builderPackageName);
        }

        public static void
        CreateBuilderInstance()
        {
            if (null == State.BuilderName)
            {
                throw new Exception("Name of the Builder has not been specified");
            }

            if (null == State.ScriptAssembly)
            {
                throw new Exception("Script assembly has not been set");
            }

            IBuilder builderInstance = null;
            var attributes = State.ScriptAssembly.GetCustomAttributes(typeof(DeclareBuilderAttribute), false) as DeclareBuilderAttribute[];
            foreach (var attribute in attributes)
            {
                if (attribute.Name == State.BuilderName)
                {
                    builderInstance = BuilderFactory.CreateBuilder(attribute.Type);
                    break;
                }
            }
            if (null == builderInstance)
            {
                throw new Exception("Unsupported builder '{0}'. Please double check the spelling as the name is case sensitive", State.BuilderName);
            }

            State.BuilderInstance = builderInstance;
        }
    }
}
