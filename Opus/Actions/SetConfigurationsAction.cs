﻿// <copyright file="SetConfigurationsAction.cs" company="Mark Final">
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
                return "Set configurations to build";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Configurations = new Opus.Core.StringArray(arguments.Split(';'));
        }

        private Core.StringArray Configurations
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.Array<Core.EConfiguration> buildConfigurations = new Opus.Core.Array<Opus.Core.EConfiguration>();

            foreach (string configuration in this.Configurations)
            {
                Core.EConfiguration c = Core.EConfiguration.Invalid;
                foreach (Core.EConfiguration config in System.Enum.GetValues(typeof(Core.EConfiguration)))
                {
                    if (config.ToString().ToLower() == configuration)
                    {
                        c = config;
                        break;
                    }
                }

                if (Core.EConfiguration.Invalid == c)
                {
                    throw new Core.Exception(System.String.Format("Configuration '{0}' not recognized", configuration), false);
                }

                if (buildConfigurations.Contains(c))
                {
                    throw new Core.Exception(System.String.Format("Configuration '{0}' already specified", configuration), false);
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
    }
}