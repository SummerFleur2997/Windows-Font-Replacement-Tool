using FontTool.Framework.NameTable;

namespace WFRT.Framework;

internal struct NameTableData
{
    public NameTable NameTable;
    public string FontName;

    public NameTableData(NameTable nameTable, string fontName)
    {
        NameTable = nameTable;
        FontName = fontName;
    }
}