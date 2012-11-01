// <copyright file="CompilerFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class CompilerFactory
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Compiler> CCompilerMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Compiler>();
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Compiler> CPlusPlusCompilerMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Compiler>();

        private static Compiler AgnosticGetTargetInstance(Opus.Core.Target target, string className, System.Collections.Generic.Dictionary<Opus.Core.Target, Compiler> map)
        {
            System.Type type = Opus.Core.State.Get(target.Toolchain, className) as System.Type;
            Compiler instance = null;
            // TODO: want to remove this
            lock (map)
            {
                if (map.ContainsKey(target))
                {
                    instance = map[target];
                }
                else
                {
                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Compiler;
                    }
                    catch (System.Exception exception)
                    {
                        System.Exception innerException = exception;
                        while (null != innerException.InnerException)
                        {
                            innerException = innerException.InnerException;
                        }

                        throw innerException;
                    }
                    if (null == instance)
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unable to create compiler instance of type '{0}'", type.ToString()));
                    }
                    map.Add(target, instance);
                }
            }
            return instance;
        }

        private static Compiler AgnosticGetInstance(Opus.Core.Target target, bool isCPlusPlus)
        {
            // TODO!
            System.Collections.Generic.Dictionary<Opus.Core.Target, Compiler> map = isCPlusPlus ? CPlusPlusCompilerMap : CCompilerMap;

            // TODO: why is this hard coded to typeof C.Compiler?
            var toolchainsMap = Opus.Core.State.Get("Toolchains", "Map");
            if (!(toolchainsMap as System.Collections.Generic.Dictionary<System.Type, string>).ContainsKey(typeof(C.Compiler)))
            {
                throw new Opus.Core.Exception("Unable to locate toolchain for type C.Compiler", false);
            }
            string toolchainNameForThisTool = (toolchainsMap as System.Collections.Generic.Dictionary<System.Type, string>)[typeof(C.Compiler)];
            var toolchainTypeMap = Opus.Core.State.Get("ToolchainTypeMap", toolchainNameForThisTool);
            if (!(toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, Opus.Core.RegisterToolsetAttribute.ToolAndOptions>).ContainsKey(typeof(C.Compiler)))
            {
                throw new Opus.Core.Exception("Unable to locate targetted tool type for type C.Compiler", false);
            }
            Opus.Core.RegisterToolsetAttribute.ToolAndOptions toolAndOptions = (toolchainTypeMap as System.Collections.Generic.Dictionary<System.Type, Opus.Core.RegisterToolsetAttribute.ToolAndOptions>)[typeof(C.Compiler)];
            System.Type type = toolAndOptions.ToolType;

            Compiler instance = null;
            // TODO: want to remove this
            lock (map)
            {
                if (map.ContainsKey(target))
                {
                    instance = map[target];
                }
                else
                {
                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Compiler;
                    }
                    catch (System.Exception exception)
                    {
                        System.Exception innerException = exception;
                        while (null != innerException.InnerException)
                        {
                            innerException = innerException.InnerException;
                        }

                        throw innerException;
                    }
                    if (null == instance)
                    {
                        throw new Opus.Core.Exception(System.String.Format("Unable to create compiler instance of type '{0}'", type.ToString()));
                    }
                    map.Add(target, instance);
                }
            }
            return instance;
        }

        public static Compiler GetTargetInstance(Opus.Core.Target target, string className)
        {
            Compiler compilerInstance = null;
            if (ClassNames.CCompilerTool == className)
            {
                compilerInstance = AgnosticGetTargetInstance(target, className, CCompilerMap);
            }
            else if (ClassNames.CPlusPlusCompilerTool == className)
            {
                compilerInstance = AgnosticGetTargetInstance(target, className, CPlusPlusCompilerMap);
            }
            return compilerInstance;
        }

        public static Compiler GetInstance(Opus.Core.Target target, bool isCPlusPlus)
        {
            // NEW STYLE
            Compiler compilerInstance = AgnosticGetInstance(target, isCPlusPlus);

#if false
            if (!isCPlusPlus)
            {
                compilerInstance = AgnosticGetTargetInstance(target, className, CCompilerMap);
            }
            else if (ClassNames.CPlusPlusCompilerTool == className)
            {
                compilerInstance = AgnosticGetTargetInstance(target, className, CPlusPlusCompilerMap);
            }
#endif
            return compilerInstance;
        }
    }
}