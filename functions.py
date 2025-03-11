import os
import argparse
from fontTools.ttLib import TTFont
from fontTools.ttLib.ttCollection import TTCollection

# selected_font = []
#
# for _ in range(16):
#     selected_font.append(None)

# 定义资源文件路径
path = os.getcwd()


# def OpenHelpDoc():
#     os.startfile(f"{path}\\Libs\\Help.pdf")
#
#
# def OpenOutputDir():
#     os.startfile(f"{path}\\output")
#
#
# def InitOutput():
#
#     time = str(datetime.datetime.now())[0:19]
#     dirname = time.replace('-', '').replace(':', '').replace(' ', '_')
#     dirname = dirname + "_" + os.path.basename(selected_font[0])[0:-4]
#
#     os.makedirs(f"{path}\\output\\{dirname}")
#
#     for file in os.listdir(f"{path}\\output\\cache"):
#         if file[0] == 's' or file[0] == 'S':
#             src = os.path.join(f"{path}\\output\\cache", file)
#             dst = os.path.join(f"{path}\\output\\{dirname}", file)
#             shutil.move(src, dst)
#
#     return dirname


def fontPropertyReplace(args) -> int:
    """
    将 args.xml 中的 name 表数据转移至 args.font 中，进行字体属性替换。
    :param args: 命令行传递参数
    """
    if args.font is None:
        return 1001
    if args.xml is None:
        return 1002
    temp_font = TTFont(args.xml)
    targeted_font = TTFont(args.font)
    targeted_font["name"] = temp_font["name"]
    targeted_font.save(f"{path}\\output\\cache\\{os.path.basename(args.xml)}")

    return 0


def mergeTTC(args) -> int:
    """
    将指定的 ttf 合并为 ttc，并保存至 args.dirname 文件夹内。
    :param args: 命令行传递参数
    """
    TTC = TTCollection()
    num = 1
    for obj in os.listdir(f"{path}\\output\\cache"):
        if obj[:-6] == args.ttc:
            num += 1
    for suf in range(1, num):
        TTC.fonts.append(TTFont(f"{path}\\output\\cache\\{args.ttc}{suf:02d}.ttf"))
    TTC.save(f"{path}\\output\\{args.dirname}\\{args.ttc}.ttc")

    return 0


def splitTTC(arg) -> int:
    """
    给定 arg.ttc 字体集文件，将其拆分为 ttf。
    :param arg: 命令行传递参数
    """
    if arg is None:
        return 3001
    index = 1
    name = os.path.basename(arg.ttc)[:-4]
    TTC = TTCollection(arg.ttc)
    for ttf in TTC.fonts:
        ttf.save(f"{path}\\output\\cache\\{name}{index:02d}.ttf")
        index += 1

    return 0


def main():
    parser = argparse.ArgumentParser(description="我的脚本")
    subparsers = parser.add_subparsers(help="子命令帮助")

    parser_func1 = subparsers.add_parser("propertyRep")
    parser_func1.add_argument("font")           # 个性化字体文件，应为绝对路径
    parser_func1.add_argument("xml")            # 带有 msyh 原始数据的文件，应为绝对路径
    parser_func1.set_defaults(func=fontPropertyReplace)

    parser_func2 = subparsers.add_parser("mergeTTC")
    parser_func2.add_argument("dirname")        # output 文件夹下的输出目录名，应只保留文件夹名称
    parser_func2.add_argument("ttc")            # 需要合并的 ttf 系列，应去除所有后缀
    parser_func2.set_defaults(func=mergeTTC)

    parser_func3 = subparsers.add_parser("splitTTC")
    parser_func3.add_argument("ttc")            # 需要拆分的 ttc 文件，应为绝对路径
    parser_func3.set_defaults(func=splitTTC)

    args = parser.parse_args()
    args.func(args)

# def getSHA1(name):
#     with open(f"{path}/Libs/{name}", 'rb') as db:
#         return hashlib.new("sha1", db.read()).hexdigest()
#
#
# def DecodeLibs():
#     with open(f"{path}/Libs/dataLibs.lib", 'rb') as f:
#         data = f.read()
#     with open(f"{path}/Libs/archive", 'wb') as f:
#         archive = base64.b64decode(data)
#         f.write(archive)
#     with zipfile.ZipFile(f"{path}/Libs/archive", "r") as f:
#         f.extractall(f'{path}/Libs/xmls', pwd=b"SaXNnYbgwMjDvgyl")
#         f.close()
#     sha1 = getSHA1("archive")
#     os.remove(f"{path}/Libs/archive")
#     return sha1


if __name__ == "__main__":
    main()
