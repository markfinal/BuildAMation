// <copyright file="AddDefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddDefineAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddDefineAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-adddefine";
            }
        }

        public string Description
        {
            get
            {
                return "Adds a #define to the Opus package compilation step";
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

            if (!xmlFile.Definitions.Contains(this.Definition))
            {
                xmlFile.Definitions.Add(this.Definition);
                xmlFile.Write();

                Core.Log.MessageAll("Added #define '{0}' to package '{1}'", this.Definition, mainPackageId.ToString());

                return true;
            }
            else
            {
                Core.Log.MessageAll("#define '{0}' already used by package '{1}'", this.Definition, mainPackageId.ToString());

                return false;
            }
        }
    }
}