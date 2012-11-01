// <copyright file="LinkerAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.LinkerAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class LinkerAction : Opus.Core.IActionWithArguments
    {
        private string Linker
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Linker = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.LINK";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the linker used.";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("Linker is '{0}'", this.Linker);

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

            map[typeof(C.ILinkerTool)] = this.Linker;

            return true;
        }
    }
}
