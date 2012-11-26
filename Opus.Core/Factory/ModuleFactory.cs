// <copyright file="ModuleFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ModuleFactory
    {
        public static BaseModule CreateModule(System.Type moduleType)
        {
            TypeUtilities.CheckTypeImplementsInterface(moduleType, typeof(IModule));
            TypeUtilities.CheckTypeIsNotAbstract(moduleType);

            BaseModule module = System.Activator.CreateInstance(moduleType) as BaseModule;

            return module;
        }

        public static BaseModule CreateModule(System.Type moduleType, Target target)
        {
            TypeUtilities.CheckTypeImplementsInterface(moduleType, typeof(IModule));
            TypeUtilities.CheckTypeIsNotAbstract(moduleType);

            BaseModule module = System.Activator.CreateInstance(moduleType, new object[] { target }) as BaseModule;
            return module;
        }
    }
}