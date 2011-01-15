// <copyright file="MapToolChainClassTypesAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class MapToolChainClassTypesAttribute : System.Attribute
    {
        public MapToolChainClassTypesAttribute(string toolChainName, string implementationName, string toolName, System.Type toolType, System.Type toolOptionsType)
        {
            if (!State.HasCategory("Toolchains"))
            {
                Log.DebugMessage("No toolchains were necessary");
                return;
            }

            if (!State.Has("Toolchains", toolChainName))
            {
                Log.DebugMessage("Toolchain '{0}' is not required to be mapped", toolChainName);
                return;
            }

            if (!Core.State.HasCategory(implementationName))
            {
                throw new Exception(System.String.Format("Target ToolChain '{0}' has not been registered. Use the RegisterTargetToolChain assembly attribute", implementationName), false);
            }

            if (!Core.State.Has(implementationName, toolName))
            {
                Core.State.Add<System.Type>(implementationName, toolName, toolType);
            }
            else
            {
                throw new Exception(System.String.Format("Target ToolChain '{0}' tool '{1}' is already registered", implementationName, toolName));
            }

            string optionCollectionName = System.String.Format("{0}Options", toolName);
            if (!Core.State.Has(implementationName, optionCollectionName))
            {
                Core.State.Add<System.Type>(implementationName, optionCollectionName, toolOptionsType);
            }
            else
            {
                throw new Exception(System.String.Format("Target ToolChain '{0}' tool '{1}' option class '{2}' is already registered", implementationName, toolName, toolOptionsType));
            }
        }
    }
}