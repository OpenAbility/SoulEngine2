#!/bin/sh

file_name=$1
json_file=tmp/font.json
png_file=tmp/font.png
zip_file=${file_name%.png}.sfont

rm -rf tmp
mkdir -p tmp

cp license.txt tmp/license.txt

wine msdf-atlas-gen.exe -font $file_name -format png -json $json_file -imageout $png_file -glyphset allglyphs.txt -type mtsdf

python gen-font-data.py $file_name

pushd tmp
7z a -tzip ../$zip_file

popd

rm -rf tmp