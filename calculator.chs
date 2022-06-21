cjp x1 %xE00
set xA0 xE00
@cvt_loop
cjp @end_cvt_loop %%xA0
sub %xA0 x30
mul x0 d10
add x0 %%xA0
clr %xA0
inc xA0
jmp @cvt_loop
@end_cvt_loop
cjp @end_cvt_loop %xE00
sub xE00 x2A
set x2 %xE00
clr xE00
@after_operator
cjp @after_operator %xE00
set xA0 xE00
@cvt_loop2
cjp @calculate %%xA0
sub %xA0 x30
mul x1 d10
add x1 %%xA0
clr %xA0
inc xA0
jmp @cvt_loop2
@calculate
cjp @mul %x2
sub x2 x1
cjp @add %x2
sub x2 x2
cjp @sub %x2
jmp @div
@add
add x0 %x1
jmp @print
@sub
sub x0 %x1
jmp @print
@mul
mul x0 %x1
jmp @print
@div
div x0 %x1
@print
set xF09 %xF08
set xF08 %xF07
set xF07 %xF06
set xF06 %xF05
set xF05 %xF04
set xF04 %xF03
set xF03 %xF02
set xF02 %xF01
set xF01 %xF00
set xF00 %x0
mod xF00 d10
add xF00 x30
div x0 d10
cjp @end %x0
jmp @print
@end
int
set x0 xFFF
@clp
cjp x1 %x0
clr %x0
dec x0
jmp @clp