#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetConfigurationsAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class SetConfigurationsAction :
        Core.IActionWithArguments
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

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Configurations = new Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
        }

        private Core.StringArray Configurations
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            var buildConfigurations = new Core.Array<Core.EConfiguration>();

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

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}