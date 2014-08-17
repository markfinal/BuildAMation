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

[assembly: Bam.Core.RegisterAction(typeof(Bam.ShowDefinitionAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class ShowDefinitionAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-showdefinition";
            }
        }

        public string Description
        {
            get
            {
                return "Display the current package's definition";
            }
        }

        private void
        DisplayDependencies(
            Core.PackageDefinitionFile definition,
            int depth)
        {
            foreach (var id in definition.PackageIdentifiers)
            {
                var platformFilter = Core.Platform.ToString(id.PlatformFilter, '|');

                Core.Log.MessageAll("{0}{1}{2} (filter: {3}) (root: '{4}')", new string('\t', depth), (null != id.Root) ? id.ToString("-") : id.ToString("-").ToUpper(), id.IsDefaultVersion ? "*" : System.String.Empty, platformFilter, (null != id.Root) ? id.Root.AbsolutePath : "UNKNOWN");

                if ((null != id.Definition) && (id.Definition.PackageIdentifiers.Count > 0))
                {
                    DisplayDependencies(id.Definition, depth + 1);
                }
            }
        }

        public bool
        Execute()
        {
            // there may be multiple versions of packages - so show them all
            Core.PackageUtilities.IdentifyMainAndDependentPackages(false, true);

            var mainPackageId = Core.State.PackageInfo[0].Identifier;
            var definitionFile = mainPackageId.Definition;

            var packageName = mainPackageId.ToString();
            var formatString = "Definition of package '{0}'";
            int dashLength = formatString.Length - 3 + packageName.Length;
            Core.Log.MessageAll("Definition of package '{0}'", mainPackageId.ToString());
            Core.Log.MessageAll(new string('-', dashLength));
            Core.Log.MessageAll("\nSupported on: {0}", Core.Platform.ToString(definitionFile.SupportedPlatforms, ' '));
            Core.Log.MessageAll("\nBuildAMation assemblies:");
            foreach (var opusAssembly in definitionFile.OpusAssemblies)
            {
                Core.Log.MessageAll("\t{0}", opusAssembly);
            }
            Core.Log.MessageAll("\nDotNet assemblies:");
            foreach (var desc in definitionFile.DotNetAssemblies)
            {
                if (null == desc.RequiredTargetFramework)
                {
                    Core.Log.MessageAll("\t{0}", desc.Name);
                }
                else
                {
                    Core.Log.MessageAll("\t{0} (version {1})", desc.Name, desc.RequiredTargetFramework);
                }
            }
            if (definitionFile.Definitions.Count > 0)
            {
                Core.Log.MessageAll("\n#defines:");
                foreach (var definition in definitionFile.Definitions)
                {
                    Core.Log.MessageAll("\t{0}", definition);
                }
            }

            if (definitionFile.PackageRoots.Count > 0)
            {
                Core.Log.MessageAll("\nExtra package search directories:");
                foreach (var rootPath in definitionFile.PackageRoots)
                {
                    var absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(rootPath);

                    Core.Log.MessageAll("\t'{0}'\t(absolute path '{1}')", rootPath, absolutePackageRoot);
                }
            }

            if (definitionFile.PackageIdentifiers.Count > 0)
            {
                Core.Log.MessageAll("\nDependent packages (* = default version):", mainPackageId.ToString());
                this.DisplayDependencies(definitionFile, 1);
            }
            else
            {
                Core.Log.MessageAll("\nNo dependent packages", mainPackageId.ToString());
            }

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