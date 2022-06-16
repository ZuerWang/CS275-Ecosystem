using static System.Math;
using NumSharp;
using System;

public class Genes {

    const float mutationChance = .1f;
    const float maxMutationAmount = .3f;
    static readonly System.Random prng = new System.Random ();

    public bool isMale;
    //public readonly float[] values;
    public bool isCarnivore;
    public float maxHp;
    public float speed;
    public float attack;
    public float defense;
    public float agility;
    public float colorR;
    public float colorG;
    public float colorB;

    public int inputSize = 5;
    public int outputSize = 5;
    public NDArray weights;

    public float consumptionRate;
    public int numBabies;
    static Random rnd = new Random();

    // constructor
    public Genes (bool isMale, bool isCarnivore, float maxHp, float speed, float attack, 
                  int inputSize, int outputSize, NDArray weights, int numBabies, float defense, float agility) {
        this.isMale = isMale;
        this.isCarnivore = isCarnivore;
        this.maxHp = maxHp;
        this.speed = speed;
        this.attack = attack;
        this.defense = defense;
        this.agility = agility;
        this.inputSize = inputSize;
        this.outputSize = outputSize;
        this.weights = weights;
        float[] colors = getColor();
        this.colorR = colors[0];
        this.colorG = colors[1];
        this.colorB = colors[2];
        this.consumptionRate = (speed/1.5f)*attack*defense*maxHp;
        this.numBabies = numBabies;
    }

    // get random genes
    public static Genes RandomGenes (int outputSize, int inputSize) {
        bool isMale = RandomValue () < 0.5f;
        bool isCarnivore = RandomValue () < 0.5f;
        float maxHp = 1f + 0.1f * RandomGaussian ();
        float speed = 1.5f + 0.1f * RandomGaussian ();
        float attack = 1f + 0.1f * RandomGaussian ();
        float defense = 1f + 0.1f * RandomGaussian ();
        float agility = 1f + 0.1f * RandomGaussian ();
        int numBabies = rnd.Next(1, 5);
        NDArray weights = np.random.rand((outputSize, inputSize))-0.5;
        return new Genes (isMale, isCarnivore, maxHp, speed, attack, inputSize, outputSize, weights, numBabies, defense, agility);
    }

    // display genes information
    // public void displayGenes(){
    //     Console.WriteLine($"isMale: {this.isMale}");
    //     Console.WriteLine($"isCarnivore: {this.isCarnivore}");
    //     Console.WriteLine($"size: {this.size}");
    //     Console.WriteLine($"speed: {this.speed}");
    //     Console.WriteLine($"aggressiveness: {this.aggressiveness}");
    //     Console.WriteLine($"colorR: {this.colorR}");
    //     Console.WriteLine($"colorG: {this.colorG}");
    //     Console.WriteLine($"colorB: {this.colorB}");
    //     Console.WriteLine("weights: ");
    //     for (int i = 0; i < this.weights.shape[0]; i++) 
    //     {
    //         for (int j = 0; j < this.weights.shape[1]; j++) 
    //         {
    //             Console.Write($"{this.weights[i,j]} ");
    //         }
    //         Console.WriteLine("");
    //     }
        
    // }

    // current brain structure is a neural network with a single linear layer
    // example inputs: sensory inputs, internal clock...
    // example outputs: move in certain direction, accelerate, rotate, herding desire, 
    // eat desire, reset clock(internal clock) pheromone production, attack, flee...
    public NDArray getNNOuput(NDArray input){
        // single linear layer without activation
        return np.dot(this.weights, input);;
    }

    // randomly mutate genes
    public void mutate(){
        if (RandomValue () < mutationChance) {
            this.speed += 0.1f * RandomGaussian ();
            if (this.speed < 0) {
                this.speed = 1.5f;
            }
        }
        if ((RandomValue () < mutationChance)) {
            this.numBabies += RandomValue () < 0.5f ? -1 : 1;
            if (this.numBabies < 1) {
                this.numBabies = 1;
            }
        }
        if ((RandomValue () < mutationChance)) {
            this.attack += 0.1f * RandomGaussian ();
        }
        if ((RandomValue () < mutationChance)) {
            this.defense += 0.1f * RandomGaussian ();
        }
        if ((RandomValue () < mutationChance)) {
            this.agility= 0.1f * RandomGaussian ();
            if (this.agility <= 0) {
                this.agility = 1;
            }
        }
        if (RandomValue () < mutationChance) {
            this.maxHp += 0.1f * RandomGaussian ();
            if (this.maxHp < 0) {
                this.maxHp = 1f;
            }
        }
        for (int i = 0; i < this.weights.shape[0]; i++) 
        {
            for (int j = 0; j < this.weights.shape[1]; j++) 
            {
                if (RandomValue () < mutationChance){
                    this.weights[i,j] += maxMutationAmount * RandomGaussian ();
                }
            }
        }
        this.consumptionRate = (speed/1.5f)*attack*defense*maxHp;
    }

    public void inheritGenes(Genes g1, Genes g2) {
        this.isMale = RandomValue () < 0.5f;
        this.isCarnivore = g1.isCarnivore;
        this.maxHp = (g1.maxHp+g2.maxHp)/2;
        this.speed = (g1.speed+g2.speed)/2;
        this.attack = (g1.attack+g2.attack)/2;
        this.defense = (g1.defense+g2.defense)/2;
        this.agility = (g1.agility+g2.agility)/2;
        this.inputSize = g1.inputSize;
        this.outputSize = g1.outputSize;
        this.weights = (g1.weights+g2.weights)/2;
        float[] colors = getColor();
        this.colorR = colors[0];
        this.colorG = colors[1];
        this.colorB = colors[2];
        this.numBabies = RandomValue () < 0.5f ? g1.numBabies : g2.numBabies;
        mutate();
    }


    public void saveGene(){
        // TODO: save genes in a file
    }

    public void loadGene(){
        // TODO: load genes from a file
    }

    public float[] getColor(){
        // TODO: compute colors based on genes
        float[] colors = {RandomValue (), RandomValue (), RandomValue ()};
        return colors;
        
    }

    // public static Genes InheritedGenes (Genes mother, Genes father) {
    //     float[] values = new float[mother.values.Length];
    //     // TODO: implement inheritance
    //     Genes genes = new Genes (values);
    //     return genes;
    // }

    static float RandomValue () {
        return (float) prng.NextDouble ();
    }

    public static float RandomGaussian () {
        double u1 = 1 - prng.NextDouble ();
        double u2 = 1 - prng.NextDouble ();
        double randStdNormal = Sqrt (-2 * Log (u1)) * Sin (2 * PI * u2);
        return (float) randStdNormal;
    }
}