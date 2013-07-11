// <copyright file="AddDefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddDefineAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddDefineAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
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
                return "Adds a #define to the package definition and to Opus package compilation step (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            var definitions = arguments.Split(System.IO.Path.PathSeparator);
            this.DefinitionArray = new Opus.Core.StringArray(definitions);
        }

        private Core.StringArray DefinitionArray
        {
            get;
            set;
        }

        public bool Execute()
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
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}