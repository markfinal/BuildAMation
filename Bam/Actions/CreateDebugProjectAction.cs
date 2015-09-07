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

[assembly: Bam.Core.RegisterAction(typeof(Bam.CreateDebugProjectAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class CreateDebugProjectAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-createdebugproject";
            }
        }

        public string Description
        {
            get
            {
                return "Create a VisualStudio project of the package scripts and dependencies to debug";
            }
        }

        private string PackagePath
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            // TODO: would be nice not to have to resolve down to a single version in the debug project
            // but there are namespace clashes if you do
            Core.PackageUtilities.IdentifyMainAndDependentPackages(true, false);

#if true
#else
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Package has not been specified. Re-run from the package directory.");
            }
#endif

            var fatal = false;
            Core.PackageUtilities.ProcessLazyArguments(fatal);
            Core.PackageUtilities.HandleUnprocessedArguments(fatal);

#if true
#else
            var mainPackage = Core.State.PackageInfo.MainPackage;

            Core.Log.DebugMessage("Package is '{0}' in '{1}'", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root.GetSingleRawPath());
#endif

            // this is now optional - if you pass -builder=<name> then the generated package will be limited to that
            // otherwise, all packages with names ending in 'Builder' will have their scripts added
            Core.BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            var resourceFilePathName = Core.PackageListResourceFile.WriteResXFile(null);

#if true
#else
            // Project to debug the script
            CSharpProject.Create(mainPackage, VisualStudioVersion.VS2008, new string[] { resourceFilePathName });

            Core.Log.Info("Successfully created debug project for package '{0}'",
                          mainPackage.Identifier.ToString("-"));
            Core.Log.Info("\t{0}",
                          mainPackage.DebugProjectFilename);
#endif

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