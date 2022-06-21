# Cheese
A random x86-64 assembly inspired interpreted language written in python (for now)

#commands:

jmp <arg> : jumps to line
cjp <arg> <arg> : jumps to first argument if second argument is zero
set <arg> <arg> : sets a registers value to second argument
add <arg> <arg> : adds a registers value by second argument
sub <arg> <arg> : subtracts a registers value by second argument
mul <arg> <arg> : multiplies a registers value by second argument
div <arg> <arg> : divides a registers value by second argument and discards remainder
mod <arg> <arg> : adds a registers value by second argument
psh <arg> : pushes register value onto stack
pop <arg> : pops top value of stack into register
swp <arg> : swaps registers value with top of stack
inc <arg> : increments register
dec <arg> : decrements register
clr <arg> : resets register to zero
int : interrupts program to display contents of $xf00-$xfff
~comment

#types:

d<dec> dec
x<hex> hex
b<bin> bin
%<number> value of register
<number> numerical value
@<label> label

#syntax:

<cmd> <arg> <arg>
@<label>
~<comment>

#memory:
x000-xdef : program memory
xdf0-xdff : important program statistics
xe00-xeff : user input
xf00-xfff : program output (printed with the int command)
