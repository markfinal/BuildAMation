// <copyright file="RegisterToolsetAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public class RegisterToolsetAttribute : System.Attribute
    {
        public RegisterToolsetAttribute(string name, System.Type toolsetType)
        {
            if (!Opus.Core.State.HasCategory("Toolset"))
            {
                Opus.Core.State.AddCategory("Toolset");
            }
            Opus.Core.State.Add("Toolset", name, Opus.Core.ToolsetFactory.CreateToolset(toolsetType));
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
                throw new Exception("No toolchains were registered", false);
            }
        }
    }
}
