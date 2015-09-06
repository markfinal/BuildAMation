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

[assembly: Bam.Core.RegisterAction(typeof(Bam.AddDefineAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class AddDefineAction :
        Core.IActionWithArguments
    {
        public string
        CommandLineSwitch
        {
            get
            {
                return "-adddefine";
            }
        }

        public string Description
        {
            get
            {
                return "Adds a #define to the package definition and to BuildAMation package compilation step (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var definitions = arguments.Split(System.IO.Path.PathSeparator);
            this.DefinitionArray = new Core.StringArray(definitions);
        }

        private Core.StringArray DefinitionArray
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

            var success = false;
            foreach (var definition in this.DefinitionArray)
            {
                if (!xmlFile.Definitions.Contains(definition))
                {
                    xmlFile.Definitions.Add(definition);

                    Core.Log.MessageAll("Added #define '{0}' to package '{1}'", definition, mainPackageId.ToString());

                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("#define '{0}' already used by package '{1}'", definition, mainPackageId.ToString());
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