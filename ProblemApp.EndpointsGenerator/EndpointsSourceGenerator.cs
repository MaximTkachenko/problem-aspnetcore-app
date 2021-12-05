﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProblemApp.EndpointsGenerator
{
    [Generator]
    public class EndpointsSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var startOnlyScriptType = compilation.GetTypeByMetadataName("ProblemApp.Common.IStartOnlyScript`1");
            var scriptType = compilation.GetTypeByMetadataName("ProblemApp.Common.IScript`1");

            var startOnlyScripts = new List<ITypeSymbol>();
            var scripts = new List<ITypeSymbol>();

            var aa = compilation.GetTypeByMetadataName("ProblemApp.Common.IStartOnlyScript`DeadlockOnThreadPoolRequest");

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                startOnlyScripts.AddRange(syntaxTree.GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<ITypeSymbol>()
                    .Where(x => x.AllInterfaces.Any(i => i.OriginalDefinition == startOnlyScriptType)));

                scripts.AddRange(syntaxTree.GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(x => semanticModel.GetDeclaredSymbol(x))
                    .OfType<ITypeSymbol>()
                    .Where(x => x.AllInterfaces.Any(i => i.OriginalDefinition == scriptType)));
            }

            var code = new StringBuilder(@"
using System;
using ProblemApp.Scripts;
using Microsoft.AspNetCore.Mvc;

namespace ProblemApp.Controllers
{
    public partial class ScriptsController
    {");

            foreach (var startOnlyScript in startOnlyScripts)
            {
                code.Append($@"
        [HttpPost({startOnlyScript.Name}.Action)]
        public async Task<IActionResult> Execute{startOnlyScript.Name}Async({startOnlyScript.AllInterfaces[0].TypeArguments[0].Name} request)
        {{
            await HttpContext.RequestServices.GetRequiredService<{startOnlyScript.Name}>().StartAsync(request);
            return Ok(""Started"");
        }}");
            }

            foreach (var script in scripts)
            {
                code.Append($@"
        [HttpPost({script.Name}.Action)]
        public async Task<IActionResult> Start{script.Name}Async({script.AllInterfaces[0].TypeArguments[0].Name} request)
        {{
            var started = await HttpContext.RequestServices.GetRequiredService<{script.Name}>().StartAsync(request);
            return Ok(started ? ""Started"" : ""Already started"");
        }}
        [HttpDelete({script.Name}.Action)]
        public async Task<IActionResult> Stop{script.Name}Async()
        {{
            var stopped = await HttpContext.RequestServices.GetRequiredService<{script.Name}>().StopAsync();
            return Ok(stopped ? ""Stopped"" : ""Already stopped"");
        }}");
            }

            code.Append(@"
    }
}");
            context.AddSource("generatedSource", code.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //uncomment to debug
            //System.Diagnostics.Debugger.Launch();
        }
    }
}
