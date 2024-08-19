import os
import shutil
import datetime
from uitools import *
from fontTools.ttLib import TTFont
from fontTools.ttLib.ttCollection import TTCollection

selected_font = []
custom_font = ()
model_font = None

for _ in range(16):
    selected_font.append(None)

# 定义资源文件路径
path = os.getcwd()
xmls_path = path + r'\Libs\xmls'


def OpenHelpDoc():
    os.startfile(f"{path}\\Libs\\Help.pdf")


def OpenOutputDir():
    os.startfile(f"{path}\\output")


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


def CustomFileOpen(type_):

    global custom_font, model_font

    if type_ == "single":
        temp = FileOpen([('所有支持的字体文件', '.ttf .otf .ttc')])
        if temp is None:
            return
        if temp[-4:] == '.ttc':
            os.makedirs(f"{path}\\output\\cache", exist_ok=True)
            splitTTC(temp)
            custom_font = (".ttc", )
        else:
            custom_font = (".ttf", temp)

    elif type_ == "multiple":
        temp = [".ttf"]
        fonts = FilesOpen()
        if fonts is None:
            return
        temp.extend(fonts)
        custom_font = temp

    elif type_ == "model":
        temp = FileOpen()
        if temp is None:
            return
        model_font = temp


def fontPropertyReplace(font, xml):
    temp_font = TTFont(xml)
    targeted_font = TTFont(font)
    targeted_font["name"] = temp_font["name"]
    targeted_font.save(f"{path}\\output\\cache\\{os.path.basename(xml)[:-4]}.ttf")


def mergeTTC(dirname, ttcs=("msyh", "msyhbd", "msyhl")):

    for ttc in ttcs:
        TTC = TTCollection()
        num = 1
        for obj in os.listdir(f"{path}\\output\\cache"):
            if obj[:-6] == ttc:
                num += 1
        for suf in range(1, num):
            TTC.fonts.append(TTFont(f"{path}\\output\\cache\\{ttc}{suf:02d}.ttf"))

        TTC.save(f"{path}\\output\\{dirname}\\{ttc}.ttc")


def splitTTC(ttc):
    index = 1
    name = os.path.basename(ttc)[:-4]
    TTC = TTCollection(ttc)
    for ttf in TTC.fonts:
        ttf.save(f"{path}\\output\\cache\\{name}{index:02d}.ttf")
        index += 1


if __name__ == "__main__":
    pass
