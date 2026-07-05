# SabreTools Studio

A cross-platform (Windows/Linux) graphical front-end for [SabreTools](https://github.com/SabreTools/SabreTools),
built with [Avalonia UI](https://avaloniaui.net/). SabreTools Studio assembles and runs SabreTools CLI
commands for you, with every option documented inline via tooltips sourced from the SabreTools wiki.

SabreTools Studio wraps the SabreTools executable as a child process (using its `--script` mode) and
streams all output into a sliding log drawer, so the main window stays dedicated to setting up commands.

## Features

| Feature | Status |
| ------- | ------ |
| Statistics Output | Available |
| DAT From Dir (DFD/D2D) | Planned (session 2) |
| Verify from DAT | Planned (session 2) |
| Sort/Rebuild from DAT | Planned (session 3) |
| Specialized DAT Splitting | Planned (session 3) |
| Update and Manipulate DATs | Planned (session 4) |
| Batch Running | Planned (session 5) |
| Extract/Restore Headers | Planned (session 5) |

Plus: named option presets per feature, live CLI command preview with copy-to-clipboard,
OS-following light/dark theme with manual override, and global thread/log-level options.

## Building

Requires the .NET 10 SDK (matching upstream SabreTools) and a sibling checkout of the
SabreTools repository (`../SabreTools` relative to this repo). After cloning SabreTools,
remember to run `git submodule update --init` inside it — SabreTools.Serialization is a submodule.

```
# 1. Publish the SabreTools CLI into tools/sabretools (bundled with the GUI)
./build-cli.ps1        # Windows
./build-cli.sh         # Linux

# 2. Build and run the GUI
dotnet run --project src/SabreToolsStudio
```

A different SabreTools executable can be selected at runtime on the Settings page.

## License

MIT, matching SabreTools itself.
