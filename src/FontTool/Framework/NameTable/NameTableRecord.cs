namespace FontTool.Framework.NameTable;

/// <summary>
/// Auxiliary class for storing name table records.
/// </summary>
public class NameTableRecord
{
    public ushort PlatformID;
    public ushort EncodingID;
    public ushort LanguageID;
    public ushort NameID;
    public ushort Length;
    public uint StringDataOffset;

    public string StringData = "";

    private string NameId2String()
        => NameID switch
        {
            0 => "Copyright",
            1 => "FontFamily",
            2 => "FontSubfamily",
            3 => "UniqueFontID",
            4 => "FullName",
            5 => "Version",
            6 => "PostScriptName",
            7 => "Trademark",
            8 => "Manufacturer",
            9 => "Designer",
            10 => "Description",
            11 => "URLVendor",
            12 => "URLDesigner",
            13 => "LicenseDescription",
            14 => "LicenseInfoURL",
            16 => "TypographicFamily",
            17 => "TypographicSubfamily",
            _ => NameID.ToString()
        };

    public override string ToString()
        => string.Concat(PlatformID, "-", LanguageID, "-", NameId2String(), ": ", StringData);
}