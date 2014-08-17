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

[assembly: Bam.Core.RegisterAction(typeof(MingwCommon.InstallPathAction))]

namespace MingwCommon
{
    [Bam.Core.PreambleAction]
    public sealed class InstallPathAction :
        Bam.Core.IActionWithArguments
    {
        private string InstallPath
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.InstallPath = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-mingw.installpath";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Override the MinGW installation path";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            if (!System.IO.Directory.Exists(this.InstallPath))
            {
                var absolutePath = System.IO.Path.Combine(Bam.Core.State.WorkingDirectory, this.InstallPath);
                if (!System.IO.Directory.Exists(absolutePath))
                {
                    throw new Bam.Core.Exception("Path '{0}' does not exist and is not relative to the working directory '{1}", this.InstallPath, Bam.Core.State.WorkingDirectory);
                }

                this.InstallPath = absolutePath;
            }

            Bam.Core.State.AddCategory("Mingw");
            Bam.Core.State.Set("Mingw", "InstallPath", this.InstallPath);

            Bam.Core.Log.DebugMessage("Mingw installation path is now '{0}'", this.InstallPath);

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
