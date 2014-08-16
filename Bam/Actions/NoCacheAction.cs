// <copyright file="NoCacheAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.NoCacheAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class NoCacheAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-nocache";
            }
        }

        public string Description
        {
            get
            {
                return "Do not create/use the package assembly cache";
            }
        }

        public bool
        Execute()
        {
            Core.State.CacheAssembly = false;
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