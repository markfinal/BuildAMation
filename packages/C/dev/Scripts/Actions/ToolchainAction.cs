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

[assembly: Bam.Core.RegisterAction(typeof(C.ToolchainAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class ToolchainAction :
        Bam.Core.IActionWithArguments
    {
        public
        ToolchainAction()
        {
            if (!Bam.Core.State.HasCategory("C"))
            {
                Bam.Core.State.AddCategory("C");
            }

            if (!Bam.Core.State.Has("C", "ToolToToolsetName"))
            {
                var map = new System.Collections.Generic.Dictionary<System.Type, string>();
                Bam.Core.State.Add("C", "ToolToToolsetName", map);
            }
        }

        private string Toolchain
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Toolchain = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.toolchain";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Assign the toolchain used for building C code";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.Log.DebugMessage("C toolchain is '{0}'", this.Toolchain);

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(ICompilerTool)]                   = this.Toolchain;
            map[typeof(ICxxCompilerTool)]                = this.Toolchain;
            map[typeof(IObjCCompilerTool)]               = this.Toolchain;
            map[typeof(IObjCxxCompilerTool)]             = this.Toolchain;
            map[typeof(ILinkerTool)]                     = this.Toolchain;
            map[typeof(IArchiverTool)]                   = this.Toolchain;
            map[typeof(IWinResourceCompilerTool)]        = this.Toolchain;
            map[typeof(INullOpTool)]                     = this.Toolchain;
            map[typeof(IThirdPartyTool)]                 = this.Toolchain;
            map[typeof(IWinManifestTool)]                = this.Toolchain;
            map[typeof(IPosixSharedLibrarySymlinksTool)] = this.Toolchain;

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
