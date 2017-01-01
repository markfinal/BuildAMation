/*
Copyright (c) 2010-2017, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

#if defined(D_BAM_PLATFORM_WINDOWS)
#include <Windows.h>

const char pathSep = '\\';

static int
fileExists(
    const char *inPath)
{
    DWORD fileAttrs = GetFileAttributes(inPath);
    if (INVALID_FILE_ATTRIBUTES == fileAttrs)
    {
        return -1;
    }

    if (FILE_ATTRIBUTE_NORMAL != (fileAttrs & FILE_ATTRIBUTE_NORMAL))
    {
        if (FILE_ATTRIBUTE_ARCHIVE != (fileAttrs & FILE_ATTRIBUTE_ARCHIVE))
        {
            return -1;
        }
    }

    return 0;
}

static int
directoryExists(
    const char *inPath)
{
    DWORD fileAttrs = GetFileAttributes(inPath);
    if (INVALID_FILE_ATTRIBUTES == fileAttrs)
    {
        return -1;
    }

    if (FILE_ATTRIBUTE_DIRECTORY != (fileAttrs & FILE_ATTRIBUTE_DIRECTORY))
    {
        return -1;
    }

    return 0;
}
#else
#include <sys/stat.h>

const char pathSep = '/';

static int
fileExists(
    const char *inPath)
{
    struct stat info;
    int exists = stat(inPath, &info);
    if (0 != exists)
    {
        return -1;
    }
    int doesExist = S_ISREG(info.st_mode);
    return (0 == doesExist);
}

static int
directoryExists(
    const char *inPath)
{
    struct stat info;
    int exists = stat(inPath, &info);
    if (0 != exists)
    {
        return -1;
    }
    int doesExist = S_ISDIR(info.st_mode);
    return (0 == doesExist);
}
#endif

static char *
getParentDirectory(
    const char *exePath)
{
    size_t len = strlen(exePath);
    char *copy = malloc(len + 1);
    char *lastPathSep = 0;
    strcpy(copy, exePath);
    lastPathSep = strrchr(copy, pathSep);
    if (0 == lastPathSep)
    {
        free(copy);
        return 0;
    }
    *lastPathSep = '\0';
    return copy;
}

static char *
joinPaths(
    const char *dir,
    const char *filename)
{
    size_t lendir = strlen(dir);
    size_t lenfile = strlen(filename);
    size_t lentotal = lendir + lenfile + 2; /* pathsep + null char */
    char *combined = malloc(lentotal);
    strcpy(combined, dir);
    combined[lendir] = pathSep;
    combined[lendir + 1] = '\0';
    strcat(combined, filename);
    return combined;
}

static int
validateTestFile1(
    const char *parentDir)
{
    int exitStatus = 0;
    char *path = joinPaths(parentDir, "testfile1.txt");
    if (0 == fileExists(path))
    {
        fprintf(stdout, "FOUND file: '%s'\n", path);
    }
    else
    {
        fprintf(stderr, "Unable to locate file '%s'\n", path);
        ++exitStatus;
    }
    free(path);
    return exitStatus;
}

static int
validateTestDir1_renamed(
    const char *parentDir,
    const char *dirName)
{
    int exitStatus = 0;
    char *dirpath = joinPaths(parentDir, dirName);
    if (0 == directoryExists(dirpath))
    {
        fprintf(stdout, "FOUND directory: '%s'\n", dirpath);

        /* check for files within directory */
        {
            char *path = joinPaths(dirpath, "file1.txt");
            if (0 == fileExists(path))
            {
                fprintf(stdout, "FOUND file: '%s'\n", path);
            }
            else
            {
                fprintf(stderr, "Unable to locate file '%s'\n", path);
                ++exitStatus;
            }
            free(path);
        }
            {
                char *path = joinPaths(dirpath, "file2.txt");
                if (0 == fileExists(path))
                {
                    fprintf(stdout, "FOUND file: '%s'\n", path);
                }
                else
                {
                    fprintf(stderr, "Unable to locate file '%s'\n", path);
                    ++exitStatus;
                }
                free(path);
            }

            /* check for subdirectory */
            {
                char *subdirPath = joinPaths(dirpath, "subdir");
                if (0 == directoryExists(subdirPath))
                {
                    fprintf(stdout, "FOUND directory: '%s'\n", subdirPath);

                    /* check for file in subdir */
                    {
                        char *path = joinPaths(subdirPath, "file3.txt");
                        if (0 == fileExists(path))
                        {
                            fprintf(stdout, "FOUND file: '%s'\n", path);
                        }
                        else
                        {
                            fprintf(stderr, "Unable to locate file '%s'\n", path);
                            ++exitStatus;
                        }
                        free(path);
                    }
                }
                else
                {
                    fprintf(stderr, "Unable to directory file '%s'\n", subdirPath);
                    ++exitStatus;
                }
                free(subdirPath);
            }
    }
    else
    {
        fprintf(stderr, "Unable to locate directory '%s'\n", dirpath);
        ++exitStatus;
    }
    free(dirpath);
    return exitStatus;
}

static int
validateTestDir1(
    const char *parentDir)
{
    return validateTestDir1_renamed(parentDir, "testdir1");
}

int
main(
    int argc,
    char **argv)
{
    char *exePath = argv[0];
    char *exeDir = getParentDirectory(exePath);
    int exitStatus = 0;
    char *libDir = 0;

    /* check if single data file exists next to executable */
    exitStatus += validateTestFile1(exeDir);

    /* check if directory (and all files/subdirs within) exists next to executable */
    exitStatus += validateTestDir1(exeDir);

    libDir = joinPaths(exeDir, "lib");
    /* check directory again, but in a different location, and a different name */
    exitStatus += validateTestDir1_renamed(libDir, "testdir1_renamed");

    free(libDir);
    free(exeDir);

    if (0 == exitStatus)
    {
        fprintf(stdout, "\nNo errors found. All files are present in the expected locations.\n");
    }
    else
    {
        fprintf(stderr, "\nSome files were not found in the expected locations. See log above.\n");
    }
    return exitStatus;
}
