// <copyright file="Win32ResourceCompilerFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class Win32ResourceCompilerFactory
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Win32ResourceCompilerBase> resourceCompilerMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Win32ResourceCompilerBase>();

        public static Win32ResourceCompilerBase GetTargetInstance(Opus.Core.Target target)
        {
            System.Type type = Opus.Core.State.Get(target.Toolchain, ClassNames.Win32ResourceCompilerTool) as System.Type;
            Win32ResourceCompilerBase instance = null;
            // TODO: want to remove this
            lock (resourceCompilerMap)
            {
                if (resourceCompilerMap.ContainsKey(target))
                {
                    instance = resourceCompilerMap[target];
                }
                else
                {
                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Win32ResourceCompilerBase;
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
                        throw new Opus.Core.Exception(System.String.Format("Unable to create Win32 resource compiler instance of type '{0}'", type.ToString()));
                    }
                    resourceCompilerMap.Add(target, instance);
                }
            }
            return instance;
        }
    }
}