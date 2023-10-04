# This is my fork to generate Timeless Jewel data as readily available .csv files

# Making sure to grab current passive tree data:

- Go to https://snosme.github.io/poe-dat-viewer/
- Import the files for the current patch
- Export data/passiveskills.dat64 and data/stats.dat64 as json and put them into Content/data (rename to passive_skills.json)

# To update the jewel data

- Run Program.cs in debug mode
- Run archiver.py
- push to upstream
