// <copyright file="CxxCompilerFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class CxxCompilerFactory
    {
        private static System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Opus.Core.Target, Opus.Core.ITool>> map = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Opus.Core.Target, Opus.Core.ITool>>();

        public static Opus.Core.ITool GetInstance(Opus.Core.Target target)
        {
            var toolchainsMap = Opus.Core.State.Get("Toolchains", "Map");
            if (!(toolchainsMap as System.Collections.Generic.Dictionary<System.Type, string>).ContainsKey(typeof(C.CxxCompiler)))
            {
                throw new Opus.Core.Exception("Unable to locate toolchain for type C.CxxCompiler", false);
            }
            string toolchainNameForThisTool = (toolchainsMap as System.Collections.Generic.Dictionary<System.Type, string>)[typeof(C.CxxCompiler)];
            var toolchainTypeMap = Opus.Core.State.Get("ToolchainTypeMap", toolchainNameForThisTool);
            if (!(toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, Opus.Core.RegisterToolchainAttribute.ToolAndOptions>).ContainsKey(typeof(C.CxxCompiler)))
            {
                throw new Opus.Core.Exception("Unable to locate targetted tool type for type C.CxxCompiler", false);
            }
            Opus.Core.RegisterToolchainAttribute.ToolAndOptions toolAndOptions = (toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, Opus.Core.RegisterToolchainAttribute.ToolAndOptions>)[typeof(C.CxxCompiler)];
            System.Type type = toolAndOptions.ToolType;

            Opus.Core.ITool instance = null;
            lock (map)
            {
                System.Collections.Generic.Dictionary<Opus.Core.Target, Opus.Core.ITool> innerMap = null;
                if (map.ContainsKey(type))
                {
                    innerMap = map[type];
                }
                else
                {
                    innerMap = map[type] = new System.Collections.Generic.Dictionary<Opus.Core.Target, Opus.Core.ITool>();
                }

                if (innerMap.ContainsKey(target))
                {
                    instance = map[type][target];
                }
                else
                {
                    instance = map[type][target] = System.Activator.CreateInstance(type, new object[] { target }) as Opus.Core.ITool;
                }
            }

            return instance;
        }
    }
}