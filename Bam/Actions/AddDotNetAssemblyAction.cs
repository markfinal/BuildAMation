// <copyright file="AddDotNetAssemblyAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddDotNetAssemblyAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddDotNetAssemblyAction :
        Core.IActionWithArguments
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
                return "Adds a DotNet assembly to the package definition (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var assemblyNames = arguments.Split(System.IO.Path.PathSeparator);
            this.DotNetAssemblyNameArray = new Core.StringArray(assemblyNames);
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

            foreach (var dotNetAssemblyName in this.DotNetAssemblyNameArray)
            {
                string assemblyName = null;
                string targetVersion = null;
                if (dotNetAssemblyName.Contains("-"))
                {
                    var split = dotNetAssemblyName.Split('-');
                    if (split.Length != 2)
                    {
                        throw new Core.Exception("DotNet assembly name and version is ill-formed: '{0}'", dotNetAssemblyName);
                    }

                    assemblyName = split[0];
                    targetVersion = split[1];
                }
                else
                {
                    assemblyName = dotNetAssemblyName;
                }

                foreach (var desc in xmlFile.DotNetAssemblies)
                {
                    if (desc.Name == assemblyName)
                    {
                        throw new Core.Exception("DotNet assembly '{0}' already referenced by the package", assemblyName);
                    }
                }

                var descToAdd = new Core.DotNetAssemblyDescription(assemblyName);
                if (null != targetVersion)
                {
                    descToAdd.RequiredTargetFramework = targetVersion;
                }
                xmlFile.DotNetAssemblies.Add(descToAdd);

                if (null != targetVersion)
                {
                    Core.Log.MessageAll("Added DotNet assembly '{0}', framework version '{1}', to package '{2}'", assemblyName, targetVersion, mainPackageId.ToString());
                }
                else
                {
                    Core.Log.MessageAll("Added DotNet assembly '{0}' to package '{1}'", assemblyName, mainPackageId.ToString());
                }
            }

            xmlFile.Write();

            return true;
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