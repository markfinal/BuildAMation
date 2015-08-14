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
#include "common.h"
#include "socket.h"
#include "errorhandler.h"
#include "texture.h"

#include <iostream>

#define DEFAULT_PORT "8888"

int
main(int UNUSEDARG(argc), const char *UNUSEDARG(argv)[])
{
    int li32SocketVersion = MAKEWORD(1, 1);
    if (!Networking::Socket::Initialize(li32SocketVersion))
    {
        return -1;
    }

#if 1
    Networking::Socket mySocket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    ::SOCKADDR_IN lServerInf;
    lServerInf.sin_family = AF_INET;
    lServerInf.sin_addr.s_addr = INADDR_ANY;
    lServerInf.sin_port = ::htons(8888);

    bool lbResult = mySocket.Bind(&lServerInf, sizeof(lServerInf));
#else
    struct addrinfo hints;
    struct addrinfo *result;

    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    // Resolve the server address and port
    int iResult = ::getaddrinfo(NULL, DEFAULT_PORT, &hints, &result);
    if ( iResult != 0 )
    {
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to getaddrinfo, error %d, '%s'", iResult);
        return -1;
    }

    Networking::Socket mySocket(result->ai_family, result->ai_socktype, result->ai_protocol);
    bool lbResult = mySocket.Bind(result->ai_addr, result->ai_addrlen);

    ::freeaddrinfo(result);
#endif

    if (!lbResult)
    {
        return -1;
    }

    lbResult = mySocket.Listen(10);
    if (!lbResult)
    {
        return -1;
    }

    Networking::Socket clientSocket = mySocket.Accept();

    char buffer[256];
    int len;
    clientSocket.Receive(buffer, len);

    REPORTERROR2("Received %d bytes, '%s'", len, buffer);

    TextureHeader header;
    header.mu32Width = 1;
    header.mu32Height = 1;
    header.mu32TotalTextureDataSize = 1 * 1 * 4;
    clientSocket.Send(&header, sizeof(header));

    unsigned char *lpImageData = new unsigned char[header.mu32TotalTextureDataSize];
    lpImageData[0] = 255;
    lpImageData[1] = 255;
    lpImageData[2] = 0;
    lpImageData[3] = 255;
    clientSocket.Send(lpImageData, header.mu32TotalTextureDataSize);

    return 0;
}
