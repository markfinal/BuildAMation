// <copyright file="ModuleFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ModuleFactory
    {
        private static void CheckModuleTypeForInterface(System.Type moduleType)
        {
            if (!typeof(IModule).IsAssignableFrom(moduleType))
            {
                throw new Exception(System.String.Format("Type '{0}' does not implement the interface {1}", moduleType.ToString(), typeof(IModule).ToString()), false);
            }
        }

        public static IModule CreateModule(System.Type moduleType)
        {
            CheckModuleTypeForInterface(moduleType);

            IModule module = System.Activator.CreateInstance(moduleType) as IModule;

            return module;
        }

        public static IModule CreateModule(System.Type moduleType, Target target)
        {
            CheckModuleTypeForInterface(moduleType);

            IModule module = System.Activator.CreateInstance(moduleType, new object[] { target }) as IModule;
            return module;
        }
    }
}