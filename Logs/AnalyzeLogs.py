import matplotlib.pyplot as plt

filename = './Census.txt'
logfile = open(filename, 'r')
start = logfile.readline()
Lines = logfile.readlines()
logfile.close()
  
logs = {}
species_to_color = {'Rabbit':'b', 'Plant':'g', 'Fox':'r', 'Eagle_Elite':'k'}


for line in Lines:
    tokens = line.split()
    species = tokens[0]
    population = int(tokens[1])
    if species not in logs:
        logs[species] = {"population":[population]}
    else:
        logs[species]["population"].append(population)
    for i in range(2, len(tokens), 2):
        if tokens[i] not in logs[species]:
            logs[species][tokens[i]] = [float(tokens[i+1])]
        else:
            logs[species][tokens[i]].append(float(tokens[i+1]))

indices = [i for i in range(len(logs["Rabbit"]["population"]))]

print(list(logs["Rabbit"].keys()))

for key in list(logs["Rabbit"].keys()):
    for species in logs:
        if species != "Plant" or key == "population":
            plt.plot(indices, logs[species][key], species_to_color[species])
        
    plt.xlabel('time')        
    plt.ylabel(key)
    plt.show()
    
    