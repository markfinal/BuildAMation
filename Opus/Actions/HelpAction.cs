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
            Core.Log.MessageAll("Syntax: Opus [@<response file > | <command 0> <command 1> .. <command N>]");

            Core.Log.MessageAll("Commands:");
            var actionAttributeArray = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(Core.RegisterActionAttribute), false);
            foreach (Core.RegisterActionAttribute actionAttribute in actionAttributeArray)
            {
                Core.IAction action = actionAttribute.Action;

                string commandSwitch = action.CommandLineSwitch;
                string description = action.Description;

                // TODO: this is a bit hacky
                if (commandSwitch.Length < 6)
                {
                    Core.Log.MessageAll("\t{0}\t\t\t{1}", commandSwitch, description);
                }
                else if (commandSwitch.Length < 15)
                {
                    Core.Log.MessageAll("\t{0}\t\t{1}", commandSwitch, description);
                }
                else
                {
                    Core.Log.MessageAll("\t{0}\t{1}", commandSwitch, description);
                }
            }

            return true;
        }
    }
}