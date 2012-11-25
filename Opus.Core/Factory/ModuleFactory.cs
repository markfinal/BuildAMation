// <copyright file="ModuleFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ModuleFactory
    {
        // TODO: Make this generic in a typeutilities module
        private static void CheckModuleTypeForInterface(System.Type moduleType)
        {
            if (!typeof(IModule).IsAssignableFrom(moduleType))
            {
                throw new Exception(System.String.Format("Type '{0}' does not implement the interface {1}", moduleType.ToString(), typeof(IModule).ToString()), false);
            }
        }

        public static BaseModule CreateModule(System.Type moduleType)
        {
            CheckModuleTypeForInterface(moduleType);

            BaseModule module = System.Activator.CreateInstance(moduleType) as BaseModule;

            return module;
        }

        public static BaseModule CreateModule(System.Type moduleType, Target target)
        {
            CheckModuleTypeForInterface(moduleType);

            BaseModule module = System.Activator.CreateInstance(moduleType, new object[] { target }) as BaseModule;
            return module;
        }
    }
}