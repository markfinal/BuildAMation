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
#endregion // License

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetPlatformsAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class SetPlatformsAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-platforms";
            }
        }

        public string Description
        {
            get
            {
                return "Set platforms to build (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Platforms = new Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
        }

        private Core.StringArray Platforms
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            var buildPlatforms = new Core.Array<Core.EPlatform>();

            foreach (var platform in this.Platforms)
            {
                var p = Core.Platform.FromString(platform);
                if (buildPlatforms.Contains(p))
                {
                    throw new Core.Exception("Platform '{0}' already specified", platform);
                }
                else
                {
                    Core.Log.DebugMessage("Adding platform '{0}'", p.ToString());
                    buildPlatforms.Add(p);
                }
            }

            Core.State.BuildPlatforms = buildPlatforms;

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