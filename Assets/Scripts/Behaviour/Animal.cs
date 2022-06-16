using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

public class Animal : LivingEntity {

    public const int maxViewDistance = 10;

    [EnumFlags]
    public Species diet;
    public CreatureAction currentAction;
    public Genes partnerGenes;
    public Color maleColour;
    public Color femaleColour;
    public int wantToMate = 0;

    // Settings:
    float timeBetweenActionChoices = 1;
    float timeToDeathByHunger = 200;
    float timeToDeathByThirst = 200;
    float timeToReproduce = 2.5f;
    float reprodHungerCost = 25;
    float reprodThirstCost = 25;

    float drinkDuration = 6;
    float eatDuration = 10;

    float criticalPercent = 0.7f;

    // Visual settings:
    float moveArcHeight = .2f;

    // State:
    [Header ("State")]
    public float hunger;
    public float thirst;
    public float reprod;
    public int newBaby;
    public float lastReproductionTime;

    protected LivingEntity foodTarget;
    protected LivingEntity mateTarget;
    protected Coord waterTarget;

    // Move data:
    bool animatingMovement;
    Coord moveFromCoord;
    Coord moveTargetCoord;
    Vector3 moveStartPos;
    Vector3 moveTargetPos;
    float moveTime;
    float moveSpeedFactor;
    float moveArcHeightFactor;
    Coord[] path;
    int pathIndex;

    // Other
    float lastActionChooseTime;
    const float sqrtTwo = 1.4142f;
    const float oneOverSqrtTwo = 1 / sqrtTwo;

    public override void Init (Coord coord) {
        base.Init (coord);
        moveFromCoord = coord;
        genes = Genes.RandomGenes (3,4);
        lastReproductionTime = Time.time;
        hunger = 0;
        thirst = 0;
        reprod = 0;
        newBaby = 0;
        wantToMate = 0;

        material.color = (genes.isMale) ? maleColour : femaleColour;

        ChooseNextAction ();
    }



    protected virtual void Update () {
        Environment.regeneratePlant();
        Environment.reportPopulation();
        // Increase hunger and thirst over time
        // hunger += genes.consumptionRate * Time.deltaTime * 1 / timeToDeathByHunger;
        // thirst += genes.consumptionRate * Time.deltaTime * 1 / timeToDeathByThirst;
        // reprod += Time.deltaTime * 1 / timeToReproduce;

        // Animate movement. After moving a single tile, the animal will be able to choose its next action
        if (animatingMovement) {
            AnimateMove ();
        } else {
            // Handle interactions with external things, like food, water, mates
            HandleInteractions ();
            float timeSinceLastActionChoice = Time.time - lastActionChooseTime;
            if (timeSinceLastActionChoice > timeBetweenActionChoices*(1/genes.agility)) {
                ChooseNextAction ();
                hunger += genes.consumptionRate * Time.deltaTime * 1 / timeToDeathByHunger;
                thirst += genes.consumptionRate * Time.deltaTime * 1 / timeToDeathByThirst;
                reprod += Time.deltaTime * 1 / timeToReproduce;
            }
        }

        if (hunger >= 1) {
            Die (CauseOfDeath.Hunger);
        } else if (thirst >= 1) {
            Die (CauseOfDeath.Thirst);
        } else if (newBaby == 1){
            //Baby();
            float timeSinceLastRreporduction = Time.time - lastReproductionTime;
            if (timeSinceLastRreporduction > timeToReproduce) {
                lastReproductionTime = Time.time;
                // Debug.Log ("timeSinceLastRreporduction: " + timeSinceLastRreporduction);
                for (int i = 0; i < genes.numBabies; i++) {
                    Environment.RegisterBirth (this);
                    // hunger += genes.consumptionRate * Time.deltaTime * reprodHungerCost / timeToDeathByHunger;
                    // thirst += genes.consumptionRate * Time.deltaTime * reprodThirstCost / timeToDeathByThirst;
                    hunger += genes.consumptionRate * reprodHungerCost / timeToDeathByHunger;
                    thirst += genes.consumptionRate * reprodThirstCost / timeToDeathByThirst;
                }
            } 
            wantToMate = 0;
            newBaby = 0;
        }
    }

    // Animals choose their next action after each movement step (1 tile),
    // or, when not moving (e.g interacting with food etc), at a fixed time interval
    protected virtual void ChooseNextAction () {
        // HardcodeAction ();
        EvolvedAction ();
    }

    protected virtual void EvolvedAction () {
        lastActionChooseTime = Time.time;
        // Get info about surroundings

        // Decide next action by neural net:
        NDArray input = np.zeros((genes.inputSize, 1));
        bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget && hunger > 0;
        input[0,0] = hunger;
        input[1,0] = thirst;
        input[2,0] = currentlyEating ? 0 : 1;
        input[3,0] = reprod;
        NDArray output = genes.getNNOuput(input);
        int choice = np.argmax(output);

        switch (choice) {
            case 0:
                FindFood ();
                break;
            case 1:
                FindWater ();
                break;
            case 2:
                FindMate ();
                break;
        }

        Act ();
    }

    protected virtual void HardcodeAction () {
        lastActionChooseTime = Time.time;
        // Get info about surroundings

        // Decide next action:
        // Eat if (more hungry than thirsty) or (currently eating and not critically thirsty)
        bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget && hunger > 0;
        if (hunger >= thirst || currentlyEating && thirst < criticalPercent) {
            FindFood ();
        }
        // More thirsty than hungry
        else {
            FindWater ();
        }

        Act ();

    }


    protected virtual void FindFood () {
        LivingEntity foodSource = Environment.SenseFood (coord, this, FoodPreferencePenalty);
        if (foodSource) {
            currentAction = CreatureAction.GoingToFood;
            foodTarget = foodSource;
            CreatePath (foodTarget.coord);

        } else {
            currentAction = CreatureAction.Exploring;
        }
    }

    protected virtual void FindWater () {
        Coord waterTile = Environment.SenseWater (coord);
        if (waterTile != Coord.invalid) {
            currentAction = CreatureAction.GoingToWater;
            waterTarget = waterTile;
            CreatePath (waterTarget);

        } else {
            currentAction = CreatureAction.Exploring;
        }
    }

    protected virtual void FindMate () {
        //Debug.Log ("Action: find mate");
        // asexual reproduction
        //currentAction = CreatureAction.AsexualReproduction;

        // bisexual reproduction
        wantToMate = 1;
        List<Animal> mates = Environment.SensePotentialMates(coord, this);
        if (mates.Count > 0) {
            Animal nearestFriend = mates[0];
            currentAction = CreatureAction.SearchingForMate;
            mateTarget = nearestFriend;
            CreatePath (mateTarget.coord);
        } else {
            currentAction = CreatureAction.Exploring;
        }
    }

    // When choosing from multiple food sources, the one with the lowest penalty will be selected
    protected virtual int FoodPreferencePenalty (LivingEntity self, LivingEntity food) {
        return Coord.SqrDistance (self.coord, food.coord);
    }

    protected void Act () {
        switch (currentAction) {
            case CreatureAction.Exploring:
                StartMoveToCoord (Environment.GetNextTileWeighted (coord, moveFromCoord));
                break;
            case CreatureAction.GoingToFood:
                if (Coord.AreNeighbours (coord, foodTarget.coord)) {
                    LookAt (foodTarget.coord);
                    currentAction = CreatureAction.Eating;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.GoingToWater:
                if (Coord.AreNeighbours (coord, waterTarget)) {
                    LookAt (waterTarget);
                    currentAction = CreatureAction.Drinking;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.SearchingForMate:
                if (Coord.AreNeighbours (coord, mateTarget.coord)) {
                    LookAt (mateTarget.coord);
                    currentAction = CreatureAction.Mating;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.AsexualReproduction:
                break;
        }
    }

    protected void CreatePath (Coord target) {
        // Create new path if current is not already going to target
        if (path == null || pathIndex >= path.Length || (path[path.Length - 1] != target || path[pathIndex - 1] != moveTargetCoord)) {
            path = EnvironmentUtility.GetPath (coord.x, coord.y, target.x, target.y);
            pathIndex = 0;
        }
    }

    protected void StartMoveToCoord (Coord target) {
        moveFromCoord = coord;
        moveTargetCoord = target;
        moveStartPos = transform.position;
        moveTargetPos = Environment.tileCentres[moveTargetCoord.x, moveTargetCoord.y];
        animatingMovement = true;

        bool diagonalMove = Coord.SqrDistance (moveFromCoord, moveTargetCoord) > 1;
        moveArcHeightFactor = (diagonalMove) ? sqrtTwo : 1;
        moveSpeedFactor = (diagonalMove) ? oneOverSqrtTwo : 1;

        LookAt (moveTargetCoord);
    }

    protected void LookAt (Coord target) {
        if (target != coord) {
            Coord offset = target - coord;
            transform.eulerAngles = Vector3.up * Mathf.Atan2 (offset.x, offset.y) * Mathf.Rad2Deg;
        }
    }

    void HandleInteractions () {
        if (currentAction == CreatureAction.Eating) {
            if (foodTarget && hunger > 0) {
                float eatAmount = Mathf.Min (hunger, genes.attack * foodTarget.genes.defense *
                                                     Time.deltaTime * 1 / eatDuration);
                eatAmount = (foodTarget).Consume (eatAmount);
                float foodConversionRate = (foodTarget is Animal) ? 0.9f : 0.5f;
                hunger -= foodConversionRate * Environment.resourceLevel * eatAmount;
                hunger = Mathf.Clamp01 (hunger);
            }
        } else if (currentAction == CreatureAction.Drinking) {
            if (thirst > 0) {
                thirst -= Environment.resourceLevel * Time.deltaTime * 1 / drinkDuration;
                thirst = Mathf.Clamp01 (thirst);
            }
        } else if (currentAction == CreatureAction.Mating) {
            if (mateTarget && reprod > 1 && !genes.isMale){
                partnerGenes = ((Animal) mateTarget).genes;
                reprod = 0;
                // do something to clone a copy
                newBaby = 1;
            }
        } else if (currentAction == CreatureAction.AsexualReproduction) {
            reprod = 0;
            Debug.Log ("Error: AsexualReproduction");
            newBaby = 1;
        }
    }

    void AnimateMove () {
        // Move in an arc from start to end tile
        moveTime = Mathf.Min (1, moveTime + Time.deltaTime * genes.speed * moveSpeedFactor);
        float height = (1 - 4 * (moveTime - .5f) * (moveTime - .5f)) * moveArcHeight * moveArcHeightFactor;
        transform.position = Vector3.Lerp (moveStartPos, moveTargetPos, moveTime) + Vector3.up * height;

        // Finished moving
        if (moveTime >= 1) {
            Environment.RegisterMove (this, moveFromCoord, moveTargetCoord);
            coord = moveTargetCoord;

            animatingMovement = false;
            moveTime = 0;
            ChooseNextAction ();
        }
    }

    void OnDrawGizmosSelected () {
        if (Application.isPlaying) {
            var surroundings = Environment.Sense (coord);
            Gizmos.color = Color.white;
            if (surroundings.nearestFoodSource != null) {
                Gizmos.DrawLine (transform.position, surroundings.nearestFoodSource.transform.position);
            }
            if (surroundings.nearestWaterTile != Coord.invalid) {
                Gizmos.DrawLine (transform.position, Environment.tileCentres[surroundings.nearestWaterTile.x, surroundings.nearestWaterTile.y]);
            }

            if (currentAction == CreatureAction.GoingToFood) {
                var path = EnvironmentUtility.GetPath (coord.x, coord.y, foodTarget.coord.x, foodTarget.coord.y);
                Gizmos.color = Color.black;
                for (int i = 0; i < path.Length; i++) {
                    Gizmos.DrawSphere (Environment.tileCentres[path[i].x, path[i].y], .2f);
                }
            }
        }
    }

}