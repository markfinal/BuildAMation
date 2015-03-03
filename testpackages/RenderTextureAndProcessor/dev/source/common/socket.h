/*
Copyright 2010-2015 Mark Final

This file is part of BuildAMation.

BuildAMation is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

BuildAMation is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
*/
#ifndef SOCKET_H
#define SOCKET_H

#include <winsock2.h>
#include <ws2tcpip.h>

namespace Networking
{

class Socket
{
public:
    static bool Initialize(const int li32Params);
    static void Release();

    explicit Socket(int af, int type, int protocol);
    ~Socket();

    bool IsValid() const;

    bool Connect(const void *lpName, const int liNameLen);
    bool Bind(const void *lpName, const int liNameLen);
    bool Listen(const int liMaxConnections);
    Socket Accept();
    void Shutdown();

    bool Send(const void *lpBuffer, const int liBufferLen);
    bool Receive(void *lpBuffer, int &liBufferLen);

private:
    explicit Socket(size_t lhSocket);

private:
    size_t mhSocket;
};

} // namespace Networking

#endif // SOCKET_H
