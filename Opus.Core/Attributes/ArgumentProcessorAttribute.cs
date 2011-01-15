// <copyright file="ArgumentProcessorAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class ArgumentProcessorAttribute : System.Attribute
    {
        public System.Type Type
        {
            get;
            private set;
        }
        
        public ArgumentProcessorAttribute(System.Type type)
        {
            this.Type = type;
            // TODO: do I really need a factory for this?
            this.Instance = System.Activator.CreateInstance(type);
        }

        private object Instance
        {
            get;
            set;
        }
        
        public bool Process(string argument)
        {
            // TODO: could be a little more rigorous on finding the method, e.g. return values, types, etc.
            System.Reflection.MethodInfo processMethod = this.Type.GetMethod("Process");
            bool processed = (bool)processMethod.Invoke(this.Instance, new object[] { argument });
            return processed;
        }
    }
}