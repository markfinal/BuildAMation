#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    /// <summary>
    /// Utility functions instantiating build mode policy classes.
    /// Note that the where clause specifies class, whereas T can only ever be an interface.
    /// </summary>
    public static class ExecutionPolicyUtilities<T> where T: class
    {
        static System.Collections.Generic.Dictionary<string, T> Policies = new System.Collections.Generic.Dictionary<string, T>();

        private static T
        InternalCreate(
            string classname)
        {
            var type = System.Type.GetType(classname,
                (typename) =>
                {
                    // TODO: this does not seem to be used
                    return Graph.Instance.ScriptAssembly;
                },
                (assembly, name, checkcase) =>
                {
                    return Graph.Instance.ScriptAssembly.GetType(name);
                });
            if (null == type)
            {
                throw new Exception("Unable to locate class '{0}'", classname);
            }
            var policy = System.Activator.CreateInstance(type) as T;
            if (null == policy)
            {
                throw new Exception("Unable to create instance of class '{0}'", classname);
            }
            return policy;
        }

        /// <summary>
        /// Create an instance of the policy class, classname, that implements policy in interface T.
        /// </summary>
        /// <param name="classname">Name of the class implementing policy T</param>
        public static T
        Create(
            string classname)
        {
            if (!Policies.ContainsKey(classname))
            {
                Policies.Add(classname, InternalCreate(classname));
            }
            return Policies[classname];
        }
    }
}
