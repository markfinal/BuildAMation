#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License

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
                return "Adds a DotNet assembly to the package definition (format: assembly-frameworkversion) (separated by " + System.IO.Path.PathSeparator + ")";
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
#if true
            return false;
#else
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
#endif
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