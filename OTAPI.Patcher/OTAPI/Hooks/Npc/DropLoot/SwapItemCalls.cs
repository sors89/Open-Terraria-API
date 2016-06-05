﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using NDesk.Options;
using OTAPI.Patcher.Extensions;
using OTAPI.Patcher.Modifications.Helpers;
using System;
using System.Linq;

namespace OTAPI.Patcher.Modifications.Hooks.Npc
{
    public class DropLoot_1_SwapItemCalls : OTAPIModification<OTAPIContext>
    {
        public override void Run(OptionSet options)
        {
            Console.Write("Hooking Npc.NPCLoot\\NewItem...");
            
            var npcLoot = this.Context.Terraria.Types.Npc.Method("NPCLoot");
            var dropLoot = this.Context.Terraria.Types.Npc.Method("DropLoot");

            //In this patch we swap all Item.NewItem calls in NPCLoot to use our previously
            //created DropLoot method

            //Gather all Item.NewItem calls
            var itemCalls = npcLoot.Body.Instructions.Where(x => x.OpCode == OpCodes.Call
                                && x.Operand is MethodReference
                                && (x.Operand as MethodReference).Name == "NewItem"
                                && (x.Operand as MethodReference).DeclaringType.Name == "Item").ToArray();
            
            //Swap each Item.NewItem calls to our Npc.DropLoot method.
            foreach (var call in itemCalls)
                call.Operand = dropLoot;

            Console.WriteLine("Done");
        }
    }
}
