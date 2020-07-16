using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RollerAgent : Agent
{
    public Transform targetTransform;
    public float speed = 10;
    private Rigidbody _rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    /*Called to setup the environment for a new episode. This method sets the agent & environment
     states when the episode ends*/
    {
        if(this.transform.localPosition.y<0)
        {
            //If the Agent is falling, zero it's momentum
            this._rigidbody.angularVelocity = Vector3.zero;
            this._rigidbody.velocity = Vector3.zero;

            //Set the position back to the default
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        //Move target to a new spot
        targetTransform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    /* Called to send information collected to the neural network*/
    {
        //Target and Agent positions
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        //Agent velocity
        sensor.AddObservation(_rigidbody.velocity.x);
        sensor.AddObservation(_rigidbody.velocity.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    /* Called to apply actions to the agent and reward the agent*/
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        _rigidbody.AddForce(controlSignal * speed);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, targetTransform.localPosition);

        if(distanceToTarget <1.42f)
        {
            //Reached target
            SetReward(1.0f);
            EndEpisode();
        }

        if(this.transform.localPosition.y <0)
        {
            //Fell off platform
            EndEpisode();
        }

    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }
}
