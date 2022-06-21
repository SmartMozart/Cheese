# Cheese
A random x86-64 assembly inspired interpreted language written in python (for now)

## Commands:

- jmp : jumps to line
- cjp : jumps to first argument if second argument is zero
- set : sets a registers value to second argument
- add : adds a registers value by second argument
- sub : subtracts a registers value by second argument
- mul : multiplies a registers value by second argument
- div : divides a registers value by second argument and discards remainder
- mod : adds a registers value by second argument
- psh : pushes register value onto stack
- pop : pops top value of stack into register
- swp : swaps registers value with top of stack
- inc : increments register
- dec : decrements register
- clr : resets register to zero
- int : interrupts program to display contents of $xf00-$xfff
- ~comment

## Types:

- d dec
- x hex
- b bin
- %[integer] returns value of register
- @ label

## Syntax:

- [cmd] [arg(s)]

## Memory:
- x000-xDEF : program memory
- xDF0-xDFF : important program statistics
- xE00-xEFF : user input
- xF00-xFFF : program output (printed with the int command)

I'm pretty bad with md so forgive my bad formatting
