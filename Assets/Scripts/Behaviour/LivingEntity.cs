using UnityEngine;

public class LivingEntity : MonoBehaviour {
    public float amountRemaining = 1;
    const float consumeSpeed = 16;//originally 8

    public int colourMaterialIndex;
    public Species species;
    public Genes genes;
    public Material material;

    public Coord coord;
    //
    [HideInInspector]
    public int mapIndex;
    [HideInInspector]
    public Coord mapCoord;

    protected bool dead;
    public static float minPlantRemaning = 0.1f;

    public virtual void Init (Coord coord) {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];
        genes = Genes.RandomGenes (3,4);
        // Set material to the instance material
        var meshRenderer = transform.GetComponentInChildren<MeshRenderer> ();
        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            if (meshRenderer.sharedMaterials[i] == material) {
                material = meshRenderer.materials[i];
                break;
            }
        }
    }

    protected virtual void Die (CauseOfDeath cause) {
        if (!dead) {
            dead = true;
            Environment.RegisterDeath (this);
            // if (species != (Species) (1 << 1)) {
            //     Destroy (gameObject);
            // }
            Destroy (gameObject);
        }
    }

    // add Consume function from Plant
    public float Consume (float amount) {
        if (amountRemaining > 0) {
            float amountConsumed = Mathf.Max (0, Mathf.Min (amountRemaining, amount));
            amountRemaining -= amount * consumeSpeed;
            //amountRemaining -= amountConsumed;
            transform.localScale = Vector3.one * amountRemaining;

            if (amountRemaining <= 0) {
                if (species != (Species) (1 << 1)) {
                    Die (CauseOfDeath.Eaten);
                } 
            }
            return amountConsumed;
        }
        return 0;
    }

    public float AmountRemaining {
        get {
            return amountRemaining;
        }
    }
}