using FontReader.Framework.NameTable;

namespace Windows_Font_Replacement_Tool.Framework;

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