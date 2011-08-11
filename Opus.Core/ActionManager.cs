// <copyright file="ActionManager.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ActionManager
    {
        private static Array<RegisterActionAttribute> actions = null;

        static ActionManager()
        {
            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetEntryAssembly();
            RegisterActionAttribute[] customAttributes = executingAssembly.GetCustomAttributes(typeof(Opus.Core.RegisterActionAttribute), false) as RegisterActionAttribute[];
            actions = new Array<RegisterActionAttribute>(customAttributes);
        }

        public static Array<RegisterActionAttribute> Actions
        {
            get
            {
                return actions;
            }
        }

        public static Array<RegisterActionAttribute> PreambleActions
        {
            get
            {
                Array<RegisterActionAttribute> filteredAction = new Array<RegisterActionAttribute>();

                foreach (RegisterActionAttribute action in actions)
                {
                    var actionType = action.Action.GetType().GetCustomAttributes(false);
                    if (0 == actionType.Length)
                    {
                        throw new Core.Exception(System.String.Format("Action '{0}' does not have a type attribute", action.GetType().ToString()));
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
                Array<RegisterActionAttribute> filteredAction = new Array<RegisterActionAttribute>();

                foreach (RegisterActionAttribute action in actions)
                {
                    var actionType = action.Action.GetType().GetCustomAttributes(false);
                    if (0 == actionType.Length)
                    {
                        throw new Core.Exception(System.String.Format("Action '{0}' does not have a type attribute", action.GetType().ToString()));
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
                Array<RegisterActionAttribute> filteredAction = new Array<RegisterActionAttribute>();

                foreach (RegisterActionAttribute action in actions)
                {
                    var actionType = action.Action.GetType().GetCustomAttributes(false);
                    if (0 == actionType.Length)
                    {
                        throw new Core.Exception(System.String.Format("Action '{0}' does not have a type attribute", action.GetType().ToString()));
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

        public static IAction FindByType(System.Type actionType)
        {
            foreach (RegisterActionAttribute action in actions)
            {
                if (action.Action.GetType() == actionType)
                {
                    return action.Action;
                }
            }

            return null;
        }
    }
}