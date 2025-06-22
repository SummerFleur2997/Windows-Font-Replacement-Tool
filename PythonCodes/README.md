## 说明

此目录用于存储字体替换所需要用到的 Python 程序源代码。需要使用 pyinstaller 打包为 .exe 后在命令行中运行，打包命令：

```bash
pyinstaller -w functions.py
```

打包的 .exe 将会被后续 C# 程序调用执行相应的操作。因本人知识有限，因此仅想到了将其打包为命令行程序后在 C# 中调用的方法。

## 依赖项

若您正在使用 PyCharm，那么 IDE 应该会自动识别依赖项。

- Python 3.9 及以上（更低的版本可能可以，但没试过不确定）
- fonttools 版本 4.53.1
- pyinstaller 任意版本

您也可以使用以下命令安装，默认使用清华源：

```bash
pip install fonttools==4.53.1 -i https://pypi.tuna.tsinghua.edu.cn/simple/
```
```bash
pip install pyinstaller -i https://pypi.tuna.tsinghua.edu.cn/simple/
```

# 使用方法

命令行程序的详细使用方法已经在 [functions.py](functions.py) 各个函数的文档注释中详细说明，可详细参考代码。
