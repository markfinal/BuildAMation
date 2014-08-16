// <copyright file="ToolchainAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

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
