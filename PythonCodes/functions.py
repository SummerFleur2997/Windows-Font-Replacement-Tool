import os
import sys
import argparse
from fontTools.ttLib import TTFont
from fontTools.ttLib.ttCollection import TTCollection

# 定义资源文件路径
path = os.getcwd()


def _get_font_family(ttfont: TTFont, subfamily: bool = True) -> str:
    """内部函数\n
    输入一个 ttfont 字体，返回其 FontFamily 属性。
    :param ttfont: ttf 字体
    :return: ttf 字体的 FontFamily 属性
    """
    name_table = ttfont["name"]
    font_family = ""
    font_subfamily = ""
    for data in name_table.names:
        if data.platformID != 3:
            continue
        if data.nameID == 1 and data.langID == 0x409:
            font_family = data.toUnicode()
        if data.nameID == 2 and data.langID == 0x409:
            font_subfamily = data.toUnicode()

    if subfamily:
        return font_family + "-" + font_subfamily
    return font_family


def font_property_replace(args) -> int:
    """
    将 args.xml 中的 name 表数据转移至 args.font 中，进行字体属性替换。命令行使用样例：

    functions.exe propertyRep [font绝对路径] [xml绝对路径] --path [带有目标文件名的目录名称]

    将会默认保存至 cache 目录，若提供可选参数 --path，则保存为 path
    """
    if args.font is None:
        return 1001
    if args.xml is None:
        return 1002
    temp_font = TTFont(args.xml)
    targeted_font = TTFont(args.font)
    targeted_font["name"] = temp_font["name"]
    if not args.path:
        targeted_font.save(f"{path}\\output\\cache\\{os.path.basename(args.xml)}")
    else:
        targeted_font.save(f"{path}\\output\\{args.path}")

    return 0


def merge_ttc(args) -> int:
    """
    将指定的 ttf 合并为 ttc，并保存至 args.dirname 文件夹内。命令行使用样例：

    functions.exe mergeTTC [dirname目录名称] [ttc字体集文件名]

    将会把 cache 目录下名为 [ttc]01.ttf, [ttc]02.ttf ... 等等文件合并为一个 ttc 字体集，
    然后存储到 [dirname] 目录下，名为 [ttc].ttc
    """
    ttc = TTCollection()
    num = 1
    for obj in os.listdir(f"{path}\\output\\cache"):
        if obj[:-6] == args.ttc:
            num += 1
    for suf in range(1, num):
        ttc.fonts.append(TTFont(f"{path}\\output\\cache\\{args.ttc}{suf:02d}.ttf"))
    ttc.save(f"{path}\\output\\{args.dirname}\\{args.ttc}.ttc")

    return 0


def split_ttc(args) -> int:
    """
    给定 arg.ttc 字体集文件，将其拆分为 ttf。命令行使用样例：

    functions.exe splitTTC [ttc绝对路径] --path [目录名称]
    """
    if args is None:
        return 3001
    index = 1
    name = os.path.basename(args.ttc)[:-4]
    ttc = TTCollection(args.ttc)
    if not args.path:
        for ttf in ttc.fonts:
            ttf.save(f"{path}\\output\\cache\\{name}{index:02d}.ttf")
            index += 1
    else:
        for ttf in ttc.fonts:
            font_family = _get_font_family(ttf)
            ttf.save(f"{path}\\output\\{args.path}\\{index:02d}_{font_family}.ttf")
            index += 1

    return 0


def get_font_family(arg) -> str:
    """
    _getFontFamily() 函数的命令行调用形式。命令行使用样例：

    functions.exe fontFamily [font绝对路径]
    """
    ttfont = TTFont(arg.font)
    return _get_font_family(ttfont, False)


def convert_type(args) -> int:
    """
    转化字体文件格式为 ttf 或 otf。命令行使用样例：

    functions.exe convertType ttf2otf [导出文件夹名称] [font绝对路径]
    """
    ttfont = TTFont(args.font)
    filename = os.path.basename(args.font)[:-4]
    dirname = args.dirname
    if args.type == "otf2ttf":
        ttfont.save(f"{path}\\output\\{dirname}\\{filename}.ttf")
        return 0
    elif args.type == "ttf2otf":
        ttfont.save(f"{path}\\output\\{dirname}\\{filename}.otf")
        return 0
    return 5001


def get_cjk_character_count(args) -> str:
    """
    获取 CJK Unified Ideographs (U+4E00～U+9FFF) 内的字符数量。命令行使用样例：

    functions.exe getCjk [font绝对路径]
    """
    font = TTFont(args.font)
    cjk_chars = set()

    for table in font["cmap"].tables:
        if table.isUnicode():
            cjk_chars.update(code for code in table.cmap if 0x4E00 <= code <= 0x9FFF)

    return str(len(cjk_chars))


def main():
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers()

    parser_func1 = subparsers.add_parser("propertyRep")
    parser_func1.add_argument("font")           # 个性化字体文件，应为绝对路径
    parser_func1.add_argument("xml")            # 带有 msyh 原始数据的文件，应为绝对路径
    parser_func1.add_argument("--path", required=False, default=False, type=str)
    parser_func1.set_defaults(func=font_property_replace)

    parser_func2 = subparsers.add_parser("mergeTTC")
    parser_func2.add_argument("dirname")        # output 文件夹下的输出目录名，应只保留文件夹名称
    parser_func2.add_argument("ttc")            # 需要合并的 ttf 系列，应去除所有后缀
    parser_func2.set_defaults(func=merge_ttc)

    parser_func3 = subparsers.add_parser("splitTTC")
    parser_func3.add_argument("ttc")            # 需要拆分的 ttc 文件，应为绝对路径
    parser_func3.add_argument("--path", required=False, default=False, type=str)
    parser_func3.set_defaults(func=split_ttc)

    parser_func4 = subparsers.add_parser("fontFamily")
    parser_func4.add_argument("font")
    parser_func4.set_defaults(func=get_font_family)

    parser_func5 = subparsers.add_parser("convertType")
    parser_func5.add_argument("type")
    parser_func5.add_argument("dirname")
    parser_func5.add_argument("font")
    parser_func5.set_defaults(func=convert_type)

    parser_func6 = subparsers.add_parser("getCjk")
    parser_func6.add_argument("font")
    parser_func6.set_defaults(func=get_cjk_character_count)

    args = parser.parse_args()
    try:
        result = args.func(args)
        if isinstance(result, str):
            print(result)
            sys.exit(0)
        else:
            sys.exit(result)
    except Exception as e:
        print(f"未捕获的异常: {str(e)}", file=sys.stderr)
        sys.exit(99)


if __name__ == "__main__":
    main()
