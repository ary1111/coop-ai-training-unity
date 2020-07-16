using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

namespace Learning
{
    public class CNAgent1 : Agent
    {
        public float speed = 1;
        public float maxTime = 60;

        public Transform InstrumentTableTransform;
        public GameObject Tool1;
        public GameObject Tool2;
        public GameObject Tool3;
        public GameObject Tool4;
        public GameObject Tool5;

        public bool HasTool1=false;
        public bool HasTool2=false;
        public bool HasTool3=false;
        public bool HasTool4=false;
        public bool HasTool5=false;

        private Rigidbody _rigidbody;

        private float time;
        private List<Vector3> ToolPositions;

        // Start is called before the first frame update
        void Start()
        {
            //Tools 1, 3 and 5 are only used as position holders in this version
            ToolPositions = new List<Vector3>();
            ToolPositions.Add(Tool1.transform.localPosition);
            ToolPositions.Add(Tool2.transform.localPosition);
            ToolPositions.Add(Tool3.transform.localPosition);
            ToolPositions.Add(Tool4.transform.localPosition);
            ToolPositions.Add(Tool5.transform.localPosition);

            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnEpisodeBegin()
        /*Called to setup the environment for a new episode. This method sets the agent & environment
         states when the episode ends*/
        {
            //Place the Agent somewhere random in the OR
            this.transform.localPosition = new Vector3(UnityEngine.Random.value * 5.5f - 2, 0.5f, UnityEngine.Random.value * 6 - 1);

            //Shuffle the tool positions
            ShuffleList.Shuffle(ToolPositions);
            Tool1.transform.localPosition = ToolPositions[0];
            Tool2.transform.localPosition = ToolPositions[1];
            Tool3.transform.localPosition = ToolPositions[2];
            Tool4.transform.localPosition = ToolPositions[3];
            Tool5.transform.localPosition = ToolPositions[4];

            //None of the penalty tools are activated
            Tool2.SetActive(true);
            Tool4.SetActive(true);

            HasTool1 = false;
            HasTool2 = false;
            HasTool3 = false;
            HasTool4 = false;
            HasTool5 = false;

            time = maxTime;
        }

        public override void CollectObservations(VectorSensor sensor)
        /* Called to send information collected to the neural network
           OBSERVATIONS: 12 
        */
        {
            sensor.AddObservation(InstrumentTableTransform.localPosition);
            sensor.AddObservation(Tool2.transform.localPosition);
            sensor.AddObservation(Tool4.transform.localPosition);

            sensor.AddObservation(this.transform.localPosition);
        }

        public override void OnActionReceived(float[] vectorAction)
        /* Called to apply actions to the agent and reward the agent
           ACTIONS: 2 
        */
        {
            //Move the agent kinematically          
            float x = vectorAction[0];
            float z = vectorAction[1];
            Vector3 deltaPos = new Vector3(vectorAction[0], 0, vectorAction[1])*0.1f;
            _rigidbody.MovePosition(transform.position + deltaPos);          

            time = time - 0.01f;

            if(time < 0)
            {
                SetReward(0f);
                EndEpisode();
            }

            if(transform.position.y <0)
            {
                SetReward(0f);
                EndEpisode();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "InstrumentTable")
            {
                if (HasTool2 && HasTool4)
                {
                    AddReward(100f);
                    AddReward((-1*maxTime + time) / 4);
                    EndEpisode();
                }                
            }

            else if (collision.gameObject.name == "Tool1")
            {
                Tool1.SetActive(false);
                HasTool1 = true;
            }

            else if (collision.gameObject.name == "Tool2")
            {
                Tool2.SetActive(false);
                HasTool2 = true;
            }

            else if (collision.gameObject.name == "Tool3")
            {
                Tool3.SetActive(false);
                HasTool3 = true;
            }

            else if (collision.gameObject.name == "Tool4")
            {
                Tool4.SetActive(false);
                HasTool4 = true;
            }

            else if (collision.gameObject.name == "Tool5")
            {
                Tool5.SetActive(false);
                HasTool5 = true;
            }
        }

        public override void Heuristic(float[] actionsOut)
        {
            actionsOut[0] = Input.GetAxis("Horizontal");
            actionsOut[1] = Input.GetAxis("Vertical");
        }
    }
}
