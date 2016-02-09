#region License
// Copyright (c) 2010-2016, Mark Final
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
using System.Linq;
namespace C
{
    /// <summary>
    /// Base class for containers of homogenous object files. Provides methods that automatically
    /// generate modules of the correct type given the source paths.
    /// </summary>
    public abstract class CCompilableModuleContainer<ChildModuleType> :
        CModuleContainer<ChildModuleType>
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    {
        /// <summary>
        /// Add an object file from an existing SourceFile module.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="sourceModule">Source module.</param>
        /// <param name="preInitDlg">Pre init dlg.</param>
        virtual public ChildModuleType
        AddFile(
            SourceFile sourceModule,
            Bam.Core.Module.PreInitDelegate<ChildModuleType> preInitDlg = null)
        {
            var child = Bam.Core.Module.Create<ChildModuleType>(this, preInitCallback: preInitDlg);
            var requiresSource = child as IRequiresSourceModule;
            if (null == requiresSource)
            {
                throw new Bam.Core.Exception("Module type {0} does not implement the interface {1}", typeof(ChildModuleType).FullName, typeof(IRequiresSourceModule).FullName);
            }
            requiresSource.Source = sourceModule;
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }
    }
}
