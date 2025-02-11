using OpenConstructionSet;
using OpenConstructionSet.Data;
using OpenConstructionSet.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

var file = args.Length > 0 ? args[0] : "data.json";

var installations = OcsDiscoveryService.Default.DiscoverAllInstallations();

Installation? installation = installations.Count switch
{
    0 => null,
    1 => installations.Values.First(),
    _ => PromptInstallationChoice()
};

if (installation is null)
{
    Console.WriteLine("Failed to find game");
    return 1;
}

Console.WriteLine();

Console.Write("Building data...");

var options = new OcsDataContexOptions(Name: Guid.NewGuid().ToString(),
                                       Installation: installation,
                                       LoadGameFiles: ModLoadType.Base,
                                       LoadEnabledMods: ModLoadType.Base,
                                       ThrowIfMissing: false);

var items = OcsDataContextBuilder.Default.Build(options).Items.Values.ToList();

Console.WriteLine(" Complete");

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
};

try
{
    Console.Write("Serializing to file...");

    var directory = Path.GetDirectoryName(file);

    if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.Delete(file);

    using var stream = File.Create(file);

    JsonSerializer.Serialize(stream, items, jsonOptions);
}
catch (Exception ex)
{
    Console.WriteLine(" Failed");

    Console.WriteLine();
    Console.WriteLine(ex.ToString());
    return 1;
}

Console.WriteLine(" Complete");

return 0;

Installation PromptInstallationChoice()
{
    var keys = installations.Keys.ToList();

    Console.WriteLine("Multiple installations found");

    // Output names of found installations
    for (var i = 0; i < keys.Count; i++)
    {
        Console.WriteLine($"{i + 1} - {keys[i]}");
    }

    Console.Write("Please select which to use: ");

    // Read the users input defaulting to the first installation if invalid
    if (!int.TryParse(Console.ReadLine() ?? "1", out var choice) || choice < 1 || choice > installations.Count)
    {
        choice = 1;
    }

    var selectedKey = keys[choice - 1];

    Console.WriteLine();

    Console.WriteLine($"Using the {selectedKey} installation");

    return installations[selectedKey];
}