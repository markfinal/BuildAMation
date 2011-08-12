// <copyright file="AddSupportedPlatformAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddSupportedPlatformAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddSupportedPlatformAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addsupportedplatform";
            }
        }

        public string Description
        {
            get
            {
                return "Adds a supported platform to the package";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Platform = arguments;
        }

        private string Platform
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

            Core.EPlatform platform = Core.Platform.FromString(this.Platform);

            if (!Core.Platform.Contains(xmlFile.SupportedPlatforms, platform))
            {
                xmlFile.SupportedPlatforms |= platform;
                xmlFile.Write();

                Core.Log.MessageAll("Added platform '{0}' to package '{1}'", this.Platform, mainPackageId.ToString());

                return true;
            }
            else
            {
                Core.Log.MessageAll("Platform '{0}' already used by package '{1}'", this.Platform, mainPackageId.ToString());

                return false;
            }
        }
    }
}