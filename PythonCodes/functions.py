import argparse
import os
import sys
from fontTools.ttLib import TTFont
from fontTools.ttLib.ttCollection import TTCollection

# 定义资源文件路径
path = os.getcwd()


def merge_ttc(args) -> int:
    """
    将指定的 ttf 合并为 ttc，并保存至 args.dirname 文件夹内。命令行使用样例：

    functions.exe mergeTTC [dirname目录名称] [ttc字体集文件名]

    将会把 cache 目录下名为 [ttc]01.ttf, [ttc]02.ttf ... 等等文件合并为一个 ttc 字体集，
    然后存储到 [dirname] 目录下，名为 [ttc].ttc
    """
    ttc = TTCollection()
    num = 1
    for obj in os.listdir(f"{path}\\output\\{args.dirname}"):
        if obj[:-6] == args.ttc:
            num += 1
    for suf in range(1, num):
        ttc.fonts.append(TTFont(f"{path}\\output\\{args.dirname}\\{args.ttc}{suf:02d}.ttf"))
    ttc.save(f"{path}\\output\\{args.dirname}\\{args.ttc}.ttc")

    return 0


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


def main():
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers()

    parser_func2 = subparsers.add_parser("mergeTTC")
    parser_func2.add_argument("dirname")        # output 文件夹下的输出目录名，应只保留文件夹名称
    parser_func2.add_argument("ttc")            # 需要合并的 ttf 系列，应去除所有后缀
    parser_func2.set_defaults(func=merge_ttc)

    parser_func5 = subparsers.add_parser("convertType")
    parser_func5.add_argument("type")
    parser_func5.add_argument("dirname")
    parser_func5.add_argument("font")
    parser_func5.set_defaults(func=convert_type)

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
