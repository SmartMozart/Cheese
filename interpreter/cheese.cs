using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

class Interpreter {
	public int index = 0;
	public int[] memory = new int[4096];
	public List<int> stack = new List<int>();
	public Dictionary<string, int> cmds = new Dictionary<string, int>() {{"jmp",1},{"cjp",2},{"set",2},{"add",2},{"sub",2},{"mul",2},{"div",2},{"mod",2},{"psh",1},{"pop",1},{"swp",1},{"inc",1},{"dec",1},{"clr",1},{"int", 0}};
	public Dictionary<char, int> types = new Dictionary<char, int>() {{'x',16},{'b',2},{'d',10}};

	public static void Main(string[] args) {
		if (args.Length == 0 || !File.Exists(args[0])) {
			Environment.Exit(0);
		}
		string[] lines = File.ReadAllLines(args[0]);
		new Interpreter().Start(lines);
	}

	public void Error(string err) {
		Console.WriteLine("[ERROR]: {0} (Line {1})", err, index);
		Environment.Exit(0);
	}

	public void Warn(string warn) {
		Console.WriteLine("[WARN]: {0} (Line {1})", warn, index);
	}

	public void Start(string[] lines) {
		for (int i = 0; i < 4096; i++) {
			memory[i] = 0;
		}
		Thread ioloop = new Thread(IOLoop);
		ioloop.IsBackground = true;
		ioloop.Start();
		Dictionary<string, int> labels = FindLabels(lines);
		List<List<string>> tokens = SplitTokens(lines);
		Interpret(tokens, labels);
	}

	public List<List<string>> SplitTokens(string[] lines) {
		List<List<string>> tokens = new List<List<string>>();
		foreach (string line in lines) {
			List<string> toadd = new List<string>();
			if (line == "" || line[0] == '~' || line[0] == '@') {
				toadd.Add(line);
				tokens.Add(toadd);
				continue;
			}
			toadd = line.Split(' ').ToList();
			tokens.Add(toadd);
		}
		return tokens;
	}

	public void IOLoop() {
		while (true) {
			string input = Console.ReadLine();
			if (input.Length > 255) {
				Error("Input exceeds address xEFF");
			}
			for (int i = 0; i < input.Length; i++) {
				memory[i+3584] = (int)input[i];
			}
		}
	}

	public Dictionary<string, int> FindLabels(string[] lines) {
		int lIndex = 0;
		Dictionary<string, int> labels = new Dictionary<string, int>();
		foreach (string lc in lines) {
			if (lc == "") {
				lIndex++;
				continue;
			}
			if (lc[0] == '@') {
				string label = lc.Substring(1);
				if (lc.Contains(" ")) {
					Error("Labels cannot contain spaces");
				}
				if (labels.Keys.Contains(label)) {
					Warn("Label \""+label+"\" overrides existing label");
					labels[label] = lIndex;
				} else {
					labels.Add(label, lIndex);
				}
			}
			lIndex++;
		}
		return labels;
	}

	public List<int> EvalTokens(List<string> tokens, Dictionary<string, int> labels) {
		List<int> rettokens = new List<int>();
		foreach (string token in tokens) {
			if (token[0] == '@') {
				string label = token.Substring(1);
				if (!labels.ContainsKey(label)) {
					Error("Unknown label \""+label+"\"");
				}
				rettokens.Add(labels[label]);
				continue;
			}
			int perC = token.Count(s => s == '%');
			char type = token[perC];
			string val = token.Substring(perC+1);
			int cval = 0;
			if (!types.ContainsKey(type)) {
				Error("Unknown type \""+type+"\"");
			}
			try {
				cval = Convert.ToInt32(val,types[type]);
			} catch {
				Error("Integer parse error \""+token.Substring(perC)+"\"");
			}
			for (int i = 0; i < perC; i++) {
				try {
					cval = memory[cval]; 
				} catch {
					Error("Memory address \""+cval+"\" does not exist");
				}
			}
			rettokens.Add(cval);
		}
		return rettokens;
	}

	public void EvalCommand(string cmd, List<int> tokens) {
		if (cmd == "jmp") {
			index = tokens[0]-1;
		}
		if (cmd == "cjp") {
			if (tokens[1] == 0) {
				index = tokens[0]-1;
			}
		}
		if (cmd == "set") {
			memory[tokens[0]] = tokens[1];
		}
		if (cmd == "add") {
			memory[tokens[0]] += tokens[1];
		}
		if (cmd == "sub") {
			memory[tokens[0]] -= tokens[1];
		}
		if (cmd == "mul") {
			memory[tokens[0]] *= tokens[1];
		}
		if (cmd == "div") {
			try {
				memory[tokens[0]] = (int)memory[tokens[0]]/tokens[1];
			} catch {
				Error("Divide by zero");
			}
		}
		if (cmd == "mod") {
			try {
				memory[tokens[0]] %= tokens[1];
			} catch {
				Error("Divide by zero");
			}
		}

		if (cmd == "psh") {
			stack.Add(tokens[0]);
			if (stack.Count > 256) {
				Error("Stack overflow");
			}
		}
		if (cmd == "pop") {
			try {
				memory[tokens[0]] = stack[stack.Count-1];
				stack = stack.GetRange(0,stack.Count-1);
			} catch {
				Error("Stack is empty");
			}
		}
		if (cmd == "swp") {
			try {
				int tmp = stack[stack.Count-1];
				stack[stack.Count-1] = memory[tokens[0]];
				memory[tokens[0]] = tmp;
			} catch {
				Error("Stack is empty");
			}
		}
		if (cmd == "inc") {
			memory[tokens[0]] += 1;
		}
		if (cmd == "dec") {
			memory[tokens[0]] -= 1;
		}
		if (cmd == "clr") {
			memory[tokens[0]] = 0;
		}
		if (cmd == "int") {
			string intret = "";
			foreach (int val in new ArraySegment<int>(memory, 3840, 256)) {
				if (val == 0) {
					break;
				}
				try {
					intret += (char)val;
				}
				catch {
					Error("Invalid character \""+val+"\"");
				}
			}
			Console.WriteLine(intret);
			return;
		}
		memory[tokens[0]] = (int)(memory[tokens[0]]%4294967296);
	}

	public void Interpret(List<List<string>> lines, Dictionary<string, int> labels) {
		while (index < lines.Count) {
			List<string> tokens = lines[index];
			if (tokens[0] == "" || tokens[0][0] == '~' || tokens[0][0] == '@') {
				index++;
				continue;
			}
			string cmd = tokens[0];
			tokens = tokens.GetRange(1, tokens.Count-1);
			if (!cmds.ContainsKey(cmd)) {
				Error("Unknown command \""+cmd+"\"");
			}
			if (tokens.Count != cmds[cmd]) {
				Error("Argument error");
			}
			List<int> itokens = EvalTokens(tokens, labels);
			EvalCommand(cmd, itokens);
			index++;
		}
	}
}

//commands:

// jmp <arg> : jumps to line
// cjp <arg> <arg> : jumps to first argument if second argument is zero
// set <arg> <arg> : sets a registers value to second argument
// add <arg> <arg> : adds a registers value by second argument
// sub <arg> <arg> : subtracts a registers value by second argument
// mul <arg> <arg> : multiplies a registers value by second argument
// div <arg> <arg> : divides a registers value by second argument and discards remainder
// mod <arg> <arg> : adds a registers value by second argument
// psh <arg> : pushes register value onto stack
// pop <arg> : pops top value of stack into register
// swp <arg> : swaps registers value with top of stack
// inc <arg> : increments register
// dec <arg> : decrements register
// clr <arg> : resets register to zero
// int : interrupts program to display contents of xF00-xFFF
// ~comment

//types:

// d<dec> dec
// x<hex> hex
// b<bin> bin
// %<number> value of register
// <number> numerical value
// @<label> label

//syntax:

// <cmd> <arg> <arg>
// @<label>
// ~<comment>

//memory:

// x000-xDEF : program memory
// xDF0-xDFF : important program statistics
// xE00-xEFF : user input
// xF00-xFFF : program output (printed with the int command)