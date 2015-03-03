#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
