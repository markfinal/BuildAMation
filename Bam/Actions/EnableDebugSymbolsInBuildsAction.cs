// <copyright file="EnableDebugSymbolsInBuildsAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.EnableDebugSymbolsInBuildsAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class EnableDebugSymbolsInBuildsAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-debugsymbols";
            }
        }

        public string Description
        {
            get
            {
                return "Enable debug symbols in package builds";
            }
        }

        public bool
        Execute()
        {
            Core.State.CompileWithDebugSymbols = true;

            Core.Log.Detail("Package builds will use debug symbols");

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