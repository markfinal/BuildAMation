EXTRN GetStdHandle: PROC
EXTRN WriteFile:    PROC
EXTRN lstrlen:      PROC
EXTRN ExitProcess:  PROC

.DATA

    hFile        QWORD 0
    msglen       DWORD 0
    BytesWritten DWORD 0
    msg          BYTE  "Hello x64 World!", 13, 10, 0

.CODE

    main PROC

    ;int 3              ; breakpoint for debugger

    sub rsp, 28h

    lea rcx, msg
    call lstrlen
    mov msglen, eax

    mov ecx, -11        ; STD_OUTPUT
    call GetStdHandle
    mov hFile, rax

    lea r9, BytesWritten
    mov r8d, msglen
    lea rdx, msg
    mov rcx, hFile
    call WriteFile

    xor ecx, ecx        ; exit code = 0
    call ExitProcess

    main ENDP

END
