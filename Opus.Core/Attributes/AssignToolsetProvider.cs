// <copyright file="AssignToolsetProviderAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class AssignToolsetProviderAttribute : System.Attribute
    {
        private delegate string ProviderDelegate(System.Type toolType);

        private string toolsetName;
        private ProviderDelegate providerFn;

        public AssignToolsetProviderAttribute(string toolsetName)
        {
            this.toolsetName = toolsetName;
        }

        public AssignToolsetProviderAttribute(System.Type providerClass, string methodName)
        {
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Static |
                                                   System.Reflection.BindingFlags.Public |
                                                   System.Reflection.BindingFlags.NonPublic;
            System.Reflection.MethodInfo method = providerClass.GetMethod(methodName, flags);
            if (null == method)
            {
                throw new Exception("Unable to locate a static method called '{0}' in class '{1}'", methodName, providerClass.ToString());
            }
            System.Delegate dlg = System.Delegate.CreateDelegate(typeof(ProviderDelegate), method, false);
            if (null == dlg)
            {
                throw new Exception("Unable to match method '{0}' in class '{1}' to the delegate 'string fn(System.Type)'", method, providerClass.ToString());
            }
            this.providerFn = dlg as ProviderDelegate;
        }

        public string ToolsetName(System.Type toolType)
        {
            if (null == this.providerFn)
            {
                return this.toolsetName;
            }
            else
            {
                string toolsetName = this.providerFn.Method.Invoke(null, new object[] { toolType }) as string;
                return toolsetName;
            }
        }
    }
}
