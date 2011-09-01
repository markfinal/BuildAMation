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
        private Core.Array<Core.IAction> preambleActions = new Core.Array<Core.IAction>();
        private Core.IAction triggerAction = null;

        private static void displayInfo(Core.EVerboseLevel level, System.Collections.Generic.Dictionary<string,string> argDict)
        {
            Core.Log.Message(level, "Opus location: '{0}'", Core.State.OpusDirectory);
            Core.Log.Message(level, "Opus version : '{0}'", Core.State.OpusVersionString);
            Core.Log.Message(level, "Working dir  : '{0}'", Core.State.WorkingDirectory);
            string arguments = null;
            foreach (string command in argDict.Keys)
            {
                if (null != arguments)
                {
                    arguments += " ";
                }
                arguments += command;
                string value = argDict[command];
                if (null != value)
                {
                    arguments += "=" + value;
                }
            }
            Core.Log.Message(level, "Arguments    : {0}", arguments);
            Core.Log.Message(level, "");
        }

        private void AddCommandValue(System.Collections.Generic.Dictionary<string,string> argDict, string argument)
        {
            string[] splitArg = argument.Split('=');
            string command = splitArg[0];
            command = command.Trim(new char[] { '\n', '\r' });
            string value = null;
            if (splitArg.Length > 1)
            {
                value = splitArg[1];
                value = value.Trim(new char[] { '"', '\'', '\n', '\r' });
            }

            if (argDict.ContainsKey(command))
            {
                Core.Log.DebugMessage("Command '{0}' value '{1}' has been overwritten with '{2}'", command, argDict[command], value);
            }
            
            argDict[command] = value;
        }
        
        /// <summary>
        /// Initializes a new instance of the Application class.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public Application(string[] args)
        {
            Core.TimeProfile profile = new Core.TimeProfile(Core.ETimingProfiles.ProcessCommandLine);
            profile.StartProfile();

            System.Collections.Generic.Dictionary<string,string> argList = new System.Collections.Generic.Dictionary<string,string>();
            string responseFileArgument = null;
            foreach (string arg in args)
            {
                // found a response file
                if (arg.StartsWith("@"))
                {
                    if (null != responseFileArgument)
                    {
                        throw new Core.Exception("Only one response file can be specified", false);
                    }

                    responseFileArgument = arg;

                    string responseFile = responseFileArgument.Substring(1);
                    if (!System.IO.File.Exists(responseFile))
                    {
                        throw new Core.Exception(System.String.Format("Response file '{0}' does not exist", responseFile));
                    }

                    using (System.IO.TextReader responseFileReader = new System.IO.StreamReader(responseFile))
                    {
                        string responseFileArguments = responseFileReader.ReadToEnd();
                        string[] arguments = responseFileArguments.Split(new string[] { " ", "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                        foreach (string argument in arguments)
                        {
                            // handle comments
                            if (argument.StartsWith("#"))
                            {
                                continue;
                            }

                            AddCommandValue(argList, argument);
                        }
                    }
                }
                else
                {
                    // deal with other commands
                    AddCommandValue(argList, arg);
                }
            }

            foreach (string commandName in argList.Keys)
            {
                string commandValue = argList[commandName];
                Core.Log.DebugMessage("Converting command '{0}' with value '{1}' to its action", commandName, commandValue);

                bool foundAction = false;
                foreach (Core.RegisterActionAttribute actionAttribute in Core.ActionManager.Actions)
                {
                    Core.IAction action = actionAttribute.Action;
                    if (action.CommandLineSwitch == commandName)
                    {
                        if (action is Core.IActionWithArguments)
                        {
                            (action as Core.IActionWithArguments).AssignArguments(commandValue);
                        }

                        var actionType = action.GetType().GetCustomAttributes(false);
                        if (0 == actionType.Length)
                        {
                            throw new Core.Exception(System.String.Format("Action '{0}' does not have a type attribute", action.GetType().ToString()));
                        }

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
                    Core.State.LazyArguments[commandName] = commandValue;
                }
            }

            if (null == this.triggerAction)
            {
                this.triggerAction = new BuildAction();
            }

            displayInfo(Core.EVerboseLevel.Info, argList);

            profile.StopProfile();
        }
        
        /// <summary>
        /// Runs the application.
        /// </summary>
        public void Run()
        {
            Core.TimeProfile profile = new Core.TimeProfile(Core.ETimingProfiles.PreambleCommandExecution);
            profile.StartProfile();

            foreach (Core.IAction action in this.preambleActions)
            {
                if (!action.Execute())
                {
                    return;
                }
            }

            profile.StopProfile();

            if (!this.triggerAction.Execute())
            {
                System.Environment.ExitCode = -3;
            }
        }
    }
}