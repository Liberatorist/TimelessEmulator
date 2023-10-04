from io import BytesIO
import json
import os
import zipfile

directory = os.getcwd()
output_dir = os.path.join(directory, "TimelessEmulator", "Build", "Output", "Debug")

with open(os.path.join(output_dir, "data", "stats.json"), "r") as file:
    data = json.loads(file.read())
    with open(os.path.join(output_dir, "TimelessJewels", "stats.txt"), "w") as file:
        for stat in data:
            file.write(f'{stat["Id"]}\n')


zip_buffer = BytesIO()
with zipfile.ZipFile(zip_buffer, "a",
                     zipfile.ZIP_DEFLATED, False) as zip_file:


    for subdir in os.listdir(os.path.join(output_dir, "TimelessJewels")):
        if subdir.endswith(".txt"):
            zip_file.writestr(subdir, open(os.path.join(output_dir, "TimelessJewels", "stats.txt"), "rb").read())
        elif "." not in subdir:
            inner_zip_buffer = BytesIO()
            sub_path = os.path.join(output_dir, "TimelessJewels", subdir)
            with zipfile.ZipFile(inner_zip_buffer, "a",
                     zipfile.ZIP_DEFLATED, False) as inner_zip_file:

                for filename in os.listdir(sub_path):
                    with open(os.path.join(sub_path, filename), "rb") as f:
                        filePath = os.path.join(sub_path, filename)
                        inner_zip_file.writestr(filename, open(filePath, "rb").read())
            
            zip_file.writestr(subdir + ".zip", inner_zip_buffer.getvalue())




with open(os.path.join(directory, "TimelessEmulator", "Build", "Output", "TimelessJewels", "TimelessJewels.zip"), "wb") as file:
    file.write(zip_buffer.getvalue())
