namespace FontTool.Framework.NameTable;

public class NameTableRecord
{
    public ushort PlatformID;
    public ushort EncodingID;
    public ushort LanguageID;
    public ushort NameID;
    public ushort Length;
    public uint StringDataOffset;

    public string StringData = "";

    public override string ToString() => string.Concat(PlatformID, "-", NameID, ": ", StringData);
}