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
    population = float(tokens[1])
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

for species in logs:
    if species != "Plant":
        logs[species]["Birth Rate"] = [1]
        logs[species]["Death Rate"] = [1]
        for i in range (len(indices)-1):
            logs[species]["Birth Rate"].append(logs[species]["Birth"][i+1]/logs[species]["population"][i])
            logs[species]["Death Rate"].append(logs[species]["Death"][i+1]/logs[species]["population"][i])

max_timestep = -1
for key in list(logs["Rabbit"].keys()):
    for species in logs:
        if species != "Plant" or key == "population":
            plt.plot(indices[:max_timestep], logs[species][key][:max_timestep], species_to_color[species])
    plt.xlabel('time')        
    plt.ylabel(key)
    plt.savefig(key+'.png')
    plt.show()
    