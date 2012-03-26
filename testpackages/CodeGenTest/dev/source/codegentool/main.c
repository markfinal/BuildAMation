#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>

int
main(int argc, char* argv[])
{
    if (argc < 3)
    {
        fprintf(stderr, "Not enough arguments");
        return -1;
    }

    /*fprintf(stdout, "Tool is '%s'\n", argv[0]);*/
    /*fprintf(stdout, "Arg1 is '%s'\n", argv[1]);*/
    /*fprintf(stdout, "Arg2 is '%s'\n", argv[2]);*/
 
    {
        char path[256];
        char body[256];
        FILE *file;
        
#ifdef WIN32
        sprintf(path, "%s\\%s.c", argv[1], argv[2]);
#else
        sprintf(path, "%s/%s.c", argv[1], argv[2]);
#endif
        file = fopen(path, "wt");
        if (0 == file)
        {
            fprintf(stderr, "Unable to open '%s' for writing", path);
            return -2;
        }
        
        sprintf(body, "#include <stdio.h>\n\nvoid MyGeneratedFunction(){ printf(\"Hello world\\n\"); }\n");
        fwrite(body, 1, strlen(body), file);
        
        fclose(file);
        fprintf(stdout, "Generated source file written to '%s'\n", path);
    }
    
    return 0;
}
