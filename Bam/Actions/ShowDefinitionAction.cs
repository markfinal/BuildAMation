#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License

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
#if true
#else
            foreach (var id in definition.PackageIdentifiers)
            {
                var platformFilter = Core.Platform.ToString(id.PlatformFilter, '|');

                Core.Log.MessageAll("{0}{1}{2} (filter: {3}) (root: '{4}')", new string('\t', depth), (null != id.Root) ? id.ToString("-") : id.ToString("-").ToUpper(), id.IsDefaultVersion ? "*" : System.String.Empty, platformFilter, (null != id.Root) ? id.Root.AbsolutePath : "UNKNOWN");

                if ((null != id.Definition) && (id.Definition.PackageIdentifiers.Count > 0))
                {
                    DisplayDependencies(id.Definition, depth + 1);
                }
            }
#endif
        }

        public bool
        Execute()
        {
#if true
            return false;
#else
            // there may be multiple versions of packages - so show them all
            Core.PackageUtilities.IdentifyMainAndDependentPackages(false, true);

            var mainPackageId = Core.State.PackageInfo[0].Identifier;
            var definitionFile = mainPackageId.Definition;

            var packageName = mainPackageId.ToString();
            var formatString = "Definition of package '{0}'";
            int dashLength = formatString.Length - 3 + packageName.Length;
            Core.Log.MessageAll("Definition of package '{0}'", mainPackageId.ToString());
            Core.Log.MessageAll(new string('-', dashLength));
            if (!string.IsNullOrEmpty(definitionFile.Description))
            {
                Core.Log.MessageAll("Description: {0}", definitionFile.Description);
            }
            Core.Log.MessageAll("\nSupported on: {0}", Core.Platform.ToString(definitionFile.SupportedPlatforms, ' '));
            Core.Log.MessageAll("\nBuildAMation assemblies:");
            foreach (var assembly in definitionFile.BamAssemblies)
            {
                Core.Log.MessageAll("\t{0}", assembly);
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
#endif
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