import os
import shutil
import datetime
from fontTools.ttLib import TTFont
from fontTools.ttLib.ttCollection import TTCollection

selected_font = []

for _ in range(16):
    selected_font.append(None)

# 定义资源文件路径
path = os.getcwd()
xmls_path = path + r'\Libs\xmls'
tool_path = path + r'\Libs\tools'


def IsFontListValid():
    for i in range(16):
        if selected_font[i] is None:
            return False
    return True


def InitOutput():
    time = str(datetime.datetime.now())[0:19]
    dirname = time.replace('-', '').replace(':', '').replace(' ', '_')
    dirname = dirname + "_" + os.path.basename(selected_font[0])[0:-4]

    os.makedirs(f"{path}\\output\\{dirname}")

    for file in os.listdir(f"{path}\\output\\cache"):
        if file[0] == 's' or file[0] == 'S':
            src = os.path.join(f"{path}\\output\\cache", file)
            dst = os.path.join(f"{path}\\output\\{dirname}", file)
            shutil.move(src, dst)

    return dirname


def fontPropertyReplace(font, xml):
    temp_font = TTFont(f"{xmls_path}\\{xml}")
    targeted_font = TTFont(font)
    targeted_font["name"] = temp_font["name"]
    targeted_font.save(f"{path}\\output\\cache\\{xml}")


def mergeTTC(dirname):
    ttcs = ("msyh", "msyhbd", "msyhl")
    for col in ttcs:
        ttc = TTCollection()
        for suf in ("01.ttf", "02.ttf"):
            ttc.fonts.append(TTFont(f"{path}\\output\\cache\\{col}{suf}"))
        ttc.save(f"{path}\\output\\{dirname}\\{col}.ttc")
