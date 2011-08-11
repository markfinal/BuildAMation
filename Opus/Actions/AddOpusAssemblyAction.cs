// <copyright file="AddOpusAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddOpusAssemblyAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddOpusAssemblyAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addopusassembly";
            }
        }

        public string Description
        {
            get
            {
                return "Adds an Opus assembly to the package definition";
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

            if (!xmlFile.OpusAssemblies.Contains(this.OpusAssemblyName))
            {
                xmlFile.OpusAssemblies.Add(this.OpusAssemblyName);
                xmlFile.Write();

                Core.Log.MessageAll("Added Opus assembly '{0}' to package '{1}'", this.OpusAssemblyName, mainPackageId.ToString());

                return true;
            }
            else
            {
                Core.Log.MessageAll("Opus assembly '{0}' already used by package '{1}'", this.OpusAssemblyName, mainPackageId.ToString());

                return false;
            }
        }
    }
}