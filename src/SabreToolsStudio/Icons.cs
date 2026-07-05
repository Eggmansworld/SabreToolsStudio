using Avalonia.Media;

namespace SabreToolsStudio;

/// <summary>
/// Embedded icon geometries (Material Design path data) so no icon font/package is required
/// </summary>
public static class Icons
{
    public static readonly StreamGeometry FolderPlus = StreamGeometry.Parse(
        "M10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6H12L10,4M15,9H17V12H20V14H17V17H15V14H12V12H15V9Z");

    public static readonly StreamGeometry SwapHorizontal = StreamGeometry.Parse(
        "M21,9L17,5V8H10V10H17V13M7,11L3,15L7,19V16H14V14H7V11Z");

    public static readonly StreamGeometry CheckCircle = StreamGeometry.Parse(
        "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z");

    public static readonly StreamGeometry Update = StreamGeometry.Parse(
        "M12,4V1L8,5L12,9V6A6,6 0 0,1 18,12C18,13.57 17.4,15 16.4,16.1L17.82,17.52C19.14,16.07 20,14.13 20,12A8,8 0 0,0 12,4M6,12C6,10.43 6.6,9 7.6,7.9L6.18,6.48C4.86,7.93 4,9.87 4,12A8,8 0 0,0 12,20V23L16,19L12,15V18A6,6 0 0,1 6,12Z");

    public static readonly StreamGeometry CallSplit = StreamGeometry.Parse(
        "M14,4L16.29,6.29L13.41,9.17L14.83,10.59L17.71,7.71L20,10V4M10,4H4V10L6.29,7.71L11,12.41V20H13V11.59L7.71,6.29L10,4Z");

    public static readonly StreamGeometry ChartBar = StreamGeometry.Parse(
        "M22,21H2V3H4V19H6V10H10V19H12V6H16V19H18V14H22V21Z");

    public static readonly StreamGeometry PlaylistPlay = StreamGeometry.Parse(
        "M19,9H2V11H19V9M19,5H2V7H19V5M2,15H15V13H2V15M17,13V19L22,16L17,13Z");

    public static readonly StreamGeometry FileDocument = StreamGeometry.Parse(
        "M13,9V3.5L18.5,9M6,2C4.89,2 4,2.89 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2H6Z");

    public static readonly StreamGeometry Cog = StreamGeometry.Parse(
        "M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.67 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z");

    public static readonly StreamGeometry Play = StreamGeometry.Parse(
        "M8,5.14V19.14L19,12.14L8,5.14Z");

    public static readonly StreamGeometry Stop = StreamGeometry.Parse(
        "M18,18H6V6H18V18Z");

    public static readonly StreamGeometry ContentCopy = StreamGeometry.Parse(
        "M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z");

    public static readonly StreamGeometry ChevronUp = StreamGeometry.Parse(
        "M7.41,15.41L12,10.83L16.59,15.41L18,14L12,8L6,14L7.41,15.41Z");

    public static readonly StreamGeometry ChevronDown = StreamGeometry.Parse(
        "M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z");

    public static readonly StreamGeometry Delete = StreamGeometry.Parse(
        "M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z");

    public static readonly StreamGeometry ContentSave = StreamGeometry.Parse(
        "M15,9H5V5H15M12,19A3,3 0 0,1 9,16A3,3 0 0,1 12,13A3,3 0 0,1 15,16A3,3 0 0,1 12,19M17,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V7L17,3Z");
}
