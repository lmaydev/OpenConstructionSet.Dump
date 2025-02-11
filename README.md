# OpenConstructionSet Dump
CLI to dump a games mod data to json.
By default loads base data files, active mods and writes to stdout.

```
ocs-dump --help

Description:
  OpenConstructionSet Dump - output Kenshi game data to JSON

Usage:
  ocs-dump [options]

Options:
  -q, --no-stdout                           Suppress output to stdout.
  -G, --no-game-files                       Prevent loading of base game data files.
  -i, --installation <Any|Gog|Local|Steam>  Installation to use. You can provide multiple values [default: Any]
  -o, --output-file <output-file>           Specify an output file.
  --version                                 Show version information
  -?, -h, --help                            Show help and usage information
  ```