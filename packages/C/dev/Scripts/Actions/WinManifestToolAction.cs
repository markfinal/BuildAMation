// <copyright file="WinManifestToolAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(C.WinManifestToolAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class WinManifestToolAction :
        Bam.Core.IActionWithArguments
    {
        public
        WinManifestToolAction()
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

        private string WinManifestTool
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.WinManifestTool = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.MT";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Assign the Windows manifest tool used.";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.Log.DebugMessage("Windows manifest tool is '{0}'", this.WinManifestTool);

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(IWinManifestTool)] = this.WinManifestTool;

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
