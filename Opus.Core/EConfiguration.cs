// <copyright file="EConfiguration.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.Flags]
    public enum EConfiguration
    {
        Invalid   = 0,
        Debug     = (1 << 0),
        Optimized = (1 << 1),
        Profile   = (1 << 2),
        Final     = (1 << 3),
        All       = Debug | Optimized | Profile | Final
    }

    public static class Configuration
    {
        public static EConfiguration FromString(string configurationName)
        {
            EConfiguration configuration = EConfiguration.Invalid;
            if (0 == System.String.Compare(configurationName, "Debug", true))
            {
                configuration = EConfiguration.Debug;
            }
            else if (0 == System.String.Compare(configurationName, "Optimized", true))
            {
                configuration = EConfiguration.Optimized;
            }
            else if (0 == System.String.Compare(configurationName, "Profile", true))
            {
                configuration = EConfiguration.Profile;
            }
            else if (0 == System.String.Compare(configurationName, "Final", true))
            {
                configuration = EConfiguration.Final;
            }
            else
            {
                throw new Exception("Configuration name '{0}' not recognized", configurationName);
            }
            return configuration;
        }
    }
}