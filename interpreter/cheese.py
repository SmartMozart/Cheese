import sys, threading, os

# define stack, memory, line counter, labels, valid commands, and types
debug = len(sys.argv) > 2
line = 0
finished = False
stack = []
memory = [0 for _ in range(4096)]
labels = {}
cmds = {'jmp':1,'set':2,'add':2,'sub':2,'mul':2,'div':2,'mod':2,'psh':1,'pop':1,'swp':1,'inc':1,'dec':1,'clr':1,'cjp':2,'int':0,'pnt':1}
types = {'x':16,'b':2,'d':10}


# error handler
def error(message, typerr=0):
	err = 'ERROR'
	if typerr:
		err = 'WARN'
	print(f'[{err}]: {message} (Line {line+1})')
	if not typerr:
		input()
		sys.exit()


# command interpreter
def interpret_cmd(tokens):
	global line, stack, memory
	# interpret token values
	cmd = tokens[0]
	tokens = tokens[1:]

	if len(tokens) != cmds[cmd]:
		error('Argument error')

	itokens = []
	for token in tokens:
		if token[0] == '@':
			try:
				itokens.append(labels[token[1:]])
			except:
				error(f'Unknown label "{token[1:]}"')
			continue
		tk = token.replace('%','')
		if tk[0] not in types:
			error(f'Unknown type "{tk[0]}"')
		try:
			value = int(tk[1:],types[tk[0]])
		except:
			error(f'Integer parse error "{tk}"')
		if token.startswith('%'):
			try:
				value = memory[value]
				if token.startswith('%%'):
					value = memory[value]
			except:
				error(f'Memory address "{value}" does not exist')
		itokens.append(value)
	tokens = itokens


	# interpret the command
	try:
		if cmd == 'jmp':
			line = tokens[0]-1
			return
		if cmd == 'set':
			memory[tokens[0]] = tokens[1]
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'add':
			memory[tokens[0]] += tokens[1]
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'sub':
			memory[tokens[0]] -= tokens[1]
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'mul':
			memory[tokens[0]] *= tokens[1]
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'div':
			try:
				memory[tokens[0]] //= tokens[1]
			except:
				error('Divide by zero')
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'mod':
			try:
				memory[tokens[0]] %= tokens[1]
			except:
				error('Divide by zero')
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'psh':
			stack.append(tokens[0])
			if len(stack) > 256:
				error('Stack overflow')
			return

		if cmd == 'pop':
			a = memory[tokens[0]]
			try:
				memory[tokens[0]] = stack[-1]
				stack = stack[:-1]
			except:
				error('Stack is empty')
			return

		if cmd == 'swp':
			a = memory[tokens[0]]
			try:
				memory[tokens[0]], stack[-1] = stack[-1], memory[tokens[0]]
			except:
				error('Stack is empty')
			return

		if cmd == 'inc':
			memory[tokens[0]] += 1
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'dec':
			memory[tokens[0]] -= 1
			memory[tokens[0]] %= 4294967296
			return

		if cmd == 'clr':
			memory[tokens[0]] = 0
			return

		if cmd == 'cjp':
			if not tokens[1]:
				line = tokens[0]-1
			return


		if cmd == 'int':
			intret = ''
			for value in memory[3840:]:
				if value == 0:
					break
				try:
					intret += chr(value)
				except:
					error(f'Invalid character "{value}"')
			print(intret)

		if cmd == 'pnt':
			print(tokens[0])
			return

	except:
		error(f'Memory address "{tokens[0]}" does not exist')


# user input loop
def uiloop():
	global memory
	while True:
		ui = input()
		for index, char in enumerate(ui):
			if index > 255:
				error('Input exceeds address xEFF')
			memory[index+3584] = ord(char)


# read main file and remove newline characters
with open(sys.argv[1], 'r') as main:
	main = [line.replace('\n','') for line in main.readlines()]


# find labels and put them in the label dictionary
for ln, contents in enumerate(main):
	if contents.startswith('@'):
		label = contents[1:].split(' ')[0]
		if label in labels.keys():
			error(f'Label "{label}" overrides existing label',1)
		labels[label] = ln+1

# start the ui loop in a new thread
threading.Thread(target=uiloop,daemon=True).start()


# main interpreter
while line < len(main):
	lc = main[line]
	if debug: print(lc)
	if lc == '' or lc.startswith('@') or lc.startswith('~'):
		line+=1
		memory[3568] = line
		continue
	tokens = lc.split(' ')
	if tokens[0] not in cmds:
		error(f'Unknown command "{tokens[0]}"')
	interpret_cmd(tokens)
	line += 1
	memory[3568] = line


if debug: print(stack, memory)
print('[SUCCESS]: Program finished!')
input()


#commands:

# jmp <arg> : jumps to line
# cjp <arg> <arg> : jumps to first argument if second argument is zero
# set <arg> <arg> : sets a registers value to second argument
# add <arg> <arg> : adds a registers value by second argument
# sub <arg> <arg> : subtracts a registers value by second argument
# mul <arg> <arg> : multiplies a registers value by second argument
# div <arg> <arg> : divides a registers value by second argument and discards remainder
# mod <arg> <arg> : adds a registers value by second argument
# psh <arg> : pushes register value onto stack
# pop <arg> : pops top value of stack into register
# swp <arg> : swaps registers value with top of stack
# inc <arg> : increments register
# dec <arg> : decrements register
# clr <arg> : resets register to zero
# int : interrupts program to display contents of xF00-xFFF
# ~comment

#types:

# d<dec> dec
# x<hex> hex
# b<bin> bin
# %<number> value of register
# <number> numerical value
# @<label> label

#syntax:

# <cmd> <arg> <arg>
# @<label>
# ~<comment>

# memory:

#x000-xDEF : program memory
#xDF0-xDFF : important program statistics
#xE00-xEFF : user input
#xF00-xFFF : program output (printed with the int command)
