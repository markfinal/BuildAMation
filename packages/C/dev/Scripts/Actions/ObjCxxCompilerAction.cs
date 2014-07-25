// <copyright file="ObjCxxCompilerAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.ObjCxxCompilerAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class ObjCxxCompilerAction :
        Opus.Core.IActionWithArguments
    {
        public
        ObjCxxCompilerAction()
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

        private string Compiler
        {
            get;
            set;
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Compiler = arguments;
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.OBJCCXX";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Assign the compiler used for building ObjectiveC++ code";
            }
        }

        bool
        Opus.Core.IAction.Execute()
        {
            Opus.Core.Log.DebugMessage("ObjectiveC++ compiler is '{0}'", this.Compiler);

            var map = Opus.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(IObjCCompilerTool)] = this.Compiler;

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
