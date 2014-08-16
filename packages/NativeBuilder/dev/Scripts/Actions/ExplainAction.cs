// <copyright file="ExplainAction.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(NativeBuilder.ExplainAction))]

namespace NativeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class ExplainAction :
        Bam.Core.IAction
    {
        public
        ExplainAction()
        {
            if (!Bam.Core.State.HasCategory("NativeBuilder"))
            {
                Bam.Core.State.AddCategory("NativeBuilder");
            }
            Bam.Core.State.Add<bool>("NativeBuilder", "Explain", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-explain";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Explain why builds occur due to dependency checking";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("NativeBuilder", "Explain", true);

            Bam.Core.Log.DebugMessage("Explained builds are enabled");

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
