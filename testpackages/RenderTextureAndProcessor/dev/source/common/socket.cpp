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
#include "socket.h"
#include "errorhandler.h"

#include <winsock.h>

namespace Networking
{

bool
Socket::Initialize(const int li32Params)
{
    const ::WORD li16SocketVersion = static_cast< ::WORD>(li32Params);
    ::WSADATA lWsaData;
    int li32ErrorCode = ::WSAStartup(li16SocketVersion, &lWsaData);
    if (0 != li32ErrorCode)
    {
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "WSAStartup failed, error %d, '%s'", li32ErrorCode);
        return false;
    }

    const ::BYTE li8RequestedMajorVersion = LOBYTE(li16SocketVersion);
    const ::BYTE li8RequestedMinorVersion = HIBYTE(li16SocketVersion);
    const ::BYTE li8StartedMajorVersion = LOBYTE(lWsaData.wVersion);
    const ::BYTE li8StartedMinorVersion = HIBYTE(lWsaData.wVersion);
    const ::BYTE li8MaxMajorVersion = LOBYTE(lWsaData.wHighVersion);
    const ::BYTE li8MaxMinorVersion = HIBYTE(lWsaData.wHighVersion);

    // check that we have the expected version
    if (li8StartedMajorVersion != li8RequestedMajorVersion ||
        li8StartedMinorVersion != li8RequestedMinorVersion)
    {
        REPORTERROR4("Requested version %d.%d. Got version %d.%d", li8RequestedMajorVersion, li8RequestedMinorVersion, li8StartedMajorVersion, li8StartedMinorVersion);
        return false;
    }

    REPORTERROR2("Current version   = %d.%d", li8StartedMajorVersion, li8StartedMinorVersion);
    REPORTERROR2("Maximum version   = %d.%d", li8MaxMajorVersion, li8MaxMinorVersion);
    REPORTERROR1("Max sockets       = %d", lWsaData.iMaxSockets);
    REPORTERROR1("Max Udp datagrams = %d", lWsaData.iMaxUdpDg);
    REPORTERROR1("Description       = '%s'", lWsaData.szDescription);
    REPORTERROR1("System status     = '%s'", lWsaData.szSystemStatus);

    return true;
}

void
Socket::Release()
{
    ::WSACleanup();
}

Socket::Socket(int af, int type, int protocol)
    : mhSocket(INVALID_SOCKET)
{
    ::SOCKET lSocket = ::socket(af, type, protocol);
    if (INVALID_SOCKET == lSocket)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to open socket, error %d, '%s'", liSocketError);
        return;
    }

    REPORTERROR1("Created socket, %d", lSocket);

    this->mhSocket = static_cast<unsigned int>(lSocket);
}

Socket::Socket(size_t lhSocket)
    : mhSocket(lhSocket)
{
    REPORTERROR1("Wrapped existing socket, %d", lhSocket);
}

Socket::~Socket()
{
    if (INVALID_SOCKET == this->mhSocket)
    {
        return;
    }

    REPORTERROR1("Closed socket, %d", this->mhSocket);

    ::closesocket(this->mhSocket);
    this->mhSocket = INVALID_SOCKET;
}

bool
Socket::IsValid() const
{
    bool lbIsValid = (INVALID_SOCKET != this->mhSocket);
    return lbIsValid;
}

bool
Socket::Connect(const void *lpName, const int liNameLen)
{
    if (!this->IsValid())
    {
        return false;
    }

    const sockaddr *lpSockAddr = static_cast<const sockaddr *>(lpName);
    int liConnectSuccess = ::connect(this->mhSocket, lpSockAddr, liNameLen);
    if (SOCKET_ERROR == liConnectSuccess)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to make socket connect, error %d, '%s'", liSocketError);
        return false;
    }

    REPORTERROR1("Socket %d connected", this->mhSocket);

    return true;
}

bool
Socket::Bind(const void *lpName, const int liNameLen)
{
    if (!this->IsValid())
    {
        return false;
    }

    const sockaddr *lpSockAddr = static_cast<const sockaddr *>(lpName);
    int liConnectSuccess = ::bind(this->mhSocket, lpSockAddr, liNameLen);
    if (SOCKET_ERROR == liConnectSuccess)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to make socket bind, error %d, '%s'", liSocketError);
        return false;
    }

    REPORTERROR1("Socket %d bound", this->mhSocket);

    return true;
}

bool
Socket::Listen(const int liMaxConnections)
{
    if (!this->IsValid())
    {
        return false;
    }

    int liSocketError = ::listen(this->mhSocket, liMaxConnections);
    if (SOCKET_ERROR == liSocketError)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to make socket listen, error %d, '%s'", liSocketError);
        return false;
    }

    REPORTERROR1("Socket %d listening", this->mhSocket);

    return true;
}

Socket
Socket::Accept()
{
    if (!this->IsValid())
    {
        return Socket(INVALID_SOCKET);
    }

    ::SOCKET lClientSocket = ::accept(this->mhSocket, 0, 0);
    if (INVALID_SOCKET == lClientSocket)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to make socket accept, error %d, '%s'", liSocketError);
    }

    REPORTERROR1("Socket %d accepted", this->mhSocket);

    return Socket(lClientSocket);
}

void
Socket::Shutdown()
{
    if (!this->IsValid())
    {
        return;
    }

    REPORTERROR1("Socket %d shutting down", this->mhSocket);

    ::shutdown(this->mhSocket, SD_BOTH);
}

bool
Socket::Send(const void *lpBuffer, const int li32BufferLen)
{
    if (!this->IsValid())
    {
        return false;
    }

    int flags = 0;
    int liSocketError = ::send(this->mhSocket, static_cast<const char *>(lpBuffer), li32BufferLen, flags);
    if (SOCKET_ERROR == liSocketError)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to make socket send, error %d, '%s'", liSocketError);
        return false;
    }

    REPORTERROR2("Socket %d sent %d bytes of data", this->mhSocket, li32BufferLen);

    return true;
}

bool
Socket::Receive(void *lpBuffer, int &li32BufferLen)
{
    if (!this->IsValid())
    {
        return false;
    }

    char buffer[256];
    int bufferLen = 256;
    int flags = 0;
    int liSocketError = ::recv(this->mhSocket, buffer, bufferLen, flags);
    if (SOCKET_ERROR == liSocketError)
    {
        int liSocketError = ::WSAGetLastError();
        REPORTWIN32MODULEERROR(::GetModuleHandle("WS2_32.dll"), "Failed to make socket receive, error %d, '%s'", liSocketError);
        return false;
    }

    REPORTERROR2("Socket %d received %d bytes of data", this->mhSocket, liSocketError);

    li32BufferLen = liSocketError;
    memcpy(lpBuffer, buffer, liSocketError);

    return true;
}

} // namespace Networking
