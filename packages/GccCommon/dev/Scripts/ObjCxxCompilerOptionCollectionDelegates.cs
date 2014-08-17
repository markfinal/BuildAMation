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

namespace GccCommon
{
    public partial class ObjCxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option delegates
        public static void
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
                commandLineBuilder.Add("-fno-exceptions");
                break;
            case C.Cxx.EExceptionHandler.Asynchronous:
            case C.Cxx.EExceptionHandler.Synchronous:
                commandLineBuilder.Add("-fexceptions");
                break;
            default:
                throw new Bam.Core.Exception("Unrecognized exception handler option");
            }
        }
        public static void
        ExceptionHandlerXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var exceptionHandler = option as Bam.Core.ValueTypeOption<C.Cxx.EExceptionHandler>;
            var exceptionsOption = configuration.Options["GCC_ENABLE_CPP_EXCEPTIONS"];
            switch (exceptionHandler.Value)
            {
                case C.Cxx.EExceptionHandler.Disabled:
                    exceptionsOption.AddUnique("NO");
                    break;
                case C.Cxx.EExceptionHandler.Asynchronous:
                case C.Cxx.EExceptionHandler.Synchronous:
                    exceptionsOption.AddUnique("YES");
                    break;
                default:
                    throw new Bam.Core.Exception("Unrecognized exception handler option");
            }
            if (exceptionsOption.Count != 1)
            {
                throw new Bam.Core.Exception("More than one exceptions option has been set");
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["ExceptionHandler"].PrivateData = new PrivateData(ExceptionHandlerCommandLineProcessor,ExceptionHandlerXcodeProjectProcessor);
        }
    }
}
