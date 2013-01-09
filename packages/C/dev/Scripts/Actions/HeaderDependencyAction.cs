// <copyright file="HeaderDependencyAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(C.HeaderDependencyAction))]

namespace C
{
    [Opus.Core.PreambleAction]
    public sealed class HeaderDependencyAction : Opus.Core.IAction
    {
        public HeaderDependencyAction()
        {
            if (!Opus.Core.State.HasCategory("C"))
            {
                Opus.Core.State.AddCategory("C");
            }
            Opus.Core.State.Add<bool>("C", "HeaderDependencyGeneration", true);
        }

        string Opus.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.noheaderdeps";
            }
        }

        string Opus.Core.IAction.Description
        {
            get
            {
                return "Disable header dependency generation";
            }
        }

        bool Opus.Core.IAction.Execute()
        {
            Opus.Core.State.Set("C", "HeaderDependencyGeneration", false);

            Opus.Core.Log.DebugMessage("C header dependency generation has been disabled");

            return true;
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
