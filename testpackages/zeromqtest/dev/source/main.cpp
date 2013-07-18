#include "zmq.h"

int main()
{
    void *context = zmq_ctx_new();
    zmq_ctx_destroy(context);
    return 0;
}
