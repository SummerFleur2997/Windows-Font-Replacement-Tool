import os
import shutil
import datetime
from fontTools.ttLib import TTFont


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


def UniteTTC(dirname):

    uniteTTC_cmd = [
        f'uniteTTC.exe "{path}\\output\\{dirname}\\msyh.ttc" "{path}\\output\\cache\\msyhr01.ttf" "{path}\\output\\cache\\msyhr02.ttf"',
        f'uniteTTC.exe "{path}\\output\\{dirname}\\msyhbd.ttc" "{path}\\output\\cache\\msyhb01.ttf" "{path}\\output\\cache\\msyhb02.ttf"',
        f'uniteTTC.exe "{path}\\output\\{dirname}\\msyhl.ttc" "{path}\\output\\cache\\msyhl01.ttf" "{path}\\output\\cache\\msyhl02.ttf"']

    return uniteTTC_cmd


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
