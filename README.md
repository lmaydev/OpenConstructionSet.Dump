# OpenConstructionSet.Dump

A simple application that uses the [OpenConstructionSet](https://github.com/lmaydev/OpenConstructionSet) to load and dump your current [Kenshi](https://lofigames.com/) game data to JSON.

## Usage

By default `ocs-dump` will save to `data.json` in the active directory.
If an argument is passed it will be treated as the output file.

## Example

Save to `data.json`

`ocs-dump`

Save to custom file

`ocs-dump custom.json`

`ocs-dump output/data.json`

`ocs-dump C:/output/data.json`