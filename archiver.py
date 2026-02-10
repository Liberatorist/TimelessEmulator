from io import BytesIO
import json
import os
import zipfile

directory = os.getcwd()
input_dir = os.path.join(directory, "TimelessEmulator", "Build", "Output", "Debug")
output_dir = os.path.join(directory, "TimelessEmulator", "Build", "Output", "TimelessJewels")
timeless_dir = os.path.join(input_dir, "TimelessJewels")

os.makedirs(timeless_dir, exist_ok=True)

with open(os.path.join(input_dir, "data", "stats.json"), "r") as file:
    data = json.loads(file.read())
    with open(os.path.join(timeless_dir, "stats.txt"), "w") as file:
        for stat in data:
            file.write(f'{stat["Id"]}\n')

for entry in os.listdir(timeless_dir):
    entry_path = os.path.join(timeless_dir, entry)
    if not os.path.isdir(entry_path):
        continue

    zip_path = os.path.join(output_dir, f"{entry}.zip")
    passives_name = f"{entry}_passives.txt"
    passives_path = os.path.join(timeless_dir, passives_name)

    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zip_file:
        if os.path.isfile(passives_path):
            zip_file.write(passives_path, passives_name)

        for filename in os.listdir(entry_path):
            file_path = os.path.join(entry_path, filename)
            if os.path.isfile(file_path):
                zip_file.write(file_path, os.path.join(entry, filename))
