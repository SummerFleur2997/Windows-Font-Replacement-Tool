import ctypes
import subprocess
import tkinter as tk
from functions import *
from tkinter import filedialog
from tkinter import messagebox
from win32api import GetSystemMetrics


# region # 功能函数
def SingleFileModify():

    button_sof['text'] = "制作中..."
    button_sof['fg'] = "#EE0000"

    selected_font[0] = FileOpen()
    if selected_font[0] is None:
        button_sof['text'] = "选择字体并开始制作"
        button_sof['fg'] = "#000000"
        return

    os.makedirs(f"{path}\\output\\cache", exist_ok=True)

    for xml in os.listdir(xmls_path):
        fontPropertyReplace(selected_font[0], xml)

    mergeTTC(InitOutput())

    shutil.rmtree(f"{path}\\output\\cache")
    messagebox.showinfo(title="提示", message=" 字体制作完成，可点击右下角“打开导出目录”\n 来查看做好的字体文件")

    button_sof['text'] = "选择字体并开始制作"
    button_sof['fg'] = "#000000"


def MultiFileModify():

    for i in selected_font:
        if i is None:
            return

    xmlListEN = ["segoeui.ttf", "segoeuii.ttf", "seguisb.ttf", "seguisbi.ttf", "segoeuib.ttf",
                 "segoeuiz.ttf", "segoeuil.ttf", "seguili.ttf", "segoeuisl.ttf", "seguisli.ttf",
                 "seguibl.ttf",  "seguibli.ttf", "SegUIVar.ttf"]
    xmlListZH = [["msyh01.ttf", "msyh02.ttf"], ["msyhbd01.ttf", "msyhbd02.ttf"], ["msyhl01.ttf", "msyhl02.ttf"]]

    os.makedirs(f"{path}\\output\\cache", exist_ok=True)

    for i in range(3):
        for j in range(2):
            fontPropertyReplace(selected_font[i], xmlListZH[i][j])

    for i in range(13):
        fontPropertyReplace(selected_font[i+3], xmlListEN[i])

    mergeTTC(InitOutput())

    shutil.rmtree(f"{path}\\output\\cache")
    messagebox.showinfo(title="提示", message=" 字体制作完成，可点击右下角“打开导出目录”\n 来查看做好的字体文件")


def FileOpen():
    font_path = filedialog.askopenfilename(title='选择待替换字体文件', filetypes=[('True Type 字体', '*.ttf')])
    if font_path != '':
        return font_path


def SingleFileInit():
    button_sof['state'] = 'normal'
    label_s['fg'] = "#8E65E8"
    for j, k in zip(button_m, label_m):
        j['state'] = 'disabled'
        k['fg'] = "#AAAAAA"


def MultiFileInit():
    selected_font[0] = None
    button_sof['state'] = 'disabled'
    label_s['fg'] = "#AAAAAA"
    FontCheck()
    for j in button_m:
        j['state'] = 'normal'


def OpenOutputDir():
    os.startfile(f"{path}\\output")


def CopyGitHubLink():
    subprocess.run('echo https://github.com/SummerFleur2997/Windows-Font-Replacement-Tool/issues | clip', shell=True)
    messagebox.showinfo(title="提示", message=" 链接已复制至剪贴板，\n 请前往浏览器粘贴访问")


def OpenHelpDoc():
    os.startfile(f"{path}\\Libs\\Help.pdf")
# endregion


# 设置窗体参数
root = tk.Tk()
root.title('字体属性替换工具')

# 对窗体进行屏幕适配
ctypes.windll.shcore.SetProcessDpiAwareness(1)
ScreenHeight = GetSystemMetrics(1)

# 定义窗体的尺寸、标题、背景颜色
rootHeight = int(ScreenHeight * 3 / 5)
rootWidth = int(ScreenHeight * 4 / 5)
offset = int(ScreenHeight / 25)
root.geometry(f'{rootWidth}x{rootHeight}+{offset * 5}+{offset * 4}')
root.resizable(False, False)
ScaleFactor = ScreenHeight / 1440
root.call('tk', 'scaling', ScaleFactor / 0.75)

# 字体种类确认
label_tip = tk.Label(root, text="注意！在使用本工具前，请先确认\n待替换字体是否拥有多个字重!",
                     font=('Microsoft YaHei', 17),
                     fg='#FF0000', justify='left', anchor='nw')
label_tip.place(relx=0.06, rely=0.05)

button_single = tk.Button(root, text='我的字体只有一个字重', border=2, font=('Microsoft YaHei', 17), relief='groove')
button_single.place(relx=0.38, rely=0.06, relheight=0.06, relwidth=0.25)
button_single['command'] = SingleFileInit

button_multiple = tk.Button(root, text='我的字体有多个字重', border=2, font=('Microsoft YaHei', 17), relief='groove')
button_multiple.place(relx=0.66, rely=0.06, relheight=0.06, relwidth=0.25)
button_multiple['command'] = MultiFileInit

# region # 编辑区UI划分
Group0 = tk.LabelFrame(root, text=' 说明与帮助 ', padx=0.1 * offset, pady=0.1 * offset,
                       width=0.98 * rootWidth, height=0.21 * rootHeight, font=('Microsoft YaHei', 17))
Group0.pack(side='bottom', anchor='s', padx=0.3 * offset, pady=0.3 * offset)

Group1 = tk.LabelFrame(root, text=' 多字重字体编辑区 ', padx=0.1 * offset, pady=0.1 * offset,
                       width=0.98 * rootWidth, height=0.41 * rootHeight, font=('Microsoft YaHei', 17))
Group1.pack(side='bottom', anchor='s', padx=0.3 * offset)

Group2 = tk.LabelFrame(root, text=' 单字重字体编辑区 ', padx=0.1 * offset, pady=0.1 * offset,
                       width=0.48 * rootWidth, height=0.16 * rootHeight, font=('Microsoft YaHei', 17))
Group2.pack(side='left', anchor='s', padx=0.3 * offset, pady=0.3 * offset)

label_mtip = tk.Label(Group0, font=('Microsoft YaHei', 16), justify='left', anchor='w')
label_mtip.place(relx=0.02, rely=0.02, anchor='nw')
label_mtip['text'] = "★ 本工具仅支持修改 .ttf 字体文件属性\n" \
                     "★ 使用该工具过程中产生的任何问题，请\n"\
                     "★ 多字重字体diy较为复杂，如需要帮助，可阅读帮助文档\n" \
                     "★ 多字重编辑区内，将鼠标光标置于文字标签上方，可以查看当前选择的字体是什么"

openoutput = tk.Button(Group0, text='打开导出目录', border=2, font=('Microsoft YaHei', 17), relief='groove')
openoutput.place(relx=0.87, rely=0.24, relwidth=0.15, relheight=0.34, anchor='center')
openoutput['command'] = OpenOutputDir

helpdocument = tk.Button(Group0, text='打开帮助文档', border=2, font=('Microsoft YaHei', 17), relief='groove')
helpdocument.place(relx=0.87, rely=0.69, relwidth=0.15, relheight=0.34, anchor='center')

GitHub = tk.Label(Group0, text='点击前往 GitHub 反馈', font=('Microsoft YaHei', 16), fg='blue')
GitHub.place(relx=0.378, rely=0.338, relwidth=0.19, relheight=0.20, anchor='w')
GitHub.bind("<Button-1>", lambda event: CopyGitHubLink())
# endregion


# region # 单字重编辑区组件
label_s = tk.Label(Group2, text='选择需要修改的字体：', font=('Microsoft YaHei', 16), fg="#AAAAAA")
label_s.place(relx=0.04, rely=0.45, anchor='w')

button_sof = tk.Button(Group2, text='选择字体并开始制作', border=2, font=('Microsoft YaHei', 16), relief='groove',
                       state='disabled')
button_sof.place(relx=0.68, rely=0.45, relwidth=0.43, relheight=0.45, anchor='center')
button_sof['command'] = SingleFileModify
# endregion


# region # 多字重编辑区功能函数
def FontCheck():
    for ID in range(16):
        if selected_font[ID] is not None:
            label_m[ID]['fg'] = "#3EDE17"
        else:
            label_m[ID]['fg'] = "#8E65E8"
    if IsFontListValid():
        button_mstart['state'] = 'normal'
    else:
        button_mstart['state'] = 'disabled'


def ZhReSelect():
    selected_font[0] = FileOpen()
    FontCheck()


def ZhBdSelect():
    selected_font[1] = FileOpen()
    FontCheck()


def ZhLtSelect():
    selected_font[2] = FileOpen()
    FontCheck()


def EnVaSelect():
    selected_font[15] = FileOpen()
    FontCheck()


def EnReSelect():
    selected_font[3] = FileOpen()
    FontCheck()


def EnRISelect():
    selected_font[4] = FileOpen()
    FontCheck()


def EnSBSelect():
    selected_font[5] = FileOpen()
    FontCheck()


def EnSBISelect():
    selected_font[6] = FileOpen()
    FontCheck()


def EnBdSelect():
    selected_font[7] = FileOpen()
    FontCheck()


def EnBISelect():
    selected_font[8] = FileOpen()
    FontCheck()


def EnLtSelect():
    selected_font[9] = FileOpen()
    FontCheck()


def EnLISelect():
    selected_font[10] = FileOpen()
    FontCheck()


def EnSLSelect():
    selected_font[11] = FileOpen()
    FontCheck()


def EnSLISelect():
    selected_font[12] = FileOpen()
    FontCheck()


def EnBlSelect():
    selected_font[13] = FileOpen()
    FontCheck()


def EnBlISelect():
    selected_font[14] = FileOpen()
    FontCheck()
# endregion


# region # 多字重编辑区字体选择组件
label_zhr = tk.Label(Group1, text='中文常规字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                     anchor='w')
label_zhr.place(relx=0.685, rely=0.1, relheight=0.128, anchor='w')
button_mof_zhr = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                           state='disabled')
button_mof_zhr.place(relx=0.83, rely=0.1, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_zhr['command'] = ZhReSelect

label_zhb = tk.Label(Group1, text='中文粗体字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                     anchor='w')
label_zhb.place(relx=0.685, rely=0.25, relheight=0.128, anchor='w')
button_mof_zhb = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                           state='disabled')
button_mof_zhb.place(relx=0.83, rely=0.25, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_zhb['command'] = ZhBdSelect

label_zhl = tk.Label(Group1, text='中文细体字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                     anchor='w')
label_zhl.place(relx=0.685, rely=0.4, relheight=0.128, anchor='w')
button_mof_zhl = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                           state='disabled')
button_mof_zhl.place(relx=0.83, rely=0.4, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_zhl['command'] = ZhLtSelect

label_enVar = tk.Label(Group1, text='西文可变字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                       anchor='w')
label_enVar.place(relx=0.685, rely=0.55, relheight=0.128, anchor='w')
button_mof_enVar = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                             state='disabled')
button_mof_enVar.place(relx=0.83, rely=0.55, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enVar['command'] = EnVaSelect

label_enl = tk.Label(Group1, text='西文细体字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                     anchor='w')
label_enl.place(relx=0.025, rely=0.1, relheight=0.128, anchor='w')
button_mof_enl = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                           state='disabled')
button_mof_enl.place(relx=0.17, rely=0.1, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enl['command'] = EnLtSelect

label_ensl = tk.Label(Group1, text='西文半细字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                      anchor='w')
label_ensl.place(relx=0.025, rely=0.25, relheight=0.128, anchor='w')
button_mof_ensl = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                            state='disabled')
button_mof_ensl.place(relx=0.17, rely=0.25, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_ensl['command'] = EnSLSelect

label_enr = tk.Label(Group1, text='西文常规字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                     anchor='w')
label_enr.place(relx=0.025, rely=0.4, relheight=0.128, anchor='w')
button_mof_enr = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                           state='disabled')
button_mof_enr.place(relx=0.17, rely=0.4, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enr['command'] = EnReSelect

label_ensb = tk.Label(Group1, text='西文半粗字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                      anchor='w')
label_ensb.place(relx=0.025, rely=0.55, relheight=0.128, anchor='w')
button_mof_ensb = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                            state='disabled')
button_mof_ensb.place(relx=0.17, rely=0.55, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_ensb['command'] = EnSBSelect

label_enb = tk.Label(Group1, text='西文粗体字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                     anchor='w')
label_enb.place(relx=0.025, rely=0.7, relheight=0.128, anchor='w')
button_mof_enb = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                           state='disabled')
button_mof_enb.place(relx=0.17, rely=0.7, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enb['command'] = EnBdSelect

label_enbl = tk.Label(Group1, text='西文特粗字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                      anchor='w')
label_enbl.place(relx=0.025, rely=0.85, relheight=0.128, anchor='w')
button_mof_enbl = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                            state='disabled')
button_mof_enbl.place(relx=0.17, rely=0.85, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enbl['command'] = EnBlSelect

label_enli = tk.Label(Group1, text='西文细斜字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                      anchor='w')
label_enli.place(relx=0.355, rely=0.1, relheight=0.128, anchor='w')
button_mof_enli = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                            state='disabled')
button_mof_enli.place(relx=0.50, rely=0.1, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enli['command'] = EnLISelect

label_ensli = tk.Label(Group1, text='西文半细斜体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                       anchor='w')
label_ensli.place(relx=0.355, rely=0.25, relheight=0.128, anchor='w')
button_mof_ensli = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                             state='disabled')
button_mof_ensli.place(relx=0.50, rely=0.25, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_ensli['command'] = EnSLISelect

label_enri = tk.Label(Group1, text='西文斜体字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                      anchor='w')
label_enri.place(relx=0.355, rely=0.4, relheight=0.128, anchor='w')
button_mof_enri = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                            state='disabled')
button_mof_enri.place(relx=0.50, rely=0.4, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enri['command'] = EnRISelect

label_ensbi = tk.Label(Group1, text='西文半粗斜体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                       anchor='w')
label_ensbi.place(relx=0.355, rely=0.55, relheight=0.128, anchor='w')
button_mof_ensbi = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                             state='disabled')
button_mof_ensbi.place(relx=0.50, rely=0.55, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_ensbi['command'] = EnSBISelect

label_enbi = tk.Label(Group1, text='西文粗斜字体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                      anchor='w')
label_enbi.place(relx=0.355, rely=0.7, relheight=0.128, anchor='w')
button_mof_enbi = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                            state='disabled')
button_mof_enbi.place(relx=0.50, rely=0.7, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enbi['command'] = EnBISelect

label_enbli = tk.Label(Group1, text='西文特粗斜体：', font=('Microsoft YaHei', 16), fg='#AAAAAA', justify='left',
                       anchor='w')
label_enbli.place(relx=0.355, rely=0.85, relheight=0.128, anchor='w')
button_mof_enbli = tk.Button(Group1, text='选择字体', border=2, font=('Microsoft YaHei', 16), relief='groove',
                             state='disabled')
button_mof_enbli.place(relx=0.50, rely=0.85, relwidth=0.12, relheight=0.128, anchor='w')
button_mof_enbli['command'] = EnBlISelect

button_mstart = tk.Button(Group1, text='开始制作', border=2, font=('Microsoft YaHei', 16), relief='groove', state='disabled')
button_mstart.place(relx=0.81, rely=0.78, relwidth=0.12, relheight=0.128, anchor='center')
button_mstart['command'] = MultiFileModify
# endregion

# region # Tips组件
label = tk.Label(Group1, text="abcd.efgh", bg='#ffffff', justify='left', font=('Microsoft YaHei', 16), padx=offset/16, pady=offset/16)
label.config(relief='groove', borderwidth=2)


def HideTips():
    label.place(relx=2, rely=2)
    label['text'] = ""


def ZhReSelectedHint():
    if selected_font[0] is not None:
        label.place(relx=0.69, rely=0.14, anchor='nw')
        label['text'] = os.path.basename(selected_font[0])


def ZhBdSelectedHint():
    if selected_font[1] is not None:
        label.place(relx=0.69, rely=0.29, anchor='nw')
        label['text'] = os.path.basename(selected_font[1])


def ZhLtSelectedHint():
    if selected_font[2] is not None:
        label.place(relx=0.69, rely=0.44, anchor='nw')
        label['text'] = os.path.basename(selected_font[2])


def EnVaSelectedHint():
    if selected_font[15] is not None:
        label.place(relx=0.69, rely=0.59, anchor='nw')
        label['text'] = os.path.basename(selected_font[15])


def EnReSelectedHint():
    if selected_font[3] is not None:
        label.place(relx=0.03, rely=0.44, anchor='nw')
        label['text'] = os.path.basename(selected_font[3])


def EnRISelectedHint():
    if selected_font[4] is not None:
        label.place(relx=0.36, rely=0.44, anchor='nw')
        label['text'] = os.path.basename(selected_font[4])


def EnSBSelectedHint():
    if selected_font[5] is not None:
        label.place(relx=0.03, rely=0.59, anchor='nw')
        label['text'] = os.path.basename(selected_font[5])


def EnSBISelectedHint():
    if selected_font[6] is not None:
        label.place(relx=0.36, rely=0.59, anchor='nw')
        label['text'] = os.path.basename(selected_font[6])


def EnBdSelectedHint():
    if selected_font[7] is not None:
        label.place(relx=0.03, rely=0.74, anchor='nw')
        label['text'] = os.path.basename(selected_font[7])


def EnBISelectedHint():
    if selected_font[8] is not None:
        label.place(relx=0.36, rely=0.74, anchor='nw')
        label['text'] = os.path.basename(selected_font[8])


def EnLtSelectedHint():
    if selected_font[9] is not None:
        label.place(relx=0.03, rely=0.14, anchor='nw')
        label['text'] = os.path.basename(selected_font[9])


def EnLISelectedHint():
    if selected_font[10] is not None:
        label.place(relx=0.36, rely=0.14, anchor='nw')
        label['text'] = os.path.basename(selected_font[10])


def EnSLSelectedHint():
    if selected_font[11] is not None:
        label.place(relx=0.03, rely=0.29, anchor='nw')
        label['text'] = os.path.basename(selected_font[11])


def EnSLISelectedHint():
    if selected_font[12] is not None:
        label.place(relx=0.36, rely=0.29, anchor='nw')
        label['text'] = os.path.basename(selected_font[12])


def EnBlSelectedHint():
    if selected_font[13] is not None:
        label.place(relx=0.03, rely=0.89, anchor='nw')
        label['text'] = os.path.basename(selected_font[13])


def EnBlISelectedHint():
    if selected_font[14] is not None:
        label.place(relx=0.36, rely=0.89, anchor='nw')
        label['text'] = os.path.basename(selected_font[14])


label_zhr.bind('<Enter>', lambda event: ZhReSelectedHint())
label_zhb.bind('<Enter>', lambda event: ZhBdSelectedHint())
label_zhl.bind('<Enter>', lambda event: ZhLtSelectedHint())
label_enVar.bind('<Enter>', lambda event: EnVaSelectedHint())
label_enr.bind('<Enter>', lambda event: EnReSelectedHint())
label_enri.bind('<Enter>', lambda event: EnRISelectedHint())
label_ensb.bind('<Enter>', lambda event: EnSBSelectedHint())
label_ensbi.bind('<Enter>', lambda event: EnSBISelectedHint())
label_enb.bind('<Enter>', lambda event: EnBdSelectedHint())
label_enbi.bind('<Enter>', lambda event: EnBISelectedHint())
label_enl.bind('<Enter>', lambda event: EnLtSelectedHint())
label_enli.bind('<Enter>', lambda event: EnLISelectedHint())
label_ensl.bind('<Enter>', lambda event: EnSLSelectedHint())
label_ensli.bind('<Enter>', lambda event: EnSLISelectedHint())
label_enbl.bind('<Enter>', lambda event: EnBlSelectedHint())
label_enbli.bind('<Enter>', lambda event: EnBlISelectedHint())

label_zhr.bind('<Leave>', lambda event: HideTips())
label_zhb.bind('<Leave>', lambda event: HideTips())
label_zhl.bind('<Leave>', lambda event: HideTips())
label_enVar.bind('<Leave>', lambda event: HideTips())
label_enr.bind('<Leave>', lambda event: HideTips())
label_enri.bind('<Leave>', lambda event: HideTips())
label_ensb.bind('<Leave>', lambda event: HideTips())
label_ensbi.bind('<Leave>', lambda event: HideTips())
label_enb.bind('<Leave>', lambda event: HideTips())
label_enbi.bind('<Leave>', lambda event: HideTips())
label_enl.bind('<Leave>', lambda event: HideTips())
label_enli.bind('<Leave>', lambda event: HideTips())
label_ensl.bind('<Leave>', lambda event: HideTips())
label_ensli.bind('<Leave>', lambda event: HideTips())
label_enbl.bind('<Leave>', lambda event: HideTips())
label_enbli.bind('<Leave>', lambda event: HideTips())
# endregion

button_m = [button_mof_zhr, button_mof_zhb,  button_mof_zhl,
            button_mof_enr, button_mof_enri, button_mof_ensb, button_mof_ensbi, button_mof_enb,  button_mof_enbi,
            button_mof_enl, button_mof_enli, button_mof_ensl, button_mof_ensli, button_mof_enbl, button_mof_enbli,
            button_mof_enVar]

label_m = [label_zhr, label_zhb,  label_zhl,
           label_enr, label_enri, label_ensb, label_ensbi, label_enb,  label_enbi,
           label_enl, label_enli, label_ensl, label_ensli, label_enbl, label_enbli,
           label_enVar]

# root.protocol("WM_DELETE_WINDOW", os._exit(0))
root.mainloop()

# pyinstaller -F -w --distpath ../Program --workpath ./build main.py
