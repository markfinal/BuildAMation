// <copyright file="ToolchainFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class ToolchainFactory
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Toolchain> toolchainMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Toolchain>();

        public static Toolchain GetTargetInstance(Opus.Core.Target target)
        {
            System.Type type = Opus.Core.State.Get(target.Toolchain, ClassNames.Toolchain) as System.Type;
            Toolchain instance;
            // TODO: want to remove this
            lock (toolchainMap)
            {
                if (toolchainMap.ContainsKey(target))
                {
                    instance = toolchainMap[target];
                }
                else
                {
                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Toolchain;
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
                        throw new Opus.Core.Exception(System.String.Format("Unable to create toolchain instance of type '{0}'", type.ToString()));
                    }
                    toolchainMap.Add(target, instance);
                }
            }
            return instance;
        }
    }
}