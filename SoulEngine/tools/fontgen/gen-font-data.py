import freetype
import sys

path = sys.argv[::-1][0]
print("Generating glyph indices for font " + path)
face = freetype.Face(path)

pairs = []

for (char, index) in face.get_chars():
    pairs.append('"' + str(int(char)) + '"' + ": " + str(index))

result = '{' + ','.join(pairs) + "}"

out = open("tmp/map.json", "w+")
out.write(result)
out.close()