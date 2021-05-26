﻿/*
Copyright (C) 2020 DeathCradle

This file is part of Open Terraria API v3 (OTAPI)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using ModFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModFramework.Modules
{
    public delegate HookResult AssemblyFoundHandler(string filepath);

    [MonoMod.MonoModIgnore]
    public class CSharpLoader
    {
        const string ConsolePrefix = "CSharp";
        const string ModulePrefix = "CSharpScript_";

        public MonoMod.MonoModder Modder { get; set; }

        public static event AssemblyFoundHandler AssemblyFound;
        public static List<string> GlobalAssemblies { get; } = new List<string>();

        public CSharpLoader SetModder(MonoMod.MonoModder modder)
        {
            Modder = modder;

            //Console.WriteLine($"[{ConsolePrefix}] Starting script mod runtime");

            modder.OnReadMod += (m, module) =>
            {
                if (module.Assembly.Name.Name.StartsWith(ModulePrefix))
                {
                    // remove the top level program class
                    var tlc = module.GetType("<Program>$");
                    if (tlc != null)
                    {
                        module.Types.Remove(tlc);
                    }
                    Modder.RelinkAssembly(module);
                }
            };

            //RunModules();
            return this;
        }

        //public CSharpLoader()
        //{
        //    Console.WriteLine($"[{ConsolePrefix}] Starting script runtime");
        //    //RunModules();
        //}

        IEnumerable<MetadataReference> LoadExternalRefs(string path)
        {
            var refs_path = Path.Combine(path, "Metadata.refs");

            if (File.Exists(refs_path))
            {
                var refs = File.ReadLines(refs_path);

                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

                foreach (var ref_file in refs)
                {
                    var sys_path = Path.Combine(assemblyPath, ref_file);

                    if (File.Exists(ref_file))
                        yield return MetadataReference.CreateFromFile(ref_file);

                    else if (File.Exists(sys_path))
                        yield return MetadataReference.CreateFromFile(sys_path);

                    else throw new Exception($"Unable to resolve external reference: {ref_file} (Metadata.refs) in dir {Environment.CurrentDirectory}");
                }
            }
        }

        public void RunModules()
        {
            var path = Path.Combine("csharp", "modifications");
            var outDir = Path.Combine("csharp", "generated");
            if (Directory.Exists(path))
            {
                if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
                Directory.CreateDirectory(outDir);

                const string constants_path = "../../../../OTAPI.Setup/bin/Debug/net5.0/AutoGenerated.cs";
                var constants = File.Exists(constants_path) ? File.ReadAllText(constants_path) : ""; // bring across the generated constants

                foreach (var file in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
                {
                    try
                    {
                        if (AssemblyFound?.Invoke(file) == HookResult.Cancel)
                            continue; // event was cancelled, they do not wish to use this file. skip to the next.

                        Console.WriteLine($"[{ConsolePrefix}] Loading module: {file}");

                        var assemblyPath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

                        var folder = Path.GetFileName(Path.GetDirectoryName(file));
                        var is_top_level_script = folder.Equals("Modifications", StringComparison.CurrentCultureIgnoreCase);

                        var encoding = System.Text.Encoding.UTF8;
                        var options = CSharpParseOptions.Default
                            .WithKind(SourceCodeKind.Regular)
                            .WithLanguageVersion(LanguageVersion.Preview); // allows toplevel functions

                        SyntaxTree encoded;
                        SourceText source;
                        //using (var stream = File.OpenRead(file))
                        {
                            var src = File.ReadAllText(file);
                            source = SourceText.From($"{constants}\n{src}", encoding);
                            encoded = CSharpSyntaxTree.ParseText(source, options, file);
                        }

                        using var dllStream = new MemoryStream();
                        using var pdbStream = new MemoryStream();
                        using var xmlStream = new MemoryStream();

                        var assemblyName = ModulePrefix + Path.GetFileNameWithoutExtension(file);

                        var outAsmPath = Path.Combine(outDir, $"{assemblyName}.dll");
                        var outPdbPath = Path.Combine(outDir, $"{assemblyName}.pdb");

                        var refs = LoadExternalRefs(path).ToList();

                        foreach(var globalPath in GlobalAssemblies)
                        {
                            refs.Add(MetadataReference.CreateFromFile(globalPath));
                        }

                        var compile_options = new CSharpCompilationOptions(
                            is_top_level_script ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary)
                                .WithOptimizationLevel(OptimizationLevel.Debug)
                                .WithPlatform(Platform.AnyCpu);

                        var compilation = CSharpCompilation
                            .Create(assemblyName, new[] { encoded }, options: compile_options)
                            .AddReferences(
                                MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
                                //MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),

                                MetadataReference.CreateFromFile(typeof(ModType).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(Mono.Cecil.AssemblyDefinition).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(Mono.Cecil.Rocks.ILParser).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(MonoMod.MonoModder).Assembly.Location)
                            )
                            .AddReferences(refs)
                            .AddReferences(typeof(MonoMod.MonoModder).Assembly.GetReferencedAssemblies()
                                        .Select(asm => MetadataReference.CreateFromFile(Assembly.Load(asm).Location)))
                            .AddReferences(typeof(ModType).Assembly.GetReferencedAssemblies()
                                        .Select(asm => MetadataReference.CreateFromFile(Assembly.Load(asm).Location)))
                        ;

                        var emitOptions = new EmitOptions(
                            debugInformationFormat: DebugInformationFormat.PortablePdb,
                            pdbFilePath: outPdbPath
                        );

                        var embeddedTexts = new List<EmbeddedText>
                        {
                            EmbeddedText.FromSource(file, source),
                        };

                        var attempt = 0;

                    COMPILE:
                        var compilationResult = compilation.Emit(
                              peStream: dllStream,
                              pdbStream: pdbStream,
                              embeddedTexts: embeddedTexts,
                              options: emitOptions
                        );

                        if (!compilationResult.Success && attempt++ < 1)
                        {
                            compilation = compilation.WithOptions(
                                compile_options.WithOutputKind(
                                    !is_top_level_script ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary
                                )
                            );
                            goto COMPILE;
                        }

                        if (compilationResult.Success)
                        {
                            // save the file for monomod (doesnt like streams it seems?)
                            // then register the reflected assembly, then the monomod variant for patches

                            dllStream.Seek(0, SeekOrigin.Begin);
                            pdbStream.Seek(0, SeekOrigin.Begin);

                            var asm = PluginLoader.AssemblyLoader.Load(dllStream, pdbStream);
                            PluginLoader.AddAssembly(asm);

                            File.WriteAllBytes(outAsmPath, dllStream.ToArray());
                            File.WriteAllBytes(outPdbPath, pdbStream.ToArray());

                            if (Modder != null)
                                Modder.ReadMod(outAsmPath);
                            else Modifier.Apply(ModType.Runtime, null, new[] { asm }); // relay on the runtime hook
                        }
                        else
                        {
                            //Console.WriteLine($"Compilation errors for file: {Path.GetFileName(file)}");

                            foreach (var diagnostic in compilationResult.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error))
                            {
                                Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                            }

                            throw new Exception($"Compilation errors above for file: {Path.GetFileName(file)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{ConsolePrefix}] Load error: {ex}");
                        throw;
                    }
                }
            }
        }
    }
}
