// <copyright file="ModuleFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ModuleFactory
    {
        public static BaseModule CreateModule(System.Type moduleType, Target target)
        {
            TypeUtilities.CheckTypeImplementsInterface(moduleType, typeof(IModule));
            TypeUtilities.CheckTypeIsNotAbstract(moduleType);

            BaseModule module = null;
            try
            {
                if (null != moduleType.GetConstructor(new System.Type[] { typeof(Target) }))
                {
                    module = System.Activator.CreateInstance(moduleType, new object[] { target }) as BaseModule;
                }
                else
                {
                    module = System.Activator.CreateInstance(moduleType) as BaseModule;
                }
            }
            catch (System.MissingMethodException)
            {
                throw new Exception("Cannot construct object of type '{0}'. Missing public constructor?", moduleType.ToString());
            }

            return module;
        }
    }
}