// <copyright file="RemoveDotNetAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.RemoveDotNetAssemblyAction))]

namespace Bam
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
                return "Removes a DotNet assembly from the package definition (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var assemblyNames = arguments.Split(System.IO.Path.PathSeparator);
            this.DotNetAssemblyNameArray = new Opus.Core.StringArray(assemblyNames);
        }

        private Core.StringArray DotNetAssemblyNameArray
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
            foreach (var dotNetAssemblyName in this.DotNetAssemblyNameArray)
            {
                Core.DotNetAssemblyDescription foundDesc = null;
                foreach (var desc in xmlFile.DotNetAssemblies)
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

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}