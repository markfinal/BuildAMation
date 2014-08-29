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
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class AssignToolsetProviderAttribute :
        System.Attribute
    {
        private delegate string ProviderDelegate(System.Type toolType);

        private string toolsetName;
        private ProviderDelegate providerFn;

        public
        AssignToolsetProviderAttribute(
            string toolsetName)
        {
            this.toolsetName = toolsetName;
        }

        public
        AssignToolsetProviderAttribute(
            System.Type providerClass,
            string methodName)
        {
            var flags = System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic;
            var method = providerClass.GetMethod(methodName, flags);
            if (null == method)
            {
                throw new Exception("Unable to locate a static method called '{0}' in class '{1}'", methodName, providerClass.ToString());
            }
            var dlg = System.Delegate.CreateDelegate(typeof(ProviderDelegate), method, false);
            if (null == dlg)
            {
                throw new Exception("Unable to match method '{0}' in class '{1}' to the delegate 'string fn(System.Type)'", method, providerClass.ToString());
            }
            this.providerFn = dlg as ProviderDelegate;
        }

        public string
        ToolsetName(
            System.Type toolType)
        {
            if (null == this.providerFn)
            {
                return this.toolsetName;
            }
            else
            {
                var toolsetName = this.providerFn.Method.Invoke(null, new object[] { toolType }) as string;
                return toolsetName;
            }
        }
    }
}
