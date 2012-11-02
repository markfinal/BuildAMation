// <copyright file="WinResourceCompilerAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.WinResourceCompilerAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class WinResourceCompilerAction : Opus.Core.IActionWithArguments
    {
        private string WinResourceCompiler
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.WinResourceCompiler = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.RC";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the Windows resource compiler used.";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("Windows resource compiler is '{0}'", this.WinResourceCompiler);

            System.Collections.Generic.Dictionary<System.Type, string> map = null;
            if (Opus.Core.State.Has("Toolchains", "Map"))
            {
                map = Opus.Core.State.Get("Toolchains", "Map") as System.Collections.Generic.Dictionary<System.Type, string>;
            }
            else
            {
                map = new System.Collections.Generic.Dictionary<System.Type, string>();
                Opus.Core.State.Add("Toolchains", "Map", map);
            }

            map[typeof(IWinResourceCompilerTool)] = this.WinResourceCompiler;

            return true;
        }
    }
}
