using CommunityToolkit.Mvvm.ComponentModel;

namespace SabreToolsStudio.ViewModels;

/// <summary>
/// A checkable option with wiki-sourced description for tooltips
/// </summary>
public partial class FormatOption(string value, string label, string description) : ObservableObject
{
    public string Value { get; } = value;
    public string Label { get; } = label;
    public string Description { get; } = description;

    [ObservableProperty]
    private bool _isSelected;
}

/// <summary>
/// A single-choice option for combo boxes; an empty Value means "use the default"
/// </summary>
public sealed record ComboOption(string Value, string Label, string Description)
{
    public override string ToString() => Label;
}

/// <summary>
/// Shared option lists sourced from the SabreTools wiki and current CLI code
/// </summary>
public static class OptionCatalog
{
    /// <summary>DAT output formats accepted by --output-type= (fresh instances per page)</summary>
    public static List<FormatOption> CreateDatFormats() =>
    [
        new("xml", "Logiqx XML", "The de facto standard DAT format, used by most ROM managers. This is the default output format."),
        new("cmp", "ClrMamePro", "The classic ClrMamePro text format."),
        new("sd", "SabreDAT XML", "SabreTools' own XML format with extended fields."),
        new("json", "SabreDAT JSON", "SabreTools' own JSON format with extended fields."),
        new("csv", "CSV", "Standardized comma-separated values."),
        new("ssv", "SSV", "Standardized semicolon-separated values."),
        new("tsv", "TSV", "Standardized tab-separated values."),
        new("dc", "DOSCenter", "DOSCenter text format."),
        new("rc", "RomCenter", "RomCenter INI-style format."),
        new("am", "AttractMode", "AttractMode front-end romlist format."),
        new("everdrive", "Everdrive SMDB", "Everdrive Simple Metadata Database format."),
        new("lr", "MAME Listrom", "MAME -listrom style output."),
        new("lx", "MAME ListXML", "MAME -listxml style output."),
        new("sl", "MAME Software List", "MAME software list XML format."),
        new("miss", "Missfile", "Plain list of names, typically used for missing-file lists."),
        new("msx", "openMSX", "openMSX software database XML format."),
        new("ol", "OfflineList", "OfflineList XML format."),
        new("ado", "Archive.org", "Archive.org files XML format."),
        new("sfv", "SFV", "SFV hashfile (CRC-32)."),
        new("md2", "MD2 Hashfile", "Hashfile listing MD2 hashes."),
        new("md4", "MD4 Hashfile", "Hashfile listing MD4 hashes."),
        new("md5", "MD5 Hashfile", "Hashfile listing MD5 hashes."),
        new("sha1", "SHA-1 Hashfile", "Hashfile listing SHA-1 hashes."),
        new("sha256", "SHA-256 Hashfile", "Hashfile listing SHA-256 hashes."),
        new("sha384", "SHA-384 Hashfile", "Hashfile listing SHA-384 hashes."),
        new("sha512", "SHA-512 Hashfile", "Hashfile listing SHA-512 hashes."),
        new("spamsum", "SpamSum Hashfile", "Hashfile listing SpamSum fuzzy hashes."),
    ];

    /// <summary>Hash inclusion flags for DAT From Dir (fresh instances per page)</summary>
    public static List<FormatOption> CreateHashOptions() =>
    [
        new("--include-crc", "CRC-32", "Enable CRC-32 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-md5", "MD5", "Enable MD5 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-sha1", "SHA-1", "Enable SHA-1 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-sha256", "SHA-256", "Enable SHA-256 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-sha384", "SHA-384", "Enable SHA-384 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-sha512", "SHA-512", "Enable SHA-512 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-md2", "MD2", "Enable MD2 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-md4", "MD4", "Enable MD4 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-ripemd128", "RIPEMD-128", "Enable RIPEMD-128 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-ripemd160", "RIPEMD-160", "Enable RIPEMD-160 calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
        new("--include-spamsum", "SpamSum", "Enable SpamSum fuzzy-hash calculation for each file. Selecting any hash overrides the default of CRC-32, MD5, and SHA-1."),
    ];

    /// <summary>Rebuild output formats for Sort; empty value = uncompressed folders (default)</summary>
    public static readonly IReadOnlyList<ComboOption> RebuildFormats =
    [
        new("", "Uncompressed folders (default)", "Files are rebuilt into plain, uncompressed folders in the output directory."),
        new("--torrent-zip", "TorrentZip", "Files are rebuilt to TorrentZip (TZip) files: ZIP archives with standardized header information so identical content always produces byte-identical archives. Widely used by RomVault and other ROM managers."),
        new("--zstd-zip", "Zstandard Zip", "Files are rebuilt to ZIP files compressed with Zstandard (compression method 93), the same scheme used by RomVault's ZSTD mode and 7-Zip-Zstandard. Better compression than Deflate, but requires zstd-aware tools to open."),
        new("--torrent-gzip", "TorrentGZ", "Files are rebuilt to TorrentGZ (TGZ) files: GZip archives with standardized headers, named by the SHA-1 of the contained file. Primarily used by Romba-style depots."),
        new("--tar", "TAR", "Files are rebuilt to Tape ARchive (TAR) files: a standardized, uncompressed container format widely used in backup applications."),
        new("--torrent-7zip", "Torrent7Zip", "Files are rebuilt to Torrent7Zip (T7Z) files, based on the 7Zip LZMA container. Note: this currently does not produce proper Torrent-compatible outputs."),
    ];

    /// <summary>DAT preprocessing merge modes (--dat-X flags); empty value = use DAT as-is</summary>
    public static readonly IReadOnlyList<ComboOption> MergeModes =
    [
        new("", "Use DAT as-is (default)", "No preprocessing is applied; sets are used exactly as described in the DAT."),
        new("--dat-split", "Split", "Remove redundant files shared between parents and children based on the romof and cloneof tags."),
        new("--dat-merged", "Merged", "Parent sets contain all items from their children based on the cloneof tag."),
        new("--dat-full-merged", "Fully Merged", "Parent sets contain all items from their children, with deduplication performed within each parent."),
        new("--dat-non-merged", "Non-Merged", "Child sets contain all items from their parent set based on the romof and cloneof tags."),
        new("--dat-device-non-merged", "Device Non-Merged", "Child sets contain all items from their device references."),
        new("--dat-full-non-merged", "Fully Non-Merged", "Child sets contain all items from parents and device references; every set is fully self-contained."),
    ];

    /// <summary>Values for the forcemerging header tag</summary>
    public static readonly IReadOnlyList<ComboOption> ForceMergingModes =
    [
        new("", "Default (not set)", "Do not set the forcemerging tag in the output DAT header."),
        new("split", "Split", "Instruct ROM managers to create split sets."),
        new("merged", "Merged", "Instruct ROM managers to create merged sets."),
        new("fullmerged", "Fully Merged", "Instruct ROM managers to create fully merged sets."),
        new("nonmerged", "Non-Merged", "Instruct ROM managers to create non-merged sets."),
        new("full", "Fully Non-Merged", "Instruct ROM managers to create fully non-merged sets."),
        new("device", "Device Non-Merged", "Instruct ROM managers to create device non-merged sets."),
    ];

    /// <summary>Values for the forcenodump header tag</summary>
    public static readonly IReadOnlyList<ComboOption> ForceNodumpModes =
    [
        new("", "Default (not set)", "Do not set the forcenodump tag in the output DAT header."),
        new("obsolete", "Obsolete", "Mark nodump handling as obsolete."),
        new("required", "Required", "Mark nodump entries as required."),
        new("ignore", "Ignore", "Instruct ROM managers to ignore nodump entries."),
    ];

    /// <summary>Values for the forcepacking header tag</summary>
    public static readonly IReadOnlyList<ComboOption> ForcePackingModes =
    [
        new("", "Default (not set)", "Do not set the forcepacking tag in the output DAT header."),
        new("zip", "Zip", "Instruct ROM managers to rebuild all sets to named archives."),
        new("unzip", "Unzip", "Instruct ROM managers to rebuild all sets to named folders."),
        new("partial", "Partial", "Rebuild sets with more than one item to named folders; the rest go directly to the output folder."),
        new("flat", "Flat", "Rebuild all sets to the output folder without named subfolders."),
    ];
}
