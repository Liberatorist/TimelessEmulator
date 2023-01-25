import json
import os
import zipfile

directory = os.getcwd()

with open(os.path.join(directory, "data", "stats.json"), "r") as file:
    data = json.loads(file.read())
    with open(os.path.join(directory, "TimelessJewels", "stats.txt"), "w") as file:
        for stat in data:
            file.write(f'{stat["Id"]}\n')

for subdir in os.listdir(os.path.join(directory, "TimelessJewels")):
    if "." in subdir:
        continue
    sub_path = os.path.join(directory, "TimelessJewels", subdir)
    with zipfile.ZipFile(os.path.join(directory, "TimelessJewels", f"{subdir}.zip"), compression=zipfile.ZIP_DEFLATED, mode="w") as archive:
        for filename in os.listdir(sub_path):
            if filename.endswith(".csv"):
                filePath = os.path.join(sub_path, filename)
                archive.write(filePath, os.path.basename(filePath))
