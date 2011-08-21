// <copyright file="RemoveOpusAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveOpusAssemblyAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class RemoveOpusAssemblyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removeopusassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Removes an Opus assembly from the package definition";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.OpusAssemblyName = arguments;
        }

        private string OpusAssemblyName
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

            if (xmlFile.OpusAssemblies.Contains(this.OpusAssemblyName))
            {
                xmlFile.OpusAssemblies.Remove(this.OpusAssemblyName);
                xmlFile.Write();

                Core.Log.MessageAll("Removed Opus assembly '{0}' from package '{1}'", this.OpusAssemblyName, mainPackageId.ToString());

                return true;
            }
            else
            {
                Core.Log.MessageAll("Opus assembly '{0}' was not used by package '{1}'", this.OpusAssemblyName, mainPackageId.ToString());

                return false;
            }
        }
    }
}