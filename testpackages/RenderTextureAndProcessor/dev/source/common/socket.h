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
