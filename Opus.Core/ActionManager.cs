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