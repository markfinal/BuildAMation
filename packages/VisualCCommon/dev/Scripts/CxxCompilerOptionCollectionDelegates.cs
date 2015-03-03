#region License
// Copyright 2010-2015 Mark Final
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
