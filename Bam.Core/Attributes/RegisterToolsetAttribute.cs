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
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class RegisterToolsetAttribute : System.Attribute
    {
#if DEBUG
        private string name;
#endif

        public RegisterToolsetAttribute(string name, System.Type toolsetType)
        {
#if DEBUG
            this.name = name;
#endif

            State.Add("Toolset", name, ToolsetFactory.GetInstance(toolsetType));
        }

        public static void RegisterAll()
        {
            // need to use inheritence here as the base class is abstract
            var array = State.ScriptAssembly.GetCustomAttributes(typeof(RegisterToolsetAttribute),
true);
            if (null == array || 0 == array.Length)
            {
                throw new Exception("No toolchains were registered");
            }

#if DEBUG
            foreach (var a in array)
            {
                Log.DebugMessage("Registered toolset '{0}'", (a as RegisterToolsetAttribute).name);
            }
#endif
        }
    }
}
