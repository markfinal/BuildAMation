// <copyright file="Application.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>
namespace Bam
{
    /// <summary>
    /// Opus Application class.
    /// </summary>
    public sealed class Application
    {
        private Core.Array<Core.IAction> preambleActions = new Core.Array<Core.IAction>();
        private Core.IAction triggerAction = null;

        private static void
        displayInfo(
            Core.EVerboseLevel level,
            System.Collections.Generic.Dictionary<string,string> argDict)
        {
            Core.Log.Message(level, "Opus location: '{0}'", Core.State.OpusDirectory);
            Core.Log.Message(level, "Opus version : '{0}'", Core.State.OpusVersionString);
            Core.Log.Message(level, "Working dir  : '{0}'", Core.State.WorkingDirectory);
            var arguments = new System.Text.StringBuilder();
            foreach (var command in argDict.Keys)
            {
                var value = argDict[command];
                if (null != value)
                {
                    arguments.AppendFormat("{0}={1} ", command, value);
                }
                else
                {
                    arguments.AppendFormat("{0} ", command);
                }
            }
            Core.Log.Message(level, "Host Platform: {0} {1}", Core.State.Platform, Core.State.IsLittleEndian ? "(little endian)" : "(big endian)");
            Core.Log.Message(level, "Arguments    : {0}", arguments.ToString().TrimEnd());
            Core.Log.Message(level, "");
        }

        private void
        AddCommandValue(
            System.Collections.Generic.Dictionary<string,string> argDict,
            string argument)
        {
            var splitArg = argument.Split('=');
            var command = splitArg[0];
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
        /// Processes the command line arguments
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Dictionary of key-value pairs for the arguments</returns>
        private System.Collections.Generic.Dictionary<string, string>
        ProcessCommandLine(
            string[] args)
        {
            var argList = new System.Collections.Generic.Dictionary<string, string>();
            string responseFileArgument = null;
            foreach (var arg in args)
            {
                // found a response file
                if (arg.StartsWith("@"))
                {
                    if (null != responseFileArgument)
                    {
                        throw new Core.Exception("Only one response file can be specified");
                    }

                    responseFileArgument = arg;

                    var responseFile = responseFileArgument.Substring(1);
                    if (!System.IO.File.Exists(responseFile))
                    {
                        throw new Core.Exception("Response file '{0}' does not exist", responseFile);
                    }

                    using (var responseFileReader = new System.IO.StreamReader(responseFile))
                    {
                        var responseFileArguments = responseFileReader.ReadToEnd();
                        var arguments = responseFileArguments.Split(new string[] { " ", "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                        foreach (var argument in arguments)
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

            foreach (var commandName in argList.Keys)
            {
                var commandValue = argList[commandName];
                Core.Log.DebugMessage("Converting command '{0}' with value '{1}' to its action", commandName, commandValue);

                var foundAction = false;
                foreach (var actionAttribute in Core.ActionManager.Actions)
                {
                    var action = actionAttribute.Action;
                    var isThisAction = false;
                    if (action is Core.IActionCommandComparison)
                    {
                        isThisAction = (action as Core.IActionCommandComparison).Compare(action.CommandLineSwitch, commandName);
                    }
                    else
                    {
                        isThisAction = (action.CommandLineSwitch == commandName);
                    }

                    if (isThisAction)
                    {
                        var clone = action.Clone() as Core.IAction;

                        if (clone is Core.IActionWithArguments)
                        {
                            (clone as Core.IActionWithArguments).AssignArguments(commandValue);
                        }

                        var actionType = clone.GetType().GetCustomAttributes(false);
                        if (0 == actionType.Length)
                        {
                            throw new Core.Exception("Action '{0}' does not have a type attribute", clone.GetType().ToString());
                        }

                        if (actionType[0].GetType() == typeof(Core.PreambleActionAttribute))
                        {
                            this.preambleActions.Add(clone);
                        }
                        else if (actionType[0].GetType() == typeof(Core.TriggerActionAttribute))
                        {
                            if (null != this.triggerAction)
                            {
                                throw new Core.Exception("Trigger action already set to '{0}'; cannot also set '{1}'", this.triggerAction.GetType().ToString(), clone.GetType().ToString());
                            }

                            this.triggerAction = clone;
                        }

                        foundAction = true;
                        Core.State.InvokedActions.Add(clone);
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

            return argList;
        }

        /// <summary>
        /// Initializes a new instance of the Application class.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public
        Application(
            string[] args)
        {
            var profile = new Core.TimeProfile(Core.ETimingProfiles.ProcessCommandLine);
            profile.StartProfile();

            var argList = ProcessCommandLine(args);
            displayInfo(Core.EVerboseLevel.Info, argList);

            profile.StopProfile();
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        public void
        Run()
        {
            // get notified of the progress of any scheduling updates
            Core.State.SchedulerProgressUpdates.Add(new Core.BuildSchedulerProgressUpdatedDelegate(scheduler_ProgressUpdated));

            var profile = new Core.TimeProfile(Core.ETimingProfiles.PreambleCommandExecution);
            profile.StartProfile();

            foreach (var action in this.preambleActions)
            {
                if (!action.Execute())
                {
                    return;
                }
            }

            profile.StopProfile();

            if (!this.triggerAction.Execute())
            {
                System.Environment.ExitCode = -4;
            }
        }

        private void
        scheduler_ProgressUpdated(
            int percentageComplete)
        {
            Core.Log.Info("\t{0}% Scheduled", percentageComplete);
        }
    }
}