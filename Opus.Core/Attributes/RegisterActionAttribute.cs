// <copyright file="RegisterActionAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public class RegisterActionAttribute : System.Attribute, System.IComparable
    {
        public RegisterActionAttribute(System.Type classType)
        {
            if (!typeof(IAction).IsAssignableFrom(classType))
            {
                throw new Exception("Class '{0}' does not implement the IAction interface", classType.ToString());
            }

            this.Action = System.Activator.CreateInstance(classType) as IAction;
        }

        public IAction Action
        {
            get;
            private set;
        }

        int System.IComparable.CompareTo(object obj)
        {
            var thisAs = this as RegisterActionAttribute;
            var objAs = obj as RegisterActionAttribute;

            int compare = thisAs.Action.CommandLineSwitch.CompareTo(objAs.Action.CommandLineSwitch);
            return compare;
        }

        public override string ToString()
        {
            return this.Action.CommandLineSwitch + " (" + this.Action.Description + ")";
        }
    }
}
