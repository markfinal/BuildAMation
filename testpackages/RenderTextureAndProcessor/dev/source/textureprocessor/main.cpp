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
