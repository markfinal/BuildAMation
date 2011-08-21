// <copyright file="HelpAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.HelpAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class HelpAction : Core.IAction
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

        public bool Execute()
        {
            Core.Log.MessageAll("Syntax: Opus [@<response file >] <command 0>[=<value 0>] [<command 1>[=<value 1>] .. <command N>[=<value N>]]");

            Core.Log.MessageAll("\nCommands are processed left to right. Response file commands are inlined.");
            Core.Log.MessageAll("Repeated commands will always take the last value set.");
            Core.Log.MessageAll("There can be only one response file.");

            Core.Array<Core.RegisterActionAttribute> actions = Core.ActionManager.Actions;
            
            // first loop over these to determine the command length
            int maximumLength = 0;
            foreach (Core.RegisterActionAttribute actionAttribute in actions)
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

        private void DisplayCommands(Core.Array<Core.RegisterActionAttribute> actions,
                                     int maximumLength)
        {
            foreach (Core.RegisterActionAttribute actionAttribute in actions)
            {
                Core.IAction action = actionAttribute.Action;

                string commandSwitch = action.CommandLineSwitch;
                string description = action.Description;

                int length = commandSwitch.Length;
                int spaces = maximumLength - length;

                Core.Log.MessageAll("\t{0}{1}{2}", commandSwitch, new string(' ', spaces), description);
            }
        }
    }
}