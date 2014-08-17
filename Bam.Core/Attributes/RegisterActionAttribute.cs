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
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public class RegisterActionAttribute :
        System.Attribute,
        System.IComparable
    {
        public
        RegisterActionAttribute(
            System.Type classType)
        {
            if (!typeof(IAction).IsAssignableFrom(classType))
            {
                throw new Exception("Class '{0}' does not implement the IAction interface", classType.ToString());
            }

            this.Action = System.Activator.CreateInstance(classType) as IAction;
        }

        public IAction Action
        {
            get;
            private set;
        }

        int
        System.IComparable.CompareTo(
            object obj)
        {
            var thisAs = this as RegisterActionAttribute;
            var objAs = obj as RegisterActionAttribute;

            int compare = thisAs.Action.CommandLineSwitch.CompareTo(objAs.Action.CommandLineSwitch);
            return compare;
        }

        public override string
        ToString()
        {
            return this.Action.CommandLineSwitch + " (" + this.Action.Description + ")";
        }
    }
}
