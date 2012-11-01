// <copyright file="CCompilerAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.CCompilerAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class CCompilerAction : Opus.Core.IActionWithArguments
    {
        private string Compiler
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Compiler = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.CC";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the compiler used for building C code";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("C compiler is '{0}'", this.Compiler);

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

            map[typeof(C.ICompilerTool)] = this.Compiler;

            return true;
        }
    }
}
