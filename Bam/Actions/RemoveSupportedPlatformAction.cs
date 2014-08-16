// <copyright file="RemoveSupportedPlatformAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveSupportedPlatformAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class RemoveSupportedPlatformAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-removesupportedplatform";
            }
        }

        public string Description
        {
            get
            {
                return "Remove a supported platform from the package (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var platforms = arguments.Split(System.IO.Path.PathSeparator);
            this.PlatformArray = new Opus.Core.StringArray(platforms);
        }

        private Core.StringArray PlatformArray
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

            var success = false;
            foreach (var supportedPlatform in this.PlatformArray)
            {
                var platform = Core.Platform.FromString(supportedPlatform);

                if (Core.Platform.Contains(xmlFile.SupportedPlatforms, platform))
                {
                    xmlFile.SupportedPlatforms &= ~platform;

                    Core.Log.MessageAll("Removed supported platform '{0}' from package '{1}'", supportedPlatform, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Platform '{0}' was already not supported by package '{1}'", supportedPlatform, mainPackageId.ToString());
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