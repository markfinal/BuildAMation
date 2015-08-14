#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License

[assembly: Bam.Core.RegisterAction(typeof(Bam.HelpAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class HelpAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-help";
            }
        }

        public string Description
        {
            get
            {
                return "Display help text";
            }
        }

        public bool
        Execute()
        {
            Core.Log.MessageAll("Syntax: bam [@<response file >] <command 0>[=<value 0>] [<command 1>[=<value 1>] .. <command N>[=<value N>]]");

            Core.Log.MessageAll("\nCommands are processed left to right. Response file commands are inlined.");
            Core.Log.MessageAll("Repeated commands will always take the last value set.");
            Core.Log.MessageAll("There can be only one response file.");

            var actions = Core.ActionManager.Actions;

            // first loop over these to determine the command length
            int maximumLength = 0;
            foreach (var actionAttribute in actions)
            {
                int length = actionAttribute.Action.CommandLineSwitch.Length;
                if (length > maximumLength)
                {
                    maximumLength = length;
                }
            }
            maximumLength++;

            Core.Log.MessageAll("\nCommands that take effect immediately");
            DisplayCommands(Core.ActionManager.ImmediateActions, maximumLength);

            Core.Log.MessageAll("\nCommands that set up state for future commands");
            DisplayCommands(Core.ActionManager.PreambleActions, maximumLength);

            Core.Log.MessageAll("\nCommands that trigger events");
            DisplayCommands(Core.ActionManager.TriggerActions, maximumLength);

            return true;
        }

        private void
        DisplayCommands(
            Core.Array<Core.RegisterActionAttribute> actions,
            int maximumLength)
        {
            foreach (var actionAttribute in actions)
            {
                var action = actionAttribute.Action;
                var commandSwitch = action.CommandLineSwitch;
                var description = action.Description;

                int length = commandSwitch.Length;
                int spaces = maximumLength - length;

                Core.Log.MessageAll("\t{0}{1}{2}", commandSwitch, new string(' ', spaces), description);
            }
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