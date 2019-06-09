	.file	"helloworld.c"
	.section	.rodata.str1.1,"aMS",@progbits,1
.LC0:
	.string	"Hello world, from 64-bits"
	.section	.text.startup,"ax",@progbits
	.p2align 4,,15
	.globl	main
	.type	main, @function
main:
.LFB23:
	.cfi_startproc
	leaq	.LC0(%rip), %rdi
	subq	$8, %rsp
	.cfi_def_cfa_offset 16
	call	puts@PLT
	xorl	%eax, %eax
	call	CFunction@PLT
	xorl	%eax, %eax
	addq	$8, %rsp
	.cfi_def_cfa_offset 8
	ret
	.cfi_endproc
.LFE23:
	.size	main, .-main
	.ident	"GCC: (Ubuntu 6.5.0-2ubuntu1~18.04) 6.5.0 20181026"
	.section	.note.GNU-stack,"",@progbits
