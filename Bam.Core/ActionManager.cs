﻿// <copyright file="ActionManager.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public static class ActionManager
    {
        private static Array<RegisterActionAttribute> actions = null;

        private static Array<RegisterActionAttribute>
        GetActionsFromAssembly(
            System.Reflection.Assembly assembly)
        {
            if (null == assembly)
            {
                return null;
            }

            var customAttributes = assembly.GetCustomAttributes(typeof(RegisterActionAttribute), false) as RegisterActionAttribute[];
            return new Array<RegisterActionAttribute>(customAttributes);
        }

        static
        ActionManager()
        {
            actions = GetActionsFromAssembly(System.Reflection.Assembly.GetEntryAssembly());
        }

        public static Array<RegisterActionAttribute> Actions
        {
            get
            {
                return actions;
            }
        }

        public static Array<RegisterActionAttribute> ScriptActions
        {
            get
            {
                return GetActionsFromAssembly(State.ScriptAssembly);
            }
        }

        public static Array<RegisterActionAttribute> PreambleActions
        {
            get
            {
                var filteredAction = new Array<RegisterActionAttribute>();

                foreach (var action in actions)
                {
                    var actionType = action.Action.GetType().GetCustomAttributes(false);
                    if (0 == actionType.Length)
                    {
                        throw new Core.Exception("Action '{0}' does not have a type attribute", action.GetType().ToString());
                    }

                    if (actionType[0].GetType() == typeof(Core.PreambleActionAttribute))
                    {
                        filteredAction.Add(action);
                    }
                }

                filteredAction.Sort();

                return filteredAction;
            }
        }

        public static Array<RegisterActionAttribute> TriggerActions
        {
            get
            {
                var filteredAction = new Array<RegisterActionAttribute>();

                foreach (var action in actions)
                {
                    var actionType = action.Action.GetType().GetCustomAttributes(false);
                    if (0 == actionType.Length)
                    {
                        throw new Core.Exception("Action '{0}' does not have a type attribute", action.GetType().ToString());
                    }

                    if (actionType[0].GetType() == typeof(Core.TriggerActionAttribute))
                    {
                        filteredAction.Add(action);
                    }
                }

                filteredAction.Sort();

                return filteredAction;
            }
        }

        public static Array<RegisterActionAttribute> ImmediateActions
        {
            get
            {
                var filteredAction = new Array<RegisterActionAttribute>();

                foreach (var action in actions)
                {
                    var actionType = action.Action.GetType().GetCustomAttributes(false);
                    if (0 == actionType.Length)
                    {
                        throw new Core.Exception("Action '{0}' does not have a type attribute", action.GetType().ToString());
                    }

                    if (actionType[0].GetType() == typeof(Core.ImmediateActionAttribute))
                    {
                        filteredAction.Add(action);
                    }
                }

                filteredAction.Sort();

                return filteredAction;
            }
        }

        public static Array<IAction>
        FindInvokedActionsByType(
            System.Type actionType)
        {
            var array = new Array<IAction>();
            foreach (var action in State.InvokedActions)
            {
                if (action.GetType() == actionType)
                {
                    array.Add(action);
                }
            }
            return array;
        }
    }
}
