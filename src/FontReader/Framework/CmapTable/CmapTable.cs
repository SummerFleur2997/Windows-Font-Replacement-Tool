namespace FontReader.Framework.CmapTable;

public class CmapTable : Table
{
    public CmapTable(uint checkSum, uint offset, uint length)
        : base("cmap", checkSum, offset, length) { }
}