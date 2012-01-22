// <copyright file="RemoveDefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveDefineAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class RemoveDefineAction : Core.IActionWithArguments
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
                return "Removes a #define from the Opus package compilation step (semi-colon separated)";
            }
        }

        public void AssignArguments(string arguments)
        {
            string[] definitions = arguments.Split(';');
            this.DefinitionArray = new Opus.Core.StringArray(definitions);
        }

        private Core.StringArray DefinitionArray
        {
            get;
            set;
        }

        public bool Execute()
        {
            bool isComplete;
            Core.PackageIdentifier mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isComplete);
            if (null == mainPackageId)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a package", Core.State.WorkingDirectory), false);
            }
            if (!isComplete)
            {
                throw new Core.Exception(System.String.Format("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory), false);
            }

            Core.PackageDefinitionFile xmlFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isComplete)
            {
                xmlFile.Read();
            }

            bool success = false;
            foreach (string definition in this.DefinitionArray)
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
    }
}