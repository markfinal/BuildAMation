// <copyright file="CxxCompilerAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(C.CxxCompilerAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class CxxCompilerAction :
        Bam.Core.IActionWithArguments
    {
        public
        CxxCompilerAction()
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

        private string Compiler
        {
            get;
            set;
        }

        void
        Bam.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Compiler = arguments;
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.CXX";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Assign the compiler used for building C++ code";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.Log.DebugMessage("C++ compiler is '{0}'", this.Compiler);

            var map = Bam.Core.State.Get("C", "ToolToToolsetName") as System.Collections.Generic.Dictionary<System.Type, string>;
            map[typeof(ICxxCompilerTool)] = this.Compiler;

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
