﻿// <copyright file="EConfiguration.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public enum EConfiguration
    {
        Invalid,
        Debug,
        Optimized,
        Profile,
        Final
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
                throw new Exception(System.String.Format("Configuration name '{0}' not recognized", configurationName));
            }
            return configuration;
        }
    }
}