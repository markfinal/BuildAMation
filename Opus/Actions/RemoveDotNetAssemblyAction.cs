// <copyright file="RemoveDotNetAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveDotNetAssemblyAction))]

namespace Opus
{
    [Core.PreambleAction]
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
                return "Removes a DotNet assembly from the package definition (semi-colon separated)";
            }
        }

        public void AssignArguments(string arguments)
        {
            string[] assemblyNames = arguments.Split(';');
            this.DotNetAssemblyNameArray = new Opus.Core.StringArray(assemblyNames);
        }

        private Core.StringArray DotNetAssemblyNameArray
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
            foreach (string dotNetAssemblyName in this.DotNetAssemblyNameArray)
            {
                Core.DotNetAssemblyDescription foundDesc = null;
                foreach (Core.DotNetAssemblyDescription desc in xmlFile.DotNetAssemblies)
                {
                    if (desc.Name == dotNetAssemblyName)
                    {
                        foundDesc = desc;
                    }
                }

                if (null != foundDesc)
                {
                    xmlFile.DotNetAssemblies.Remove(foundDesc);

                    Core.Log.MessageAll("Removed DotNet assembly '{0}' from package '{1}'", dotNetAssemblyName, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Could not find DotNet assembly '{0}' in package '{1}'", dotNetAssemblyName, mainPackageId.ToString());
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