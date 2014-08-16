// <copyright file="WinResourceCompilerAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(C.WinResourceCompilerAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class WinResourceCompilerAction :
        Bam.Core.IActionWithArguments
    {
        public
        WinResourceCompilerAction()
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

        private string WinResourceCompiler
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.WinResourceCompiler = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.RC";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Assign the Windows resource compiler used.";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.Log.DebugMessage("Windows resource compiler is '{0}'", this.WinResourceCompiler);

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(IWinResourceCompilerTool)] = this.WinResourceCompiler;

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
