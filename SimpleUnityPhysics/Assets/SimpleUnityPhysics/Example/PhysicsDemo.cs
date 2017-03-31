using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUnityPhysics;

public class PhysicsDemo : MonoBehaviour {

    public SimplePhysics physics;

	void Start ()
    {
        timeAtLastUpdate = Time.time;
    }


    float timeAtLastUpdate;

	void Update () {
        if (Time.time - timeAtLastUpdate >= physics.dt)
        {
            physics.StepSimulation(SimplePhysics.SimulationType.Interactable);
            timeAtLastUpdate = Time.time;
        }
    }
}
