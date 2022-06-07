import os

def sumTuples(matrices):
    res = []
    partRes = []
    for matrix in matrices:
        partRes = [0.0 for i in range(2)]
        for row in matrix:
            partRes[0] += row[0]
            partRes[1] += row[1]
            partRes[0] += row[2]
            partRes[1] += row[3]
        if matrix[0][0] == 0.0 or matrix[0][2] == 0.0:
            partRes[0] /= 20
            partRes[1] /= 20
        else:
            partRes[0] /= 40
            partRes[1] /= 40
        res.append(tuple(partRes))
    return tuple(res)


f = open(os.getcwd() + "\\results.txt", 'r')
raw = f.readlines()
f.close()
i = 0
collisions = []
types = []
ratios = []

#print(collisions, types, ratios)
typeLine = []
ratioLine = []
for line in raw:
    i += 1
    if i % 28 == 2:
        collisions.append(tuple([int(s) for s in line.replace(",", "").split() if s.isdigit()]))
    elif i % 28 == 4:
        typeLine = [line.split()[1]]
    elif i % 28 == 5:
        typeLine.append(line.split()[1])
    elif i % 28 == 6:
        typeLine.append(line.split()[1])
        types.append(tuple(typeLine))
    elif i % 28 == 8:
        ratioLine = [tuple([float(s) for s in line.replace(",", "").split()[2:]])]
    elif i % 28 > 8 and i % 28 < 27:
        ratioLine.append(tuple([float(s) for s in line.replace(",", "").split()[2:]]))
    elif i % 28 == 27:
        ratioLine.append(tuple([float(s) for s in line.replace(",", "").split()[2:]]))
        ratios.append(tuple(ratioLine))


#print(collisions)
#print(types)
#print(ratios)

s = ""
#Code for types:
s += "types = ["
for tup in types:
    s += str(tup)[1:-1].replace("'", "\"") + "; "
s = s[:-2] + "];\n"

#Code for collisions. Bars of different methods, on the same scene next to each other.
s += "collisions = ["
for tup in collisions:
    s += str(tup[:-1])[1:-1] + "; "
s = s[:-2] + "];"

print(s)

processedRatios = sumTuples(ratios)
for line in processedRatios:
    print(line)
