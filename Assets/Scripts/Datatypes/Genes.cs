using static System.Math;
using NumSharp;

public class Genes {

    const float mutationChance = .1f;
    const float maxMutationAmount = .3f;
    static readonly System.Random prng = new System.Random ();

    public bool isMale;
    //public readonly float[] values;
    public bool isCarnivore;
    public float size;
    public float speed;
    public float aggressiveness;
    public float colorR;
    public float colorG;
    public float colorB;

    public int inputSize = 5;
    public int outputSize = 5;
    public NDArray weights;

    public float consumptionRate;

    // constructor
    public Genes (bool isMale, bool isCarnivore, float size, float speed, float aggressiveness, int inputSize, int outputSize, NDArray weights) {
        this.isMale = isMale;
        this.isCarnivore = isCarnivore;
        this.size = size;
        this.speed = speed;
        this.aggressiveness = aggressiveness;
        this.inputSize = inputSize;
        this.outputSize = outputSize;
        this.weights = weights;
        float[] colors = getColor();
        this.colorR = colors[0];
        this.colorG = colors[1];
        this.colorB = colors[2];
        this.consumptionRate = speed/1.5f;
    }

    // get random genes
    public static Genes RandomGenes (int outputSize, int inputSize) {
        bool isMale = RandomValue () < 0.5f;
        bool isCarnivore = RandomValue () < 0.5f;
        float size = RandomValue ();
        float speed = 1.5f + 0.1f * RandomGaussian ();
        float aggressiveness = RandomValue ();
        NDArray weights = np.random.rand((outputSize, inputSize))-0.5;
        return new Genes (isMale, isCarnivore, size, speed, aggressiveness, inputSize, outputSize, weights);
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
        this.speed += 0.1f * RandomGaussian ();
        for (int i = 0; i < this.weights.shape[0]; i++) 
        {
            for (int j = 0; j < this.weights.shape[1]; j++) 
            {
                if (RandomValue () < mutationChance){
                    this.weights[i,j] += maxMutationAmount * RandomGaussian ();
                }
            }
        }
        this.consumptionRate = speed/1.5f;
    }

    public void inheritGenes(Genes g1, Genes g2) {
        this.isMale = RandomValue () < 0.5f;
        this.isCarnivore = g1.isCarnivore;
        this.size = (g1.size+g2.size)/2;
        this.speed = (g1.speed+g2.speed)/2;
        this.aggressiveness = (g1.aggressiveness+g2.aggressiveness)/2;
        this.inputSize = g1.inputSize;
        this.outputSize = g1.outputSize;
        this.weights = (g1.weights+g2.weights)/2;
        float[] colors = getColor();
        this.colorR = colors[0];
        this.colorG = colors[1];
        this.colorB = colors[2];
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