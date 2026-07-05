# SabreTools Studio

A cross-platform (Windows/Linux) graphical front-end for [SabreTools](https://github.com/SabreTools/SabreTools),
built with [Avalonia UI](https://avaloniaui.net/). SabreTools Studio assembles and runs SabreTools CLI
commands for you, with every option documented inline via tooltips sourced from the SabreTools wiki.

SabreTools Studio wraps the SabreTools executable as a child process (using its `--script` mode) and
streams all output into a sliding log drawer, so the main window stays dedicated to setting up commands.

## Features

All seven primary SabreTools features are available:

- **DAT From Dir** - create DATs from ROM folders, with all hash types and full header control
- **Sort/Rebuild** - rebuild collections to folders, TorrentZip, Zstandard Zip, TorrentGZ (Romba), or TAR
- **Verify** - check folders against DATs and produce fixdats
- **Update DATs** - convert, merge, diff, filter, dedupe, and rewrite DATs
- **Split DATs** - split by extension, hash, level, size, total size, or item type
- **Statistics** - report generation in text/CSV/HTML/SSV/TSV
- **Batch** - run batch scripts, with a visual script builder

Plus: named option presets per feature, live CLI command preview with copy-to-clipboard,
dependency-aware options (invalid flag combinations are prevented), wiki-sourced tooltips
on every control, a sliding log drawer with right-click "Open in File Explorer" on paths,
and an OS-following light/dark theme with override.

SabreTools Studio is fully portable: all settings and presets live in a single
`SabreToolsStudio.config` file next to the executable - no registry, no user-profile
folders. Output files default to the application folder when no output directory is set.

Note: the local SabreTools checkout carries uncommitted patches for Zstandard zip
reading and writing (`--zstd-zip`); the bundled CLI is built from that patched tree.

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

## Distributable builds

Self-contained single-file builds (no .NET install required on the target machine):

```
./publish-win.ps1      # -> dist/win-x64
./publish-linux.sh     # -> dist/linux-x64
```

## License

MIT, matching SabreTools itself.
