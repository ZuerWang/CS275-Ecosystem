using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rabbit : Animal {
    float amountRemaining = 1;
    const float consumeSpeed = 1;

    public static readonly string[] GeneNames = { "A", "B" };

    public float Consume (float amount) {
        float amountConsumed = Mathf.Max (0, Mathf.Min (amountRemaining, amount));
        amountRemaining -= amount * consumeSpeed;

        transform.localScale = Vector3.one * amountRemaining;

        if (amountRemaining <= 0) {
            Die (CauseOfDeath.Eaten);
        }

        return amountConsumed;
    }

    public float AmountRemaining {
        get {
            return amountRemaining;
        }
    }
}