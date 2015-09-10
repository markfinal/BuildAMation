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
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs
//     -n=VisualCCommon
//     -c=CxxCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs
//     -pv=PrivateData
//     -e
#endregion // BamOptionGenerator
namespace VisualCCommon
{
    public partial class CxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        private static void
        ExceptionHandlerCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var exceptionHandlerOption = option as Bam.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            switch (exceptionHandlerOption.Value)
            {
                case C.Cxx.EExceptionHandler.Disabled:
                    // nothing
                    break;
                case C.Cxx.EExceptionHandler.Asynchronous:
                    commandLineBuilder.Add("-EHa");
                    break;
                case C.Cxx.EExceptionHandler.Synchronous:
                    commandLineBuilder.Add("-EHsc");
                    break;
                case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                    commandLineBuilder.Add("-EHs");
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized exception handler option");
            }
        }
        private static VisualStudioProcessor.ToolAttributeDictionary
        ExceptionHandlerVisualStudioProcessor(
             object sender,
             Bam.Core.Option option,
             Bam.Core.Target target,
             VisualStudioProcessor.EVisualStudioTarget vsTarget)
        {
            var returnVal = new VisualStudioProcessor.ToolAttributeDictionary();
            var exceptionHandlerOption = option as Bam.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == vsTarget)
            {
                switch (exceptionHandlerOption.Value)
                {
                    case C.Cxx.EExceptionHandler.Disabled:
                    case C.Cxx.EExceptionHandler.Asynchronous:
                    case C.Cxx.EExceptionHandler.Synchronous:
                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        returnVal.Add("ExceptionHandling", System.String.Format("{0}", (int)exceptionHandlerOption.Value));
                        break;
                    default:
                        throw new Bam.Core.Exception("Unrecognized exception handler option");
                }
            }
            else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == vsTarget)
            {
                switch (exceptionHandlerOption.Value)
                {
                    case C.Cxx.EExceptionHandler.Disabled:
                        returnVal.Add("ExceptionHandling", "false");
                        break;
                    case C.Cxx.EExceptionHandler.Asynchronous:
                        returnVal.Add("ExceptionHandling", "Async");
                        break;
                    case C.Cxx.EExceptionHandler.Synchronous:
                        returnVal.Add("ExceptionHandling", "Sync");
                        break;
                    case C.Cxx.EExceptionHandler.SyncWithCExternFunctions:
                        returnVal.Add("ExceptionHandling", "SyncCThrow");
                        break;
                    default:
                        throw new Bam.Core.Exception("Unrecognized exception handler option");
                }
            }
            return returnVal;
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLineProcessor,ExceptionHandlerVisualStudioProcessor);
        }
    }
}
