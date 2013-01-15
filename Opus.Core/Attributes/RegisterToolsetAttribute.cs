// <copyright file="RegisterToolsetAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolsetAttribute : System.Attribute
    {
#if DEBUG
        private string name;
#endif

        public RegisterToolsetAttribute(string name, System.Type toolsetType)
        {
#if DEBUG
            this.name = name;
#endif

            Opus.Core.State.Add("Toolset", name, Opus.Core.ToolsetFactory.GetInstance(toolsetType));
        }

        public class ToolAndOptions
        {
            public ToolAndOptions(System.Type toolType, System.Type optionsType)
            {
                this.ToolType = toolType;
                this.OptionType = optionsType;
            }

            public System.Type ToolType
            {
                get;
                private set;
            }

            public System.Type OptionType
            {
                get;
                private set;
            }
        }

        public static void RegisterAll()
        {
            // need to use inheritence here as the base class is abstract
            var array = State.ScriptAssembly.GetCustomAttributes(typeof(RegisterToolsetAttribute), 
true);
            if (null == array || 0 == array.Length)
            {
                throw new Exception("No toolchains were registered");
            }

#if DEBUG
            foreach (var a in array)
            {
                Opus.Core.Log.DebugMessage("Registered toolset '{0}'", (a as RegisterToolsetAttribute).name);
            }
#endif
        }
    }
}
