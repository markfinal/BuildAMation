#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
{
    public static class Configuration
    {
        public static EConfiguration
        FromString(
            string configurationName)
        {
            var configuration = EConfiguration.Invalid;
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
