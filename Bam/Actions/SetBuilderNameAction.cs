// <copyright file="SetBuilderNameAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetBuilderNameAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class SetBuilderNameAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-builder";
            }
        }

        public string Description
        {
            get
            {
                return "Specify the name of the builder to use";
            }
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.BuilderName = arguments;
        }

        private string BuilderName
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            Core.State.BuilderName = this.BuilderName;

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