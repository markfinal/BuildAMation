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
                return "Removes a #define from the Opus package compilation step";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Definition = arguments;
        }

        private string Definition
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

            if (xmlFile.Definitions.Contains(this.Definition))
            {
                xmlFile.Definitions.Remove(this.Definition);
                xmlFile.Write();

                Core.Log.MessageAll("Removed #define '{0}' from package '{1}'", this.Definition, mainPackageId.ToString());

                return true;
            }
            else
            {
                Core.Log.MessageAll("#define '{0}' was not used by package '{1}'", this.Definition, mainPackageId.ToString());

                return false;
            }
        }
    }
}