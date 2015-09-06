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

[assembly: Bam.Core.RegisterAction(typeof(Bam.CreatePackageAtAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class CreatePackageAtAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-createpackageat";
            }
        }

        public string Description
        {
            get
            {
                return "Create a new package at the specified path.";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.PackagePath = arguments;
        }

        private string PackagePath
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
            var id = Core.PackageUtilities.IsPackageDirectory(this.PackagePath, out isWellDefined);
            if ((null != id) || isWellDefined)
            {
                throw new Core.Exception("Package already present at '{0}'", this.PackagePath);
            }

            var PackageDirectory = this.PackagePath;
            if (System.IO.Directory.Exists(PackageDirectory))
            {
                Core.Log.Info("Package directory already exists at '{0}'", PackageDirectory);
                return false;
            }
            else
            {
                System.IO.Directory.CreateDirectory(PackageDirectory);
            }

            id = Core.PackageUtilities.IsPackageDirectory(this.PackagePath, out isWellDefined);

            // Xml file for dependencies
            var packageDefinition = new Core.PackageDefinitionFile(id.DefinitionPathName, true);
            if (null == packageDefinition)
            {
                throw new Core.Exception("Package definition file '%s' could not be created", packageDefinition);
            }

            if (Core.State.PackageCreationDependents != null)
            {
                Core.Log.DebugMessage("Adding dependent packages:");
                foreach (var dependentPackage in Core.State.PackageCreationDependents)
                {
                    Core.Log.DebugMessage("\t'{0}'", dependentPackage);
                    var packageNameAndVersion = dependentPackage.Split('-');
                    if (packageNameAndVersion.Length != 2)
                    {
                        throw new Core.Exception("Ill-formed package name-version pair, '{0}'", packageNameAndVersion);
                    }

                    var idToAdd = new Core.PackageIdentifier(packageNameAndVersion[0], packageNameAndVersion[1]);
                    packageDefinition.AddRequiredPackage(idToAdd);
                }
            }
            packageDefinition.BamAssemblies.Add("Bam.Core");
            {
                var system = new Core.DotNetAssemblyDescription("System");
                system.RequiredTargetFramework = "4.0.30319";
                packageDefinition.DotNetAssemblies.Add(system);

                var systemXml = new Core.DotNetAssemblyDescription("System.Xml");
                systemXml.RequiredTargetFramework = "4.0.30319";
                packageDefinition.DotNetAssemblies.Add(systemXml);

                var systemCore = new Core.DotNetAssemblyDescription("System.Core");
                systemCore.RequiredTargetFramework = "4.0.30319";
                packageDefinition.DotNetAssemblies.Add(systemCore);
            }
            packageDefinition.Write();
            id.Definition = packageDefinition;

            // Script file
            var packageScriptPathname = id.ScriptPathName;
            using (var scriptWriter = new System.IO.StreamWriter(packageScriptPathname))
            {
                scriptWriter.WriteLine("// Automatically generated by BuildAMation v" + Core.State.VersionString);
                scriptWriter.WriteLine("namespace " + id.Name);
                scriptWriter.WriteLine("{");
                scriptWriter.WriteLine("}");
                scriptWriter.Close();
            }

            Core.Log.Info("Successfully created package '{0}' in '{1}'", id.ToString("-"), id.Root.AbsolutePath);

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