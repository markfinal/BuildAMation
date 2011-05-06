// <copyright file="CscFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    // TODO: I think this is too complicated for its purpose
    public static class CscFactory
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Csc> cscMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Csc>();

        public static Csc GetTargetInstance(Opus.Core.Target target)
        {
            System.Type type = Opus.Core.State.Get(target.Toolchain, "ClassCsc") as System.Type;
            Csc instance = null;
            // TODO: want to remove this
            lock (cscMap)
            {
                if (cscMap.ContainsKey(target))
                {
                    instance = cscMap[target];
                }
                else
                {
                    if (null == type.GetConstructor(new System.Type[] { target.GetType() }))
                    {
                        throw new Opus.Core.Exception(System.String.Format("Could not find constructor '{0}(Opus.Core.Target)'", type.ToString()));
                    }

                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Csc;
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
                        throw new Opus.Core.Exception(System.String.Format("Unable to create Csc instance of type '{0}'", type.ToString()));
                    }
                    cscMap.Add(target, instance);
                }
            }
            return instance;
        }
    }
}