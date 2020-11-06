using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public Transform path;
    public float speed = 0.2f;
    public float sensorLength = 15.0f;
    public GameObject initialNode;

    private List<Transform> nodes;
    private int currentNode;

    private void Start()
    {
        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for(int i = 0; i < pathTransform.Length; i++){
            if(pathTransform[i] != path.transform){
                nodes.Add(pathTransform[i]);

                if(initialNode.transform == pathTransform[i])
                    currentNode = i-1;
            }
        }
    }

    private void FixedUpdate(){
        Drive();
        CheckWaypointDistance();
    }

    private void Drive(){

        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;

        sensorStartPos.y += 0.15f;

        if(!Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength) && !hit.collider || (hit.collider && hit.collider.gameObject.tag != "Car" && hit.collider.gameObject.tag != "Block") ){
            transform.LookAt(nodes[currentNode]);
            transform.Translate(Vector3.forward * speed);
        }
        else{
            Debug.DrawLine(transform.position, hit.point, Color.red);
        }

        Debug.DrawLine(transform.position, nodes[currentNode].position);
    }

    private void CheckWaypointDistance(){
        if(Vector3.Distance(transform.position, nodes[currentNode].position) < .5f){
            if(currentNode == nodes.Count - 1){
                currentNode = 0;
            }
            else{
                currentNode++;
            }
        }
    }
}
