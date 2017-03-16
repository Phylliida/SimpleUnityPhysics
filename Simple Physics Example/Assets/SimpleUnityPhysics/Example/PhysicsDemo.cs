using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleUnityPhysics;

public class PhysicsDemo : MonoBehaviour {

    public SimplePhysics physics;
	// Use this for initialization
	void Start ()
    {
        timeAtLastUpdate = Time.time;
    }


    float timeAtLastUpdate;
	// Update is called once per frame
	void Update () {
        if (Time.time - timeAtLastUpdate >= physics.dt)
        {
            physics.StepSimulation(SimplePhysics.SimulationType.Visable);
            timeAtLastUpdate = Time.time;
        }
    }
}
