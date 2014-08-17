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
#endregion
namespace Bam.Core
{
    [System.AttributeUsage(System.AttributeTargets.All)]
    public abstract class BaseTargetFilteredAttribute :
        System.Attribute,
        ITargetFilters
    {
        public
        BaseTargetFilteredAttribute()
        {
            this.Platform = EPlatform.All;
            this.Configuration = EConfiguration.All;
            this.ToolsetTypes = null;
        }

        public EPlatform Platform
        {
            get;
            set;
        }

        public EConfiguration Configuration
        {
            get;
            set;
        }

        public System.Type[] ToolsetTypes
        {
            get;
            set;
        }

        public override string
        ToString()
        {
            string message = System.String.Format("Platform='{0}' Configuration='{1}' ToolsetTypes='", this.Platform.ToString(), this.Configuration.ToString());
            if (null == this.ToolsetTypes)
            {
                message += "none";
            }
            else
            {
                var toolsetTypes = new StringArray();
                foreach (var type in this.ToolsetTypes)
                {
                    toolsetTypes.Add(type.ToString());
                }
                message += toolsetTypes.ToString(' ');
            }
            message += "'";
            return message;
        }
    }
}
