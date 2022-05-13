f = open(r"C:\Users\Berendt\AppData\LocalLow\DefaultCompany\Steering\sigmaResults.txt", 'r')
raw = f.readlines()
f.close()
i = 0
numbers = []
for line in raw:
    i += 1
    if i % 27 == 2:
        #print(line, end="")
        numbers.append([int(s) for s in line.replace(",", "").split() if s.isdigit()])
    elif i % 27 == 4 or i % 27 == 5:
        pass
        #print(line, end="")
    elif i % 27 == 0:
        pass
        #print("")

coll = 1000000000
collid = -1
wall = 1000000000
wallid = -1
oob = 1000000000
oobid = -1
all = 1000000000
allid = -1
i = 0
for numberLine in numbers:
    if numberLine[0] < coll:
        coll = numberLine[0]
        collid = i
    elif numberLine[0] == coll:
        if isinstance(collid, int):
            collid = [collid]
        collid.append(i)

    if numberLine[1] < wall:
        wall = numberLine[1]
        wallid = i
    elif numberLine[1] == wall:
        if isinstance(wallid, int):
            wallid = [wallid]
        wallid.append(i)

    if numberLine[2] < oob:
        oob = numberLine[2]
        oobid = i
    elif numberLine[2] == oob:
        if isinstance(oobid, int):
            oobid = [oobid]
        oobid.append(i)

    if sum(numberLine) < all:
        all = sum(numberLine)
        allid = i
    elif sum(numberLine) == all:
        if isinstance(allid, int):
            allid = [allid]
        allid.append(i)

    #print(numberLine)
    i += 1

print("Pedestrians:", collid, ",", coll)
print("Walls:", wallid, ",", wall)
print("OOB:", oobid, ",", oob)
print("All:", allid, ",", all)
