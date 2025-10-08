namespace FontTool.Framework.CmapTable;

public class CmapSubTable
{
    public ushort PlatformID;
    public ushort EncodingID;
    public uint TableDataOffset;
    public int Precedence;

    public CmapSubTable(ushort platformID, ushort encodingID, uint tableDataOffset, int precedence)
    {
        PlatformID = platformID;
        EncodingID = encodingID;
        TableDataOffset = tableDataOffset;
        Precedence = precedence;
    }
}