// <copyright file="ToolchainAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.ToolchainAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class ToolchainAction : Opus.Core.IActionWithArguments
    {
        public ToolchainAction()
        {
            if (!Opus.Core.State.HasCategory("C"))
            {
                Opus.Core.State.AddCategory("C");
            }

            if (!Opus.Core.State.Has("C", "ToolToToolsetName"))
            {
                var map = new System.Collections.Generic.Dictionary<System.Type, string>();
                Opus.Core.State.Add("C", "ToolToToolsetName", map);
            }
        }

        private string Toolchain
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Toolchain = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.toolchain";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the toolchain used for building C code";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("C toolchain is '{0}'", this.Toolchain);

            var map = Opus.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(ICompilerTool)]    = this.Toolchain;
            map[typeof(ICxxCompilerTool)] = this.Toolchain;
            map[typeof(ILinkerTool)]      = this.Toolchain;
            map[typeof(IArchiverTool)]    = this.Toolchain;
            map[typeof(IWinResourceCompilerTool)] = this.Toolchain;

            return true;
        }
    }
}
