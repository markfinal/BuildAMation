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
#define __NO_STD_VECTOR // Use cl::vector instead of STL version
#define CL_USE_DEPRECATED_OPENCL_1_1_APIS // TODO: remove the need for this
#ifdef _MSC_VER
#pragma warning(push, 3)
#endif
#include <CL/cl.hpp>
#ifdef _MSC_VER
#pragma warning(pop)
#endif

#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <iostream>
#include <string>
#include <iterator>

const std::string hw("Hello World\n");

inline void
checkErr(cl_int err, const char * name)
{
    if (err != CL_SUCCESS) {
        std::cerr << "ERROR: " << name
                 << " (" << err << ")" << std::endl;
        exit(EXIT_FAILURE);
    }
}

int main()
{
    // this is based upon http://www.codeproject.com/KB/GPU-Programming/IntroToOpenCL.aspx

    cl_int err;
    cl::vector< cl::Platform > platformList;
    cl::Platform::get(&platformList);
    checkErr(platformList.size()!=0 ? CL_SUCCESS : -1, "cl::Platform::get");
    std::cerr << "Platform count is: " << platformList.size() << std::endl;

    std::string platformVendor;
    platformList[0].getInfo((cl_platform_info)CL_PLATFORM_VENDOR, &platformVendor);
    std::cerr << "Platform is by: " << platformVendor << "\n";
    cl_context_properties cprops[3] =
        {CL_CONTEXT_PLATFORM, (cl_context_properties)(platformList[0])(), 0};

    cl::Context context(
       CL_DEVICE_TYPE_CPU,
       cprops,
       NULL,
       NULL,
       &err);
    if (err != CL_SUCCESS)
    {
        context = cl::Context(CL_DEVICE_TYPE_GPU,
                              cprops,
                              NULL,
                              NULL,
                              &err);
    }
    checkErr(err, "Context::Context()");

    char * outH = new char[hw.length()+1];
    cl::Buffer outCL(
        context,
        CL_MEM_WRITE_ONLY | CL_MEM_USE_HOST_PTR,
        hw.length()+1,
        outH,
        &err);
        checkErr(err, "Buffer::Buffer()");

    cl::vector<cl::Device> devices;
    devices = context.getInfo<CL_CONTEXT_DEVICES>();
    checkErr(
        devices.size() > 0 ? CL_SUCCESS : -1, "devices.size() > 0");

    std::ifstream file("data/lesson1_kernels.cl");
    checkErr(file.is_open() ? CL_SUCCESS:-1, "lesson1_kernel.cl");

    std::string prog(
        std::istreambuf_iterator<char>(file),
        (std::istreambuf_iterator<char>()));

    cl::Program::Sources source(

        1,
        std::make_pair(prog.c_str(), prog.length()+1));

    cl::Program program(context, source);
    err = program.build(devices,"");
    checkErr(file.is_open() ? CL_SUCCESS : -1, "Program::build()");

    cl::Kernel kernel(program, "hello", &err);
    checkErr(err, "Kernel::Kernel()");

    err = kernel.setArg(0, outCL);
    checkErr(err, "Kernel::setArg()");

    cl::CommandQueue queue(context, devices[0], 0, &err);
    checkErr(err, "CommandQueue::CommandQueue()");

    cl::Event event;
    err = queue.enqueueNDRangeKernel(
        kernel,
        cl::NullRange,
        cl::NDRange(hw.length()+1),
         cl::NDRange(1, 1),
        NULL,
        &event);
    checkErr(err, "CommandQueue::enqueueNDRangeKernel()");

    event.wait();
    err = queue.enqueueReadBuffer(
        outCL,
        CL_TRUE,
        0,
        hw.length()+1,
        outH);
    checkErr(err, "CommandQueue::enqueueReadBuffer()");
    std::cout << outH;

    return EXIT_SUCCESS;
}
