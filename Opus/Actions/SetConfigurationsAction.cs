// <copyright file="SetConfigurationsAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetConfigurationsAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetConfigurationsAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-configurations";
            }
        }

        public string Description
        {
            get
            {
                return "Set configurations to build (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Configurations = new Opus.Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
        }

        private Core.StringArray Configurations
        {
            get;
            set;
        }

        public bool Execute()
        {
            var buildConfigurations = new Core.Array<Opus.Core.EConfiguration>();

            foreach (var configuration in this.Configurations)
            {
                var c = Core.Configuration.FromString(configuration);
                if (buildConfigurations.Contains(c))
                {
                    throw new Core.Exception("Configuration '{0}' already specified", configuration);
                }
                else
                {
                    Core.Log.DebugMessage("Adding configuration '{0}'", c.ToString());
                    buildConfigurations.Add(c);
                }
            }

            Core.State.BuildConfigurations = buildConfigurations;

            return true;
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}