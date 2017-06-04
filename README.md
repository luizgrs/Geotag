# Geotag
Add GPS info into photos according to data taken from Google Location History

# Google Location History data

To get your Google Location History data in JSON format use [Google Takeout](https://takeout.google.com)

# How to use
-l, --locationFile    Required. Path of the locations file which will be used

-i, --inputFolder     Required. Path of a folder or a specific file to be
                    processed. Only JPG files are valid

-r, --readOnly        If set no files will be modified or saved

--overwrite           If set allows Geotag to overwrite GPS data in photos
                    which already contains such data

--help                Display this help screen.