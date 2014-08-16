// <copyright file="HeaderDependencyAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(C.HeaderDependencyAction))]

namespace C
{
    [Bam.Core.PreambleAction]
    public sealed class HeaderDependencyAction :
        Bam.Core.IAction
    {
        public
        HeaderDependencyAction()
        {
            if (!Bam.Core.State.HasCategory("C"))
            {
                Bam.Core.State.AddCategory("C");
            }
            Bam.Core.State.Add<bool>("C", "HeaderDependencyGeneration", true);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-C.noheaderdeps";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Disable header dependency generation";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("C", "HeaderDependencyGeneration", false);

            Bam.Core.Log.DebugMessage("C header dependency generation has been disabled");

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
