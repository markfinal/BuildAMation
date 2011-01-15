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
                        if (null != exception.InnerException)
                        {
                            if (exception.InnerException is Opus.Core.Exception)
                            {
                                throw exception.InnerException;
                            }
                        }

                        throw;
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
    }
}