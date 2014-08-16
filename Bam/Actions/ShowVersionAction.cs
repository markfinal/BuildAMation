﻿// <copyright file="ShowVersionAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.ShowVersionAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class ShowVersionAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-version";
            }
        }

        public string Description
        {
            get
            {
                return "Display the version of Opus";
            }
        }

        public bool
        Execute()
        {
            Core.Log.MessageAll(Core.State.OpusVersionString);

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