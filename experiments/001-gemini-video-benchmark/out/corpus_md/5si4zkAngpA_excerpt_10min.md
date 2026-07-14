---
title: "Exploring the CPython JIT"
source_filename: "https://www.youtube.com/watch?v=5si4zkAngpA"
duration: "00:29:42"
language: "en"
processed_at: "2026-07-14T19:36:46Z"
generator: "vectorreel-experiments/001@phase0.2"
summary: "Diego Russo introduces the CPython JIT compiler, explaining its function as a dynamic translator of hot code paths into native machine instructions. He details CPython's \"copy-and-patch\" strategy, which uses precompiled stencils and runtime patching for efficiency and portability. Russo traces the JIT's development through CPython versions 3.11-3.14, highlighting foundational changes like the adaptive interpreter and micro-operations. He then outlines the full execution pipeline from Python source code to optimized machine code, using a `sum_squares` function as a practical example."
tags: [cpython, jit, python-internals, performance, compiler, europython]
---

# Exploring the CPython JIT

## 00:00:00 Introduction to CPython JIT and Speaker

**Spoken:** Good morning everyone. I'm Diego Russo and today we're going to explore something exciting, the just-in-time compiler recently added to CPython. Now, before we start, a quick raise hand exercise. Raise your hand if you know what a JIT is. That's good. Right. Raise your hand if you knew that CPython has an integrated JIT in the runtime. That's good. Now we see the really the pro users here. Raise your hand if you ever used the CPython JIT. Oh, very few. Yes. That's expected by the way. Okay, so independently of your level of understanding and experience, I think this presentation is going to be useful in any case. So this is the official Python runtime, finally growing a JIT. And it's happening inside the interpreter we use every day. I'll walk you through how it works, what it looks like, and how we can use it. We're going to look at Python internals, including bytecodes, JIT mechanics, and even a bit of assembly and machine code. So if you're not used to that, don't worry, I'll keep things kind of on the big picture and but just you know, we're going under the hood. I've been writing Python for almost two decades and I spent the last 14 years at Arm, where I work as a principal software engineer in the runtimes team. I'm also one of the leads of the Python Guild at Arm, which is a company-wide community with over 1500 members. I've been contributing to CPython and its ecosystem for the last couple of years and focused mainly on performance and internals. In particular, I spent the last year working on the experimental JIT compiler and just a couple of months ago, in May, I became a core Python developer. Thank you.

**On screen:**
> Exploring the CPython JIT by Diego Russo 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> arm Exploring the CPython JIT EuroPython 2025
> Diego Russo https://github.com/diegorusso • Using Python for almost 20 years • 14 years at Arm Ltd • Principal Software Engineer in the Runtimes team • Python Guild with 1500 members • Contributing to CPython and its ecosystem for 2 years • Contributing to the new experimental CPython JIT for over a year • Python Core Developer since May 2024 Vote to promote Diego Russo (diegorusso) as a CPython core developer. lukasz.langa 2 days ago I would like to propose promoting Diego Russo (diegorusso) as a CPython core developer. 43 votes 100% Yes 0% Do not promote Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE

**Visual:** The video opens with a title slide for a presentation titled "Exploring the CPython JIT" by Diego Russo, featuring the EuroPython 2025 logo. A new slide appears with the Arm logo and the presentation title. The speaker, Diego Russo, begins his introduction. A slide introduces Diego Russo, detailing his experience with Python, his role at Arm, and his recent promotion to CPython core developer. A small cartoon image of a person with sunglasses is also on the slide.

## 00:02:09 Understanding JIT Compilers and Tracing

**Spoken:** So, what is a JIT compiler in the context of Python? A just-in-time compiler is a component of the runtime that identifies frequently executed portions of the code, what we call hot code paths, and compiles them into native machine instructions while the program is running. This gives us the best both the best of the both worlds. On one side, we have flexibility of the interpreter execution for most of your code, and on the other side, the performance of compiled code for the parts that matter the most. In CPython case, the JIT is a tracing JIT. It observes how the code behaves. I know that you don't see anything there. Well, I keep talking. It observes how the code behaves during the execution. It builds traces, which are linear paths through the control flow, and at the end it compiles those traces into fast native code. Um, where was I? Um, so it observes how the code behaves during the execution. It builds traces and which are linear paths through the control flow, and at the end it compiles those traces into fast native code. These traces are not general purpose functions. They are specialized paths that avoid the checks and in the interpretation. Once the JIT takes over, we completely remove the interpreter overhead for that specific path.

**On screen:**
> What's a JIT compiler? Just-In-Time compiler Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> What's a JIT compiler? Just-In-Time compiler A JIT compiler is a component of a runtime system that dynamically translates code into machine code while the program is running, rather than before execution. Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> What's a JIT compiler? Just-In-Time compiler A JIT compiler is a component of a runtime system that dynamically translates code into machine code while the program is running, rather than before execution. Detects and compiles hot code paths at runtime Uses runtime profiling information to optimize code Creates traces of frequently executed operations Eliminates interpreter overhead for performance gains Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE

**Visual:** The slide changes to display the question "What's a JIT compiler?" with the subtitle "Just-In-Time compiler." The slide now shows a definition of a JIT compiler, explaining its role in dynamically translating code to machine code for performance optimization. The slide is temporarily black, with only the conference logo visible in the corner. The slide reappears, showing the definition of a JIT compiler along with bullet points describing its functions: detecting hot code, using profiling, creating traces, and eliminating interpreter overhead.

## 00:03:58 CPython's Copy-and-Patch JIT Strategy

**Spoken:** So, let's talk about the strategy that CPython uses to implement its JIT. It's called copy-and-patch. This is quite different from the heavyweight JITs that we build where that builds and optimizes entire functions on the fly, for instance, using LLVM or some custom IRs. Instead, CPython approaches a simple and pragmatic. For every micro-operation, for example, things like loading a variable, doing an integer multiplications, or jump into the next iteration, the JIT has precompiled machine code stencils already built in. When a trace becomes hot, the JIT copies this code stencils into memory, and then it patches them with a runtime specific values. Things like constants, memory offsets, jump targets, and etcetera. These patch fragments are then linked together and executed instead of interpreting the original bytecode. There are some important benefits to these designs. It avoids the need for a full dynamic code generator, which reduces the complexity. It's portable, that means it works cleanly across platforms, including Arm and X86, and it's maintainable. It just reuses known good code paths and specialized pay trace rather than trying to compile some random Python logic.

**On screen:**
> How does a JIT compiler into? Copy-and-patch Trace hot code paths as micro-operations (µops) Copy static template code (stencils) for each µop (stencils) into executable memory Patch placeholders (jumps, constants, memory addresses) in the code Links the µop code fragments together Executes native code instead of interpreting it Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE

**Visual:** A flowchart illustrates CPython's "Copy-and-patch" JIT strategy, showing steps from tracing micro-operations to executing native code. The speaker explains the process and its benefits.

## 00:05:28 Foundational Work Leading to CPython JIT

**Spoken:** So, it's worth asking why is CPython adding a JIT now and what had to happen first? So in 3.11, CPython introduced a specialized interpreter. It runs regular bytecode, but as the program executes, it rewrites the hot bytecodes on the fly with more specialized versions. This mechanism gathers runtime data, things like types, branch outcomes, and which code is hot. The all this information are critical for a later compilation. In 3.12, a new DSL has been introduced for generating interpreter code. This helped manage the complexity, removed a lot of boilerplate code for things like stack manipulation of reference reference counting. We now have a single source of truth to generate multiple tiers of interpreters. And in 3.13, a new micro-op interpreter was added. It lowers specialized bytecode into micro-ops, which are simpler and easier to optimize. This new pipeline detects hot code and runs it more efficiently. The first implementation of the JIT is in the code base. And now in 3.14, the experimental JIT is more accessible to users. It improves micro-op performance and it adds infrastructure for better code stencil generation. So, the JIT is not just a shortcut, but it's actually the result of many years of foundational work to modernize the CPython execution pipeline.

**On screen:**
> How did we get here? PEP659: Specializing adaptive interpreter profiles and optimizes programs on-the-fly 3.11 Interpreter generator allows analysis and modification from a DSL specification 3.12 Second "micro-op" interpreter detects, optimizes, and executes "HOT" code JIT disabled at build time 3.13 Improved performance and better way to generate stencils JIT disabled at run time (still experimental) 3.14 Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE

**Visual:** A timeline slide shows the evolution of CPython's interpreter and JIT features across versions 3.11, 3.12, 3.13, and 3.14, highlighting key changes that enabled the JIT.

## 00:07:13 CPython 3.14 JIT: From Source to Machine Code

**Spoken:** So let's go more in details on how the JIT works in 3.14. So we're going to see the high-level execution pipeline in 3.14 from the Python source code all the way down to machine code. It starts as always with your Python source code. The CPython internal compiler parses it into an abstract syntax tree. It builds a control flow graph and it applies some basic optimization. Eventually it emits bytecode. That bytecode is then executed by the interpreter. While running, it collects profiling information, what types are used, which bytecodes are hot, and how values flow through the code. Based on that, the interpreter starts rewriting byte code structures with more efficient specialized versions. The all the ones that assume specific types or behavior. Then, when a loop becomes hot enough, CPython lowers the specialized bytecode into micro-operations. These are low-level fixed-purpose instructions designed for easier optimization. The next step is to take these traces of micro-ops and apply further simplifications to produce an optimized micro-op trace. And finally, the optimized trace is passed to the JIT, which uses the copy-and-patch technique. It assembles native machine code functions by copying code snippets for each micro-op and patching with the runtime details. That native trace now completely bypasses the interpreter for this part of the program. It's faster, it's and it's running real hardware instructions.

**On screen:**
> The CPython JIT in 3.14 Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> The CPython JIT in 3.14 The journey: from source code to machine code. Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> The CPython JIT in 3.14 The journey: from source code to machine code. Python application source code Compiler Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> The CPython JIT in 3.14 The journey: from source code to machine code. Python application source code Compiler Bytecode Adaptive Interpreter Specialized bytecode Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> The CPython JIT in 3.14 The journey: from source code to machine code. Python application source code Compiler Bytecode Adaptive Interpreter Specialized bytecode µop traces Optimized µop traces Native machine code JIT Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE

**Visual:** The slide changes to a new section title: "The CPython JIT in 3.14." The slide adds a subtitle: "The journey: from source code to machine code." A diagram begins to build, showing "Python application source code" flowing into a "Compiler." The diagram expands to show the compiler outputting "Bytecode," which is then processed by an "Adaptive Interpreter" that can generate "Specialized bytecode." The diagram further expands, showing specialized bytecode being converted into "µop traces," then "Optimized µop traces," and finally into "Native machine code" by the "JIT."

## 00:09:01 Illustrative Example: `sum_squares`

**Spoken:** Now we see we now that we've seen the big picture, let's follow a concrete example through the entire execution pipeline. We have a function here that we're going to follow in details, it's called sum_squares, it takes n as an input, is a very simple loop, there is a total is initialized, it iterates over a range of n and it adds i times i to the total and then finally it returns the result. So despite being simple, this function contains all the right ingredients to demonstrate the CPython JIT pipeline. A loop which can become hot, the integer operations that can be specialized, and the structure that can be easily lowered into micro-ops and optimized. Let's focus only on the for loop. And this is the respective bytecode for the cycle. Before

**On screen:**
> The CPython JIT in 3.14 Python application source code def sum_squares(n): total = 0 for i in range(n): total += i * i return total Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE
> The CPython JIT in 3.14 Bytecode def sum_squares(n): total = 0 for i in range(n): total += i * i return total FOR_ITER STORE_FAST LOAD_FAST BINARY_OP STORE_FAST JUMP_BACKWARD for i in range(n): total, i total += i * i Public 2025 Arm 2025 EUROPYTHON PRAGUE :: 14 - 20 July & REMOTE

**Visual:** The slide displays a Python code snippet for a `sum_squares` function, which calculates the sum of squares up to `n`. The speaker explains this example will be used to demonstrate the JIT pipeline. The slide shows the `sum_squares` Python code again, but now highlights the `for` loop and displays its corresponding bytecode instructions on the right.

---

## Source & licence

Generated by **vectorreel** from a Creative Commons video. The original remains the property of its authors.

> "Exploring the CPython JIT — Diego Russo" by EuroPython Conference (https://www.youtube.com/watch?v=5si4zkAngpA), CC BY 3.0
>
> Original video: https://www.youtube.com/watch?v=5si4zkAngpA
> Licence: Creative Commons Attribution (CC BY) — https://creativecommons.org/licenses/by/3.0/
