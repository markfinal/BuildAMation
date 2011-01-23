// <copyright file="Application.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>
namespace Opus
{
    /// <summary>
    /// Opus Application class.
    /// </summary>
    public sealed class Application
    {
        private Core.Array<Core.IAction> preambleActions = new Opus.Core.Array<Opus.Core.IAction>();
        private Core.IAction triggerAction = null;

        private static void displayInfo(Core.EVerboseLevel level)
        {
            Core.Log.Message(level, "Opus location: '{0}'", Core.State.OpusDirectory);
            Core.Log.Message(level, "Opus version : '{0}'", Core.State.OpusVersionString);
            Core.Log.Message(level, "Working dir  : '{0}'", Core.State.WorkingDirectory);
            Core.Log.Message(level, "");
        }
        
        /// <summary>
        /// Initializes a new instance of the Application class.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public Application(string[] args)
        {
            // handle response file
            if (1 == args.Length)
            {
                if (args[0].StartsWith("@"))
                {
                    string responseFile = args[0].Substring(1);
                    if (!System.IO.File.Exists(responseFile))
                    {
                        throw new Core.Exception(System.String.Format("Response file '{0}' does not exist", responseFile));
                    }

                    using (System.IO.TextReader responseFileReader = new System.IO.StreamReader(responseFile))
                    {
                        string responseFileArguments = responseFileReader.ReadToEnd();
                        string[] arguments = responseFileArguments.Split(new string[] { " ", "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                        int commentedArgumentCount = 0;
                        foreach (string argument in arguments)
                        {
                            if (argument.StartsWith("#"))
                            {
                                ++commentedArgumentCount;
                            }
                        }
                        if (commentedArgumentCount > 0)
                        {
                            string[] uncommentedArguments = new string[arguments.Length - commentedArgumentCount];
                            int index = 0;
                            foreach (string argument in arguments)
                            {
                                if (!argument.StartsWith("#"))
                                {
                                    uncommentedArguments[index] = argument;
                                    ++index;
                                }
                            }
                            args = uncommentedArguments;
                        }
                        else
                        {
                            args = arguments;
                        }
                    }
                }
            }

            var actionAttributeArray = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(Opus.Core.RegisterActionAttribute), false);
            foreach (Core.RegisterActionAttribute actionAttribute in actionAttributeArray)
            {
                Core.IAction action = actionAttribute.Action;

                var actionType = action.GetType().GetCustomAttributes(false);
                if (0 == actionType.Length)
                {
                    throw new Core.Exception(System.String.Format("Action '{0}' does not have a type attribute", action.GetType().ToString()));
                }
            }

            foreach (string command in args)
            {
                string[] splitCommand = command.Split('=');
                string commandName = splitCommand[0];
                commandName = commandName.Trim(new char[] { '\n', '\r' });
                string commandValue = null;
                if (splitCommand.Length > 1)
                {
                    commandValue = splitCommand[1];
                    commandValue = commandValue.Trim(new char[] { '"', '\'', '\n', '\r' });
                }
                Core.Log.DebugMessage("Added command '{0}' with value '{1}'", commandName, commandValue);
                if (commandName.StartsWith("@"))
                {
                    throw new Core.Exception("Response files must be the only argument", false);
                }

                bool foundAction = false;
                foreach (Core.RegisterActionAttribute actionAttribute in actionAttributeArray)
                {
                    Core.IAction action = actionAttribute.Action;
                    if (action.CommandLineSwitch == commandName)
                    {
                        if (action is Core.IActionWithArguments)
                        {
                            (action as Core.IActionWithArguments).AssignArguments(commandValue);
                        }

                        var actionType = action.GetType().GetCustomAttributes(false);

                        if (actionType[0].GetType() == typeof(Core.PreambleActionAttribute))
                        {
                            this.preambleActions.Add(action);
                        }
                        else if (actionType[0].GetType() == typeof(Core.TriggerActionAttribute))
                        {
                            if (null != this.triggerAction)
                            {
                                throw new Core.Exception(System.String.Format("Trigger action already set to '{0}'; cannot also set '{1}'", this.triggerAction.GetType().ToString(), action.GetType().ToString()));
                            }

                            this.triggerAction = action;
                        }

                        foundAction = true;
                        break;
                    }
                }

                if (!foundAction)
                {
                    Core.State.LazyArguments.Add(command);
                }
            }

            if (null == this.triggerAction)
            {
                this.triggerAction = new BuildAction();
            }

            displayInfo(Core.EVerboseLevel.Info);
        }
        
        /// <summary>
        /// Runs the application.
        /// </summary>
        public void Run()
        {
            foreach (Core.IAction action in this.preambleActions)
            {
                if (false == action.Execute())
                {
                    return;
                }
            }

            if (false == this.triggerAction.Execute())
            {
                System.Environment.ExitCode = -3;
            }
        }
    }
}