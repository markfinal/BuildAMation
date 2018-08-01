	.section	__TEXT,__text,regular,pure_instructions
	.macosx_version_min 10, 6
	.globl	_main                   ## -- Begin function main
	.p2align	4, 0x90
_main:                                  ## @main
	.cfi_startproc
## BB#0:
	pushl	%ebp
Lcfi0:
	.cfi_def_cfa_offset 8
Lcfi1:
	.cfi_offset %ebp, -8
	movl	%esp, %ebp
Lcfi2:
	.cfi_def_cfa_register %ebp
	subl	$8, %esp
	calll	L0$pb
L0$pb:
	popl	%eax
	leal	L_str-L0$pb(%eax), %eax
	movl	%eax, (%esp)
	calll	_puts
	calll	_CFunction
	xorl	%eax, %eax
	addl	$8, %esp
	popl	%ebp
	retl
	.cfi_endproc
                                        ## -- End function
	.section	__TEXT,__cstring,cstring_literals
	.p2align	4               ## @str
L_str:
	.asciz	"Hello world, from 32-bits"


.subsections_via_symbols
