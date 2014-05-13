#include "library.h"
#include "library2.h"
#include <stdio.h>

int main()
{
    printf("Hello world, C\n");
    printf("From library, '%s'\n", libraryFunction());
    printf("From library2, '%d'\n", library2Function());

    return 0;
}
