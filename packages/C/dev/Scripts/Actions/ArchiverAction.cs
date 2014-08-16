// <copyright file="ArchiverAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(C.ArchiverAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class ArchiverAction :
        Bam.Core.IActionWithArguments
    {
        public
        ArchiverAction()
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

        private string Archiver
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Archiver = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.AR";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Assign the archiver used.";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.Log.DebugMessage("Archiver is '{0}'", this.Archiver);

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
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
