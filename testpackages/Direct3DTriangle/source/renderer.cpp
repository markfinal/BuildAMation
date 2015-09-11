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
#include "renderer.h"
#include "errorhandler.h"
#include <Windows.h>
#include <process.h>
#include <string>
#include <d3d9.h>
#include <d3dcompiler.h> // this header exists in all versions of the SDK

#if defined(D3D_COMPILER_VERSION) && (D3D_COMPILER_VERSION >= 47)
#define USING_WINDOWS8_SDK
#endif

#ifndef USING_WINDOWS8_SDK
#include <DxErr.h>
#include <D3DX9Core.h>
#include <D3DX9Shader.h>
#endif // USING_WINDOWS8_SDK

namespace
{

const char *getErrorMessage(
    ::HRESULT result)
{
#ifdef USING_WINDOWS8_SDK
    char *buffer;
    ::FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER |
        FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        result,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR)&buffer,
        0,
        NULL);
    if (buffer != NULL)
    {
        // TODO: buffer is not freed
        return buffer;
    }
    else
    {
        return "Unknown error";
    }
#else // USING_WINDOWS8_SDK
    const char *message = ::DXGetErrorString(result);
    return message;
#endif // USING_WINDOWS8_SDK
}

} // anonymous namespace

Renderer::Renderer(void *windowHandle)
: mhWindowHandle(windowHandle),
  mhThread(0),
  mpD3D(0),
  mpD3DDevice(0),
  mpImmediateModeVB(0),
  mpVertexShader(0),
  mpPixelShader(0),
  mpTimerQuery(0),
  mu32ImmediateModeVBLength(512u),
  mu32ImmediateModeOffset(0),
  mbQuitFlag(false)
{
}

void *Renderer::operator new(size_t size)
{
    void *memory = ::malloc(size);
    return memory;
}

void Renderer::operator delete(void *object)
{
    ::free(object);
}

void Renderer::Initialize()
{
    // create a thread for Direct3D rendering
    unsigned int threadId;
    this->mhThread = reinterpret_cast<HANDLE>(_beginthreadex(
        0,
        0,
        (unsigned (__stdcall *)(void *))threadFunction,
        this,
        0,
        &threadId));
}

void Renderer::Release()
{
    this->Exit();
    this->mhThread = 0;
}

bool Renderer::CreateDevice()
{
    ::IDirect3D9 *lpD3D = ::Direct3DCreate9(D3D_SDK_VERSION);
    if (0 == lpD3D)
    {
        REPORTERROR("Direct3D9 object creation failed");
        return false;
    }

    ::HRESULT lResult;

    ::D3DCAPS9 lCaps;
    lResult = lpD3D->GetDeviceCaps(D3DADAPTER_DEFAULT, ::D3DDEVTYPE_HAL, &lCaps);
    if (FAILED(lResult))
    {
        REPORTERROR2("GetDeviceCaps failed: 0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    ::DWORD lu32BehaviourFlags = 0;
    if (D3DDEVCAPS_HWTRANSFORMANDLIGHT == (lCaps.DevCaps & D3DDEVCAPS_HWTRANSFORMANDLIGHT))
    {
        lu32BehaviourFlags |= D3DCREATE_PUREDEVICE;
        lu32BehaviourFlags |= D3DCREATE_HARDWARE_VERTEXPROCESSING;
    }
    else
    {
        lu32BehaviourFlags |= D3DCREATE_SOFTWARE_VERTEXPROCESSING;
    }
    ::D3DPRESENT_PARAMETERS lPresentParams;
    ::ZeroMemory(&lPresentParams, sizeof(lPresentParams));
    lPresentParams.hDeviceWindow = static_cast<HWND>(this->mhWindowHandle);
    lPresentParams.BackBufferCount = 1;
    lPresentParams.BackBufferWidth = 512;
    lPresentParams.BackBufferHeight = 512;
    lPresentParams.BackBufferFormat = ::D3DFMT_X8R8G8B8;
    lPresentParams.EnableAutoDepthStencil = false;
    //lPresentParams.AutoDepthStencilFormat = ::D3DFMT_D24S8;
    lPresentParams.Windowed = true;
    lPresentParams.SwapEffect = ::D3DSWAPEFFECT_COPY;
    lPresentParams.PresentationInterval = D3DPRESENT_INTERVAL_DEFAULT;

    ::IDirect3DDevice9 *lpDevice = 0;
    lResult = lpD3D->CreateDevice(
        D3DADAPTER_DEFAULT,
        ::D3DDEVTYPE_HAL,
        0,
        lu32BehaviourFlags,
        &lPresentParams,
        &lpDevice);
    if (FAILED(lResult))
    {
        REPORTERROR2("Direct3D9 creation failed: 0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    this->mpD3D = lpD3D;
    this->mpD3DDevice = lpDevice;

    return true;
}

void Renderer::DestroyDevice()
{
    if (0 != this->mpD3DDevice)
    {
        ::IDirect3DDevice9 *lpD3DDevice = static_cast<::IDirect3DDevice9 *>(this->mpD3DDevice);
        lpD3DDevice->Release();
        this->mpD3DDevice = 0;
    }

    if (0 != this->mpD3D)
    {
        ::IDirect3D9 *lpD3D = static_cast<::IDirect3D9 *>(this->mpD3D);
        lpD3D->Release();
        this->mpD3D = 0;
    }
}

void Renderer::Exit()
{
    if (0 != this->mhThread)
    {
        mbQuitFlag = true;
        REPORTERROR("Request thread shutdown");
        WaitForSingleObject(this->mhThread, INFINITE);

        for (;;)
        {
            ::DWORD exitCode;
            ::BOOL result = ::GetExitCodeThread(this->mhThread, &exitCode);
            if (0 == result)
            {
                ::DWORD errorCode = ::GetLastError();
                REPORTWIN32ERROR("Unable to exit the rendering thread; error %d, '%s'", errorCode);
                return;
            }
            else if (STILL_ACTIVE == result)
            {
                REPORTERROR("Thread is still alive");
                continue;
            }
            else
            {
                REPORTERROR1("Thread exit code is %d", result);
                ::CloseHandle(this->mhThread);
                this->mhThread = 0;
                break;
            }
        }
    }
}

// route to the worker thread
void Renderer::threadFunction(void *param)
{
    Renderer *lpRenderer = static_cast<Renderer *>(param);

    if (!lpRenderer->CreateDevice())
    {
        return;
    }

    lpRenderer->runThread();

    lpRenderer->DestroyDevice();

    // terminate rendering thread
    _endthread();
    REPORTERROR("Render thread has terminated");
}

struct Vertex
{
    float mfPosition[2];
    float mfColor[3];

    static ::D3DVERTEXELEMENT9 *GetVertexElements()
    {
        static ::D3DVERTEXELEMENT9 laVertexElements[3] =
        {
            { 0, 0, ::D3DDECLTYPE_FLOAT2, ::D3DDECLMETHOD_DEFAULT, ::D3DDECLUSAGE_POSITION, 0 },
            { 0, 8, ::D3DDECLTYPE_FLOAT3, ::D3DDECLMETHOD_DEFAULT, ::D3DDECLUSAGE_COLOR, 0 },
            D3DDECL_END()
        };

        return laVertexElements;
    }
};

// rendering loop
void Renderer::runThread()
{
    ::HRESULT lResult;

    ::IDirect3DDevice9 *lpD3DDevice = static_cast<::IDirect3DDevice9 *>(this->mpD3DDevice);

    ::IDirect3DSurface9 *lpDefaultRenderTarget = 0;
    lResult = lpD3DDevice->GetRenderTarget(0, &lpDefaultRenderTarget);
    if (FAILED(lResult))
    {
        REPORTERROR2("GetRenderTarget 0 failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return;
    }
    lpDefaultRenderTarget->Release();

    ::IDirect3DSurface9 *lpDefaultDepthStencilTarget = 0;
    lResult = lpD3DDevice->GetDepthStencilSurface(&lpDefaultDepthStencilTarget);
    if (0 != lpDefaultDepthStencilTarget)
    {
        lpDefaultDepthStencilTarget->Release();
    }

    if (!this->CreateImmediateModeVertexBuffer())
    {
        REPORTERROR("Unable to create immediate mode vertex buffer");
        return;
    }

    ::D3DVERTEXELEMENT9 *laVertexElements = Vertex::GetVertexElements();
    ::IDirect3DVertexDeclaration9 *lpVertexDeclaration = 0;
    lResult = lpD3DDevice->CreateVertexDeclaration(laVertexElements, &lpVertexDeclaration);
    if (FAILED(lResult))
    {
        REPORTERROR2("CreateVertexDeclaration failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return;
    }

    if (!this->CreateShaders())
    {
        REPORTERROR("Unable to create shaders");
        return;
    }

    this->CreateTimerQuery();

    // rendering loop
    while(!this->mbQuitFlag)
    {
        ::SwitchToThread();

        this->StartTimerQuery();

        lResult = lpD3DDevice->BeginScene();
        if (FAILED(lResult))
        {
            REPORTERROR2("BeginScene failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        ::D3DVIEWPORT9 lViewport;
        lViewport.X = 0;
        lViewport.Y = 0;
        lViewport.Width = 512;
        lViewport.Height = 512;
        lViewport.MinZ = 0;
        lViewport.MaxZ = 1;
        lResult = lpD3DDevice->SetViewport(&lViewport);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetViewport failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->Clear(0, 0, D3DCLEAR_TARGET, D3DCOLOR_XRGB(255,255,255), 0, 0);
        if (FAILED(lResult))
        {
            REPORTERROR2("Clear 0 failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        const unsigned int lu32VertexCount = 3u;
        unsigned int lu32Offset;
        Vertex *lpVertexData = static_cast<Vertex *>(this->BeginImmediateMode(lu32VertexCount * sizeof(Vertex), lu32Offset));

        lpVertexData->mfPosition[0] = -0.5f;
        lpVertexData->mfPosition[1] = -0.5f;
        lpVertexData->mfColor[0] = 1.0f;
        lpVertexData->mfColor[1] = 0;
        lpVertexData->mfColor[2] = 0;
        ++lpVertexData;

        lpVertexData->mfPosition[0] = 0.5f;
        lpVertexData->mfPosition[1] = -0.5f;
        lpVertexData->mfColor[0] = 0;
        lpVertexData->mfColor[1] = 1.0f;
        lpVertexData->mfColor[2] = 0;
        ++lpVertexData;

        lpVertexData->mfPosition[0] = 0.5f;
        lpVertexData->mfPosition[1] = 0.5f;
        lpVertexData->mfColor[0] = 0;
        lpVertexData->mfColor[1] = 0;
        lpVertexData->mfColor[2] = 1.0f;
        ++lpVertexData;

        this->EndImmediateMode();

        lResult = lpD3DDevice->SetVertexDeclaration(lpVertexDeclaration);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetVertexDeclaration failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        ::IDirect3DVertexBuffer9 *lpVertexBuffer = static_cast<::IDirect3DVertexBuffer9 *>(this->mpImmediateModeVB);
        lResult = lpD3DDevice->SetStreamSource(0, lpVertexBuffer, lu32Offset, sizeof(Vertex));
        if (FAILED(lResult))
        {
            REPORTERROR2("SetStreamSource failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->SetRenderState(::D3DRS_CULLMODE, ::D3DCULL_NONE);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetRenderState D3DRS_CULLMODE failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->SetRenderState(::D3DRS_ZFUNC, ::D3DCMP_ALWAYS);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetRenderState D3DRS_ZFUNC failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->SetRenderState(::D3DRS_ZWRITEENABLE, FALSE);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetRenderState D3DRS_ZWRITEENABLE failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->SetRenderState(::D3DRS_LIGHTING, FALSE);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetRenderState D3DRS_LIGHTING failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->SetRenderState(::D3DRS_ALPHABLENDENABLE, FALSE);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetRenderState D3DRS_ALPHABLENDENABLE failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        ::IDirect3DVertexShader9 *lpVertexShader = static_cast<::IDirect3DVertexShader9 *>(this->mpVertexShader);
        lpD3DDevice->SetVertexShader(lpVertexShader);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetVertexShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        ::IDirect3DPixelShader9 *lpPixelShader = static_cast<::IDirect3DPixelShader9 *>(this->mpPixelShader);
        lpD3DDevice->SetPixelShader(lpPixelShader);
        if (FAILED(lResult))
        {
            REPORTERROR2("SetPixelShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        const unsigned int luNumTriangles = lu32VertexCount / 3u;
        lResult = lpD3DDevice->DrawPrimitive(::D3DPT_TRIANGLELIST, 0, luNumTriangles);
        if (FAILED(lResult))
        {
            REPORTERROR2("DrawPrimitive failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        lResult = lpD3DDevice->EndScene();
        if (FAILED(lResult))
        {
            REPORTERROR2("EndScene failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }

        this->EndTimerQuery();

        // Note: this involves a synchronous wait
        REPORTERROR1("GPU time = %.4f ms", this->GetMillisecondsElapsed());

        lResult = lpD3DDevice->Present(0, 0, 0, 0);
        if (FAILED(lResult))
        {
            REPORTERROR2("Present failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
            return;
        }
    }

    REPORTERROR("Begin shutting down render thread");

    this->DestroyTimerQuery();
    this->DestroyShaders();

    if (0 != lpVertexDeclaration)
    {
        lpVertexDeclaration->Release();
    }

    this->DestroyImmediateModeVertexBuffer();
}

bool Renderer::CreateImmediateModeVertexBuffer()
{
    ::IDirect3DDevice9 *lpD3DDevice = static_cast<::IDirect3DDevice9 *>(this->mpD3DDevice);
    const ::UINT lu32Length = this->mu32ImmediateModeVBLength;
    const ::DWORD lu32Usage = D3DUSAGE_DYNAMIC | D3DUSAGE_WRITEONLY;
    const ::DWORD lu32FVF = 0;
    const ::D3DPOOL lePool = ::D3DPOOL_DEFAULT;
    ::IDirect3DVertexBuffer9 *lpVertexBuffer = 0;
    ::HRESULT lResult = lpD3DDevice->CreateVertexBuffer(
        lu32Length,
        lu32Usage,
        lu32FVF,
        lePool,
        &lpVertexBuffer,
        0);
    if (FAILED(lResult))
    {
        REPORTERROR2("CreateVertexBuffer failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    this->mpImmediateModeVB = lpVertexBuffer;
    this->mu32ImmediateModeOffset = 0;

    return true;
}

void Renderer::DestroyImmediateModeVertexBuffer()
{
    if (0 != this->mpImmediateModeVB)
    {
        ::IDirect3DVertexBuffer9 *lpVertexBuffer = static_cast<::IDirect3DVertexBuffer9 *>(this->mpImmediateModeVB);
        lpVertexBuffer->Release();

        this->mpImmediateModeVB = 0;
        this->mu32ImmediateModeOffset = 0;
    }
}

void *
Renderer::BeginImmediateMode(const unsigned int lu32Size, unsigned int &lu32Offset)
{
    ::DWORD lu32Flags = 0;
    unsigned int lu32CurrentOffset = this->mu32ImmediateModeOffset;
    const unsigned int lu32NextOffset = lu32CurrentOffset + lu32Size;
    if (lu32NextOffset >= this->mu32ImmediateModeVBLength)
    {
        lu32CurrentOffset = 0;
        lu32Flags |= D3DLOCK_DISCARD;
    }
    else
    {
        lu32Flags |= D3DLOCK_NOOVERWRITE;
    }

    ::IDirect3DVertexBuffer9 *lpVertexBuffer = static_cast<::IDirect3DVertexBuffer9 *>(this->mpImmediateModeVB);
    void *lpData;
    ::HRESULT lResult = lpVertexBuffer->Lock(lu32CurrentOffset, lu32Size, &lpData, lu32Flags);
    if (FAILED(lResult))
    {
        REPORTERROR2("VertexBuffer Lock failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return 0;
    }

    lu32Offset = lu32CurrentOffset;
    this->mu32ImmediateModeOffset = lu32NextOffset;

    return lpData;
}

void Renderer::EndImmediateMode()
{
    ::IDirect3DVertexBuffer9 *lpVertexBuffer = static_cast<::IDirect3DVertexBuffer9 *>(this->mpImmediateModeVB);
    ::HRESULT lResult = lpVertexBuffer->Unlock();
    if (FAILED(lResult))
    {
        REPORTERROR2("VertexBuffer Unlock failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return;
    }
}

bool Renderer::CreateShaders()
{
    ::IDirect3DDevice9 *lpD3DDevice = static_cast<::IDirect3DDevice9 *>(this->mpD3DDevice);

    std::string vertexSource;
    vertexSource += "struct VSInput\n";
    vertexSource += "{\n";
    vertexSource += "\tfloat4 position : POSITION;\n";
    vertexSource += "\tfloat4 color : COLOR0;\n";
    vertexSource += "};\n";
    vertexSource += "struct VSOutput\n";
    vertexSource += "{\n";
    vertexSource += "\tfloat4 position : POSITION;\n";
    vertexSource += "\tfloat4 color : COLOR0;\n";
    vertexSource += "};\n";
    vertexSource += "VSOutput main(VSInput input)\n";
    vertexSource += "{\n";
    vertexSource += "\tVSOutput output;\n";
    vertexSource += "\toutput.position = input.position;\n";
    vertexSource += "\toutput.color = input.color;\n";
    vertexSource += "\treturn output;\n";
    vertexSource += "}\n";

    ::HRESULT lResult;
#ifdef USING_WINDOWS8_SDK
    ::LPD3DBLOB lpShaderBuffer = 0;
    ::LPD3DBLOB lpErrorMessages = 0;
    ::DWORD lu32Flags = D3DCOMPILE_OPTIMIZATION_LEVEL3;
    lResult = ::D3DCompile(
        vertexSource.c_str(),
        static_cast<UINT>(vertexSource.length()),
        0,
        0,
        0,
        "main",
        "vs_2_0",
        lu32Flags,
        0,
        &lpShaderBuffer,
        &lpErrorMessages);
    if (FAILED(lResult))
    {
        REPORTERROR2("D3DCompile failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        REPORTERROR1("Messages: '%s'", lpErrorMessages->GetBufferPointer());
        REPORTERROR1("'%s'", vertexSource.c_str());
        return false;
    }

    if (0 != lpErrorMessages)
    {
        lpErrorMessages->Release();
    }
#else
    ::LPD3DXBUFFER lpShaderBuffer = 0;
    ::LPD3DXBUFFER lpErrorMessages = 0;
    ::LPD3DXCONSTANTTABLE lpConstantTable = 0;
    ::DWORD lu32Flags = D3DXSHADER_OPTIMIZATION_LEVEL3;
    lResult = ::D3DXCompileShader(
        vertexSource.c_str(),
        static_cast<UINT>(vertexSource.length()),
        0,
        0,
        "main",
        "vs_2_0",
        lu32Flags,
        &lpShaderBuffer,
        &lpErrorMessages,
        &lpConstantTable);
    if (FAILED(lResult))
    {
        REPORTERROR2("D3DXCompileShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        REPORTERROR1("Messages: '%s'", lpErrorMessages->GetBufferPointer());
        REPORTERROR1("'%s'", vertexSource.c_str());
        return false;
    }

    if (0 != lpErrorMessages)
    {
        lpErrorMessages->Release();
    }
    if (0 != lpConstantTable)
    {
        lpConstantTable->Release();
    }
#endif // USING_WINDOWS8_SDK

    ::IDirect3DVertexShader9 *lpVertexShader = 0;
    ::DWORD *lpByteCode = static_cast<::DWORD *>(lpShaderBuffer->GetBufferPointer());
    lResult = lpD3DDevice->CreateVertexShader(lpByteCode, &lpVertexShader);
    if (FAILED(lResult))
    {
        REPORTERROR2("CreateVertexShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    lpShaderBuffer->Release();

    std::string pixelSource;
    pixelSource += "struct PSInput\n";
    pixelSource += "{\n";
    pixelSource += "\tfloat4 position : POSITION;\n";
    pixelSource += "\tfloat4 color : COLOR0;\n";
    pixelSource += "};\n";
    pixelSource += "float4 main(PSInput input) : COLOR0\n";
    pixelSource += "{\n";
    pixelSource += "\tfloat4 color = input.color;\n";
    pixelSource += "\treturn color;\n";
    pixelSource += "}\n";

#ifdef USING_WINDOWS8_SDK
    lResult = ::D3DCompile(
        pixelSource.c_str(),
        static_cast<UINT>(pixelSource.length()),
        0,
        0,
        0,
        "main",
        "ps_2_0",
        lu32Flags,
        0,
        &lpShaderBuffer,
        &lpErrorMessages);
    if (FAILED(lResult))
    {
        REPORTERROR2("D3DCompile failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        REPORTERROR1("Messages: '%s'", lpErrorMessages->GetBufferPointer());
        REPORTERROR1("'%s'", vertexSource.c_str());
        return false;
    }

    if (0 != lpErrorMessages)
    {
        lpErrorMessages->Release();
    }
#else
    lResult = ::D3DXCompileShader(
        pixelSource.c_str(),
        static_cast<UINT>(pixelSource.length()),
        0,
        0,
        "main",
        "ps_2_0",
        lu32Flags,
        &lpShaderBuffer,
        &lpErrorMessages,
        &lpConstantTable);
    if (FAILED(lResult))
    {
        REPORTERROR2("D3DXCompileShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        REPORTERROR1("Messages: '%s'", lpErrorMessages->GetBufferPointer());
        REPORTERROR1("'%s'", pixelSource.c_str());
        return false;
    }

    if (0 != lpErrorMessages)
    {
        lpErrorMessages->Release();
    }
    if (0 != lpConstantTable)
    {
        lpConstantTable->Release();
    }
#endif

    ::IDirect3DPixelShader9 *lpPixelShader = 0;
    lpByteCode = static_cast<::DWORD *>(lpShaderBuffer->GetBufferPointer());
    lResult = lpD3DDevice->CreatePixelShader(lpByteCode, &lpPixelShader);
    if (FAILED(lResult))
    {
        REPORTERROR2("CreatePixelShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    lpShaderBuffer->Release();

    this->mpVertexShader = lpVertexShader;
    this->mpPixelShader = lpPixelShader;

    return true;
}

void Renderer::DestroyShaders()
{
    if (0 != this->mpPixelShader)
    {
        ::IDirect3DPixelShader9 *lpPixelShader = static_cast<::IDirect3DPixelShader9 *>(this->mpPixelShader);
        lpPixelShader->Release();
        this->mpPixelShader = 0;
    }

    if (0 != this->mpVertexShader)
    {
        ::IDirect3DVertexShader9 *lpVertexShader = static_cast<::IDirect3DVertexShader9 *>(this->mpVertexShader);
        lpVertexShader->Release();
        this->mpVertexShader = 0;
    }
}

bool Renderer::CreateTimerQuery()
{
    ::IDirect3DDevice9 *lpD3DDevice = static_cast<::IDirect3DDevice9 *>(this->mpD3DDevice);

    ::IDirect3DQuery9 *lpQuery = 0;
    ::HRESULT lResult;
    lResult = lpD3DDevice->CreateQuery(::D3DQUERYTYPE_TIMESTAMPFREQ, &lpQuery);
    if (FAILED(lResult))
    {
        REPORTERROR2("CreatePixelShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    uint64 lu64TimerFrequency;
    while (S_FALSE == lpQuery->GetData(&lu64TimerFrequency, sizeof(lu64TimerFrequency), D3DGETDATA_FLUSH))
    {
    }

    lpQuery->Release();

    lResult = lpD3DDevice->CreateQuery(::D3DQUERYTYPE_TIMESTAMP, &lpQuery);
    if (FAILED(lResult))
    {
        REPORTERROR2("CreatePixelShader failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return false;
    }

    this->mu64TimerFrequency = lu64TimerFrequency;
    this->mpTimerQuery = lpQuery;

    return true;
}

void Renderer::DestroyTimerQuery()
{
    if (0 != this->mpTimerQuery)
    {
        ::IDirect3DQuery9 *lpQuery = static_cast<::IDirect3DQuery9 *>(this->mpTimerQuery);
        lpQuery->Release();
        this->mpTimerQuery = 0;
    }
}

void Renderer::StartTimerQuery()
{
    if (0 == this->mpTimerQuery)
    {
        return;
    }

    ::IDirect3DQuery9 *lpQuery = static_cast<::IDirect3DQuery9 *>(this->mpTimerQuery);
    ::HRESULT lResult = lpQuery->Issue(D3DISSUE_END);
    if (FAILED(lResult))
    {
        REPORTERROR2("Query Issue failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return;
    }

    uint64 lu64StartTime;
    while (S_FALSE == lpQuery->GetData(&lu64StartTime, sizeof(lu64StartTime), D3DGETDATA_FLUSH))
    {
    }

    this->mu64StartTime = lu64StartTime;
}

void Renderer::EndTimerQuery()
{
    if (0 == this->mpTimerQuery)
    {
        return;
    }

    ::IDirect3DQuery9 *lpQuery = static_cast<::IDirect3DQuery9 *>(this->mpTimerQuery);
    ::HRESULT lResult = lpQuery->Issue(D3DISSUE_END);
    if (FAILED(lResult))
    {
        REPORTERROR2("Query Issue failed, HR=0x%x '%s'", lResult, getErrorMessage(lResult));
        return;
    }

    uint64 lu64EndTime;
    while (S_FALSE == lpQuery->GetData(&lu64EndTime, sizeof(lu64EndTime), D3DGETDATA_FLUSH))
    {
    }

    this->mu64EndTime = lu64EndTime;
}

float Renderer::GetMillisecondsElapsed()
{
    if (0 == this->mpTimerQuery)
    {
        return 0;
    }

    uint64 lu64TimeDifference = this->mu64EndTime - this->mu64StartTime;
    // TODO: this conversion looks wrong
    float lfMillisecondDifference = (lu64TimeDifference * 1000 * 1000 * 1000 * 1000) / float(this->mu64TimerFrequency);

    return lfMillisecondDifference;
}
