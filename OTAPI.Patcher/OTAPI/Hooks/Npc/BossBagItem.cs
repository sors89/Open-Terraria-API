﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using NDesk.Options;
using OTAPI.Patcher.Extensions;
using OTAPI.Patcher.Modifications.Helpers;
using System;
using System.Linq;

namespace OTAPI.Patcher.Modifications.Hooks.Npc
{
    /// <summary>
    /// In this patch we will replace the server code to use a custom Item.NewItem method named "BossBagItem".
    /// We then insert some IL to check the result variable for the int -1. If they match
    /// then we cancel the vanilla function by returning.
    /// </summary>
    public class BossBagItem : OTAPIModification<OTAPIContext>
    {
        public override bool IsAvailable(OptionSet options) => this.IsServer();

        public override void Run(OptionSet options)
        {
            Console.Write("Hooking Npc.DropBossBag\\Item...");

            var vanilla = this.Context.Terraria.Types.Npc.Method("DropBossBags");
            var callback = this.Context.OTAPI.Types.Npc.Method("BossBagItem");

            var il = vanilla.Body.GetILProcessor();

            //Grad the NewItem calls
            var instructions = vanilla.Body.Instructions.Where(x => x.OpCode == OpCodes.Call
                                                                && x.Operand is MethodReference
                                                                && (x.Operand as MethodReference).Name == "NewItem"
                                                                && x.Next.OpCode == OpCodes.Stloc_1);
            //Quick validation check
            if (instructions.Count() != 1) throw new NotSupportedException("Only one server NewItem call expected in DropBossBags.");

            //The first call is in the server block. TODO: client version
            var ins = instructions.First();

            //Swap the NewItem call to our custom item call
            ins.Operand = vanilla.Module.Import(callback);
            //Our argument appends the NPC instance (this) to the arguments
            il.InsertBefore(ins, il.Create(OpCodes.Ldarg_0)); //Instance methods ldarg.0 is the instance object

            //Now we start inserting our own if block to compare the call result.
            var target = ins.Next/*stloc.1*/.Next; //Grabs a reference to the instruction after the stloc.1 opcode so we can insert sequentially
            il.InsertBefore(target, il.Create(OpCodes.Ldloc_1)); //Load the num2 variable onto the stack
            il.InsertBefore(target, il.Create(OpCodes.Ldc_I4_M1)); //Load -1 onto the stack
            il.InsertBefore(target, il.Create(OpCodes.Ceq)); //Consume & compare the two variables and push 1 (true) or 0 (false) onto the stack
            il.InsertBefore(target, il.Create(OpCodes.Brfalse_S, target)); //if the output of ceq is 0 (false) then continue back on with the [target] instruction. In code terms, if the expression is not -1 then don't exit
            il.InsertBefore(target, il.Create(OpCodes.Ret)); //If we are here, the num2 variable is equal to -1, so we can exit the function.

            Console.WriteLine("Done");
        }
    }
}