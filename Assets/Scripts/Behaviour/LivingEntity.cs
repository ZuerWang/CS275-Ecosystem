using UnityEngine;

public class LivingEntity : MonoBehaviour {
    const float consumeSpeed = 16;//originally 8
    public float currentHp;
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
        currentHp = genes.maxHp;
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
        if (currentHp > 0) {
            float hpConsumed = Mathf.Max (0, Mathf.Min (currentHp, amount));
            currentHp -= amount * consumeSpeed;
            //amountRemaining -= amountConsumed;
            transform.localScale = Vector3.one * currentHp;

            if (currentHp <= 0) {
                if (species != (Species) (1 << 1)) {
                    Die (CauseOfDeath.Eaten);
                } 
            }
            return hpConsumed;
        }
        return 0;
    }
}