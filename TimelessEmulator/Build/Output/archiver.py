import json
import os
import zipfile

directory = os.getcwd()

with open(os.path.join(directory, "data", "stats.json"), "r") as file:
    data = json.loads(file.read())
    with open(os.path.join(directory, "Seeds", "stats.txt"), "w") as file:
        for stat in data:
            file.write(f'{stat["Id"]}\n')

for subdir in os.listdir(os.path.join(directory, "Seeds")):
    if "." in subdir:
        continue
    sub_path = os.path.join(directory, "Seeds", subdir)
    with zipfile.ZipFile(os.path.join(directory, "Seeds", f"{subdir}.zip"), compression=zipfile.ZIP_DEFLATED, mode="w") as archive:
        for filename in os.listdir(sub_path):
            if filename.endswith(".csv"):
                filePath = os.path.join(sub_path, filename)
                archive.write(filePath, os.path.basename(filePath))
