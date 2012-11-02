// <copyright file="ArchiverAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.ArchiverAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class ArchiverAction : Opus.Core.IActionWithArguments
    {
        private string Archiver
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Archiver = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.LIB";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the archiver used.";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("Archiver is '{0}'", this.Archiver);

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

            map[typeof(IArchiverTool)] = this.Archiver;

            return true;
        }
    }
}
