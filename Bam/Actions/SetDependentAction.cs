// <copyright file="SetDependentAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetDependentAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class SetDependentAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-dependent";
            }
        }

        public string Description
        {
            get
            {
                return "Specify the dependent package to act upon";
            }
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.DependentPackageAndVersion = arguments;
        }

        public string DependentPackageAndVersion
        {
            get;
            private set;
        }

        public bool
        Execute()
        {
            // TODO: might want to figure out the PackageIdentifier here
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