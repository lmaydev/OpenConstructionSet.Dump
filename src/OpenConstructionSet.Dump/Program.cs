using Microsoft.Extensions.DependencyInjection;
using OpenConstructionSet;
using OpenConstructionSet.Data;
using OpenConstructionSet.Installations;
using OpenConstructionSet.Mods;
using OpenConstructionSet.Mods.Context;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;

var outputFileOption = new Option<FileInfo>(aliases: ["--output-file", "-o"], description: "Specify an output file.");

var noStdOption = new Option<bool>(aliases: ["--no-stdout", "-q"], description: "Suppress output to stdout.");

var noGameFilesOption = new Option<bool>(["--no-game-files", "-G"], "Prevent loading of base game data files.");

var installationOption = new Option<InstallationType>(
    aliases: ["--installation", "-i"],
    description: $"Installation to use. You can provide multiple values",
    getDefaultValue: () => InstallationType.Any);

var rootCommand = new RootCommand("OpenConstructionSet Dump - output Kenshi game data to JSON")
{
    noStdOption,
    noGameFilesOption,
    installationOption,
    outputFileOption,
};

rootCommand.SetHandler(HandleAsync, noStdOption, noGameFilesOption, outputFileOption, installationOption);

var commandLineBuilder = new CommandLineBuilder(rootCommand);

var parser = commandLineBuilder.AddMiddleware(ExceptionHandler).UseDefaults().Build();

await parser.InvokeAsync(args);

async Task ExceptionHandler(InvocationContext context, Func<InvocationContext, Task> next)
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        context.ExitCode = 1;
    }
}

async Task HandleAsync(bool noStdOut, bool noGameFiles, FileInfo? outputFile, InstallationType installationType)
{
    if (noStdOut && outputFile is null)
    {
        Console.Error.WriteLine("Invalid options: If --no-stdout (-q) is set --output-file (-o) must be provided");
        Environment.Exit(1);
    }

    var services = new ServiceCollection().AddOpenConstructionSet().BuildServiceProvider();

    IInstallation? installation = null;

    var installationService = services.GetRequiredService<IInstallationService>();

    switch (installationType)
    {
        case InstallationType.Any:
            installationService.TryLocate(out installation);
            break;
        default:
            installationService.TryLocate(installationType.ToString(), out installation);
            break;
    }

    if (installation is null)
    {
        throw new InvalidOperationException($"Failed to locate install for option [{installationType}]");
    }

    var contextBuilder = services.GetRequiredService<IContextBuilder>();

    var contextOptions = new ModContextOptions(Guid.NewGuid().ToString(), installation)
    {
        LoadGameFiles = noGameFiles ? ModLoadType.None : ModLoadType.Base,
    };

    var context = await contextBuilder.BuildAsync(contextOptions);

    var json = JsonSerializer.Serialize(context.Items.Select(i => new Item(i)), SourceGenerationContext.Default.IEnumerableItem);

    List<Task> tasks = [];

    if (!noStdOut)
    {
        tasks.Add(Console.Out.WriteLineAsync(json));
    }

    if (outputFile is not null)
    {
        if (outputFile.Directory is not null)
        {
            Directory.CreateDirectory(outputFile.Directory.FullName);
        }

        tasks.Add(File.WriteAllTextAsync(outputFile.FullName, json));
    }

    await Task.WhenAll(tasks);
}

enum InstallationType { Any = 0, Steam = 1, Gog = 2, Local = 4 }

[JsonSerializable(typeof(IEnumerable<Item>))]
[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(FileValue))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(Vector3))]
[JsonSerializable(typeof(Vector4))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(ItemType))]
[JsonSerializable(typeof(ItemChangeType))]
[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(JsonStringEnumConverter<ItemType>), typeof(JsonStringEnumConverter<ItemChangeType>)])]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}