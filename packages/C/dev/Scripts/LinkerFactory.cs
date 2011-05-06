// <copyright file="LinkerFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class LinkerFactory
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Linker> linkerMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Linker>();

        public static Linker GetTargetInstance(Opus.Core.Target target)
        {
            System.Type type = Opus.Core.State.Get(target.Toolchain, ClassNames.LinkerTool) as System.Type;
            Linker instance = null;
            // TODO: want to remove this
            lock (linkerMap)
            {
                if (linkerMap.ContainsKey(target))
                {
                    instance = linkerMap[target];
                }
                else
                {
                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Linker;
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
                        throw new Opus.Core.Exception(System.String.Format("Unable to create linker instance of type '{0}'", type.ToString()));
                    }
                    linkerMap.Add(target, instance);
                }
            }
            return instance;
        }
    }
}