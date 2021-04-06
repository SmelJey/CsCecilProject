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
            Console.WriteLine(assembly.FullName);

            foreach (ModuleDefinition module in assembly.Modules) {
                foreach (TypeDefinition type in module.Types) {
                    foreach (MethodDefinition method in type.Methods) {
                        ProcessMethod(method);
                    }
                }
            }

            assembly.Write(outputPath);
        }

        private static void ProcessMethod(MethodDefinition method) {
            ILProcessor ilProcessor = method.Body.GetILProcessor();
            for (int i = 0; i < method.Body.Instructions.Count; i++) {
                var instruction = method.Body.Instructions[i];
                if (instruction.OpCode == OpCodes.Add) {
                    ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Sub));
                } else if (instruction.OpCode == OpCodes.Add_Ovf) {
                    ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Sub_Ovf));
                } else if (instruction.OpCode == OpCodes.Add_Ovf_Un) {
                    ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Sub_Ovf_Un));
                } else if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference methodReference) {
                    // TODO: decimal add

                }
            }
        }
    }
}
