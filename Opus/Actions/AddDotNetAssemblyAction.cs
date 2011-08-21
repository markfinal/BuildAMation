// <copyright file="AddDotNetAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddDotNetAssemblyAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddDotNetAssemblyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-adddotnetassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Adds a DotNet assembly to the package definition";
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

            string assemblyName = null;
            string targetVersion = null;
            if (this.DotNetAssemblyName.Contains("-"))
            {
                string[] split = this.DotNetAssemblyName.Split('-');
                if (split.Length != 2)
                {
                    throw new Core.Exception(System.String.Format("DotNet assembly name and version is ill-formed: '{0}'", this.DotNetAssemblyName), false);
                }

                assemblyName = split[0];
                targetVersion = split[1];
            }
            else
            {
                assemblyName = this.DotNetAssemblyName;
            }

            foreach (Core.DotNetAssemblyDescription desc in xmlFile.DotNetAssemblies)
            {
                if (desc.Name == assemblyName)
                {
                    throw new Core.Exception(System.String.Format("DotNet assembly '{0}' already referenced by the package", assemblyName), false);
                }
            }

            Core.DotNetAssemblyDescription descToAdd = new Core.DotNetAssemblyDescription(assemblyName);
            if (null != targetVersion)
            {
                descToAdd.RequiredTargetFramework = targetVersion;
            }
            xmlFile.DotNetAssemblies.Add(descToAdd);
            xmlFile.Write();

            if (null != targetVersion)
            {
                Core.Log.MessageAll("Added DotNet assembly '{0}', framework version '{1}', to package '{2}'", assemblyName, targetVersion, mainPackageId.ToString());
            }
            else
            {
                Core.Log.MessageAll("Added DotNet assembly '{0}' to package '{1}'", assemblyName, mainPackageId.ToString());
            }

            return true;
        }
    }
}