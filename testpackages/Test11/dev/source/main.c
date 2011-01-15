#include <stdio.h>

extern char *PlatformName();

int main()
{
	printf("Platform is '%s'\n", PlatformName());
	return 0;
}
