// <copyright file="ArchiverFactory.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public static class ArchiverFactory
    {
        private static System.Collections.Generic.Dictionary<Opus.Core.Target, Archiver> archiverMap = new System.Collections.Generic.Dictionary<Opus.Core.Target, Archiver>();

        public static Archiver GetTargetInstance(Opus.Core.Target target)
        {
            System.Type type = Opus.Core.State.Get(target.Toolchain, ClassNames.ArchiverTool) as System.Type;
            Archiver instance = null;
            // TODO: want to remove this
            lock (archiverMap)
            {
                if (archiverMap.ContainsKey(target))
                {
                    instance = archiverMap[target];
                }
                else
                {
                    try
                    {
                        instance = System.Activator.CreateInstance(type, new object[] { target }) as Archiver;
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
                        throw new Opus.Core.Exception(System.String.Format("Unable to create archiver instance of type '{0}'", type.ToString()));
                    }
                    archiverMap.Add(target, instance);
                }
            }
            return instance;
        }
    }
}