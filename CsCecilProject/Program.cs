using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CsCecilProject {
    class Program {
        static void Main(string[] args) {
            if (args.Length < 2) {
                throw new ArgumentException("Not enough arguments");
            }

            string inputPath = args[0];
            string outputPath = args[1];

            var assembly = AssemblyDefinition.ReadAssembly(inputPath);
            Console.WriteLine($"Input assembly: {assembly.FullName}");
            int replacementsCnt = 0;

            foreach (ModuleDefinition module in assembly.Modules) {
                foreach (TypeDefinition type in module.Types) {
                    foreach (MethodDefinition method in type.Methods) {
                        replacementsCnt += ProcessMethod(method, module);
                    }
                }
            }

            assembly.Write(outputPath);
            Console.WriteLine($"{replacementsCnt} replacements done");
            Console.WriteLine($"Output assembly was written to {outputPath}");
        }

        private static int ProcessMethod(MethodDefinition method, ModuleDefinition module) {
            ILProcessor ilProcessor = method.Body.GetILProcessor();
            int replacementsCnt = 0;

            for (int i = 0; i < method.Body.Instructions.Count; i++) {
                var instruction = method.Body.Instructions[i];
                Instruction replacementInstruction = null;

                if (instruction.OpCode == OpCodes.Add) {
                    replacementInstruction = Instruction.Create(OpCodes.Sub);

                } else if (instruction.OpCode == OpCodes.Add_Ovf) {
                    replacementInstruction = Instruction.Create(OpCodes.Sub_Ovf);

                } else if (instruction.OpCode == OpCodes.Add_Ovf_Un) {
                    replacementInstruction = Instruction.Create(OpCodes.Sub_Ovf_Un);

                } else if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference methodReference) {
                    var decimalAdd = module.ImportReference(typeof(decimal).GetMethod("op_Addition"));
                    var decimalSub = module.ImportReference(typeof(decimal).GetMethod("op_Subtraction"));

                    if (methodReference.FullName == decimalAdd.FullName) {
                        replacementInstruction = Instruction.Create(OpCodes.Call, decimalSub);
                    }
                }

                if (replacementInstruction != null) {
                    ilProcessor.Replace(instruction, replacementInstruction);
                    replacementsCnt++;
                }
            }

            return replacementsCnt;
        }
    }
}
