// <copyright file="RemoveDefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.RemoveDefineAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class RemoveDefineAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removedefine";
            }
        }

        public string Description
        {
            get
            {
                return "Removes a #define from the Opus package compilation step (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var definitions = arguments.Split(System.IO.Path.PathSeparator);
            this.DefinitionArray = new Core.StringArray(definitions);
        }

        private Core.StringArray DefinitionArray
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
            foreach (var definition in this.DefinitionArray)
            {
                if (xmlFile.Definitions.Contains(definition))
                {
                    xmlFile.Definitions.Remove(definition);

                    Core.Log.MessageAll("Removed #define '{0}' from package '{1}'", definition, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("#define '{0}' was not used by package '{1}'", definition, mainPackageId.ToString());
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