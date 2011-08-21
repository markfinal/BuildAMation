// <copyright file="RemoveDotNetAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveDotNetAssemblyAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class RemoveDotNetAssemblyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removedotnetassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Removes a DotNet assembly from the package definition";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.DotNetAssemblyName = arguments;
        }

        private string DotNetAssemblyName
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

            Core.DotNetAssemblyDescription foundDesc = null;
            foreach (Core.DotNetAssemblyDescription desc in xmlFile.DotNetAssemblies)
            {
                if (desc.Name == this.DotNetAssemblyName)
                {
                    foundDesc = desc;
                }
            }

            if (null != foundDesc)
            {
                xmlFile.DotNetAssemblies.Remove(foundDesc);
                xmlFile.Write();

                Core.Log.MessageAll("Removed DotNet assembly '{0}' from package '{1}'", this.DotNetAssemblyName, mainPackageId.ToString());

                return true;
            }
            else
            {
                Core.Log.MessageAll("Could not find DotNet assembly '{0}' in package '{1}'", this.DotNetAssemblyName, mainPackageId.ToString());

                return false;
            }
        }
    }
}