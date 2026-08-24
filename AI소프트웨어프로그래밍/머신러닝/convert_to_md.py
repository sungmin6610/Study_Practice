import os
import subprocess
import glob
import shutil
import re
from datetime import datetime

source_dir = r"D:\2604340036 - 송성민\머신러닝"
target_dir = r"d:\2604340036 - 송성민\sungmin6610.github.io\_posts"
assets_dir = r"d:\2604340036 - 송성민\sungmin6610.github.io\assets\images\ml"
date_str = "2026-06-18"  # Using current date

os.makedirs(assets_dir, exist_ok=True)

notebooks = glob.glob(os.path.join(source_dir, "*.ipynb"))

links = []

for nb in notebooks:
    filename = os.path.basename(nb)
    name_without_ext = os.path.splitext(filename)[0]
    
    # 1. 선형회귀분석 -> remove number and dot for permalink
    clean_name = re.sub(r'^\d+\.\s*', '', name_without_ext)
    
    permalink_slug = clean_name.replace(" ", "-").replace(".", "").replace("_", "-").lower()
    
    # To keep track of order, let's preserve the original filename for the post name
    safe_file_slug = name_without_ext.replace(" ", "-").replace(".", "").replace("_", "-").lower()
    output_filename = f"{date_str}-ml-{safe_file_slug}.md"
    output_path = os.path.join(target_dir, output_filename)
    
    # Run nbconvert
    print(f"Converting {filename}...")
    # we output to a temp location first or directly to target
    subprocess.run(["jupyter", "nbconvert", "--to", "markdown", nb, "--output", output_filename, "--output-dir", target_dir], check=True)
    
    # Check for image folder
    # nbconvert names the image folder: output_filename_without_ext_files
    # e.g. 2026-06-18-ml-1-선형회귀분석_files
    expected_img_dir = os.path.join(target_dir, f"{output_filename[:-3]}_files")
    
    if os.path.exists(expected_img_dir):
        # Move it to assets_dir
        dest_img_dir = os.path.join(assets_dir, f"{output_filename[:-3]}_files")
        if os.path.exists(dest_img_dir):
            shutil.rmtree(dest_img_dir)
        shutil.move(expected_img_dir, dest_img_dir)
    
    # Read the generated markdown file
    with open(output_path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    # Replace image links if necessary
    # The links in md will be: ![png](2026-06-18-ml-1-선형회귀분석_files/output_...png)
    # We change them to: ![png](/assets/images/ml/2026-06-18-ml-1-선형회귀분석_files/output_...png)
    img_folder_name = f"{output_filename[:-3]}_files"
    content = content.replace(f"]({img_folder_name}/", f"](/assets/images/ml/{img_folder_name}/")
        
    front_matter = f"""---
layout: post
title: "머신러닝: {name_without_ext}"
date: {date_str}
permalink: /ml/{permalink_slug}/
---

"""
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(front_matter + content)
        
    # extract the prefix number for sorting
    match = re.match(r'^(\d+)\.', name_without_ext)
    order = int(match.group(1)) if match else 99
    
    links.append((order, name_without_ext, output_filename.replace('.md', '')))

links.sort(key=lambda x: x[0])

print("\n--- Links for the hub page ---")
for order, name, jekyll_name in links:
    print(f"* [머신러닝: {name}]({{% post_url {jekyll_name} %}})")
print("------------------------------")
