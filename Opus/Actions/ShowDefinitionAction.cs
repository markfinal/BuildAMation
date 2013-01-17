// <copyright file="ShowDefinitionAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.ShowDefinitionAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class ShowDefinitionAction : Core.IAction
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

        private void DisplayDependencies(Core.PackageDefinitionFile definition, int depth)
        {
            foreach (Core.PackageIdentifier id in definition.PackageIdentifiers)
            {
                string platformFilter = Core.Platform.ToString(id.PlatformFilter, '|');

                if (id.IsDefaultVersion)
                {
                    Core.Log.MessageAll("{0}{1}* (filter: {2}) (root: '{3}')", new string('\t', depth), id.ToString("-"), platformFilter, id.Root);
                }
                else
                {
                    Core.Log.MessageAll("{0}{1} (filter: {2}) (root: '{3}')", new string('\t', depth), id.ToString("-"), platformFilter, id.Root);
                }

                if ((null != id.Definition) && (id.Definition.PackageIdentifiers.Count > 0))
                {
                    DisplayDependencies(id.Definition, depth + 1);
                }
            }
        }

        public bool Execute()
        {
            // there may be multiple versions of packages - so show them all
            Core.PackageUtilities.IdentifyMainAndDependentPackages(false);

            Core.PackageIdentifier mainPackageId = Core.State.PackageInfo[0].Identifier;
            Core.PackageDefinitionFile definitionFile = mainPackageId.Definition;

            string packageName = mainPackageId.ToString();
            string formatString = "Definition of package '{0}'";
            int dashLength = formatString.Length - 3 + packageName.Length;
            Core.Log.MessageAll("Definition of package '{0}'", mainPackageId.ToString());
            Core.Log.MessageAll(new string('-', dashLength));
            Core.Log.MessageAll("\nSupported on: {0}", Core.Platform.ToString(definitionFile.SupportedPlatforms, ' '));
            Core.Log.MessageAll("\nOpus assemblies:");
            foreach (string opusAssembly in definitionFile.OpusAssemblies)
            {
                Core.Log.MessageAll("\t{0}", opusAssembly);
            }
            Core.Log.MessageAll("\nDotNet assemblies:");
            foreach (Core.DotNetAssemblyDescription desc in definitionFile.DotNetAssemblies)
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
                foreach (string definition in definitionFile.Definitions)
                {
                    Core.Log.MessageAll("\t{0}", definition);
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

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}