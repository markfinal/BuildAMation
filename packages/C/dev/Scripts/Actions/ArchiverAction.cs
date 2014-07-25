// <copyright file="ArchiverAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.ArchiverAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class ArchiverAction :
        Opus.Core.IActionWithArguments
    {
        public
        ArchiverAction()
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

        private string Archiver
        {
            get;
            set;
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Archiver = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.AR";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the archiver used.";
            }
        }

        bool
        Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("Archiver is '{0}'", this.Archiver);

            var map = Opus.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(IArchiverTool)] = this.Archiver;

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
