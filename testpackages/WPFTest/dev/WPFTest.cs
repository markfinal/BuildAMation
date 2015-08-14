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
namespace WPFTest
{
    // Define module classes here
    class WPFExecutable :
        CSharp.WindowsExecutable
    {
        public
        WPFExecutable()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            this.applicationDefinition = Bam.Core.FileLocation.Get(sourceDir, "App.xaml");
            this.page = Bam.Core.FileLocation.Get(sourceDir, "MainWindow.xaml");

            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(WPFExecutable_UpdateOptions);
        }

        void
        WPFExecutable_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as CSharp.IOptions;
            options.References.Add("System.dll");
            options.References.Add("System.Data.dll");
            options.References.Add("System.Xaml.dll");
            options.References.Add("WindowsBase.dll");
            options.References.Add("PresentationCore.dll");
            options.References.Add("PresentationFramework.dll");
        }

        [CSharp.ApplicationDefinition]
        Bam.Core.Location applicationDefinition;

        [CSharp.Pages]
        Bam.Core.Location page;
    }
}
