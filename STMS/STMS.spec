# -*- mode: python ; coding: utf-8 -*-
from PyInstaller.utils.hooks import collect_all

datas = []
binaries = []
hiddenimports = []

# Collect required libraries
# matplotlib (include data files, but exclude its heavy test suite)
mat_tmp = collect_all('matplotlib')
# Remove test modules from hiddenimports if present
mat_hidden = [h for h in mat_tmp[2] if not h.startswith('matplotlib.tests')]
datas += mat_tmp[0]
binaries += mat_tmp[1]
hiddenimports += mat_hidden

# customtkinter (themes, fonts)
ctk_tmp = collect_all('customtkinter')
datas += ctk_tmp[0]
binaries += ctk_tmp[1]
hiddenimports += ctk_tmp[2]

# pandas (include data files, exclude its test suite)
pd_tmp = collect_all('pandas')
pd_hidden = [h for h in pd_tmp[2] if not h.startswith('pandas.tests')]
datas += pd_tmp[0]
binaries += pd_tmp[1]
hiddenimports += pd_hidden

# Optional: openpyxl for Excel I/O (already pulled via pandas) – no extra action needed

# Exclude large unused packages to shrink size
excludes = [
    'scipy', 'sklearn', 'torch', 'tensorflow', 'keras',
    'IPython', 'jupyter', 'jedi', 'bokeh', 'seaborn',
    'pytest', 'unittest', 'xmlrpc', 'sqlalchemy',
    'matplotlib.tests', 'pandas.tests'
]

a = Analysis(
    ['main.py'],
    pathex=[],
    binaries=binaries,
    datas=datas,
    hiddenimports=hiddenimports,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=excludes,
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='STMS',
    debug=False,
    bootloader_ignore_signals=False,
    strip=True,
    upx=False,  # disabled to avoid missing upx executable
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
