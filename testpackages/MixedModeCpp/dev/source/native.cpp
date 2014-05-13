#include <stdio.h>

extern void __stdcall DoManagedStuff();

int main()
{
    DoManagedStuff();
    printf("Hello from native C++\n");
    return 0;
}
