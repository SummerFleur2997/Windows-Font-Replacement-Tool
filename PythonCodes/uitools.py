import subprocess
from tkinter import filedialog
from tkinter import messagebox


def FileOpen(types=None):
    if types is None:
        types = [('所有支持的字体文件', '.ttf .otf')]
    font_path = filedialog.askopenfilename(title='选择待替换字体文件', filetypes=types)
    if font_path != '':
        return font_path


def FilesOpen():
    fonts_path = filedialog.askopenfilenames(title='选择待替换字体文件', filetypes=[('所有支持的字体文件', '.ttf .otf')])
    if fonts_path != '':
        return fonts_path


def CopyGitHubLink():
    subprocess.run('echo https://github.com/SummerFleur2997/Windows-Font-Replacement-Tool/issues | clip', shell=True)
    messagebox.showinfo(title="提示", message=" 链接已复制至剪贴板，\n 请前往浏览器粘贴访问")


if __name__ == "__main__":
    pass
