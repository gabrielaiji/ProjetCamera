using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPath : MonoBehaviour
{
    public enum PathTypes {linear, loop, unique_use}

    public PathTypes pathType;
    public int movementDirection = 1; //sens horaire : 1 ; sens trigo : -1
    public int movingTo = 0;
    public GameObject[] PathSequence;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public MovementPath(List<GameObject> path){
        PathSequence = path.ToArray();
        pathType = PathTypes.unique_use;
    }

    public List<GameObject> getPathList(){
        List<GameObject> list = new List<GameObject>();
        list.AddRange(PathSequence);
        return list;
    }

    public void setPath(List<GameObject> path){
        path.Reverse();
        PathSequence = path.ToArray();
        pathType = PathTypes.unique_use;
    }

    //Draws lines between our points in the Unity Editor
    public void OnDrawGizmos() {

        int length = PathSequence.Length;
        
        // if no lines to draw, do nothing
        if(PathSequence == null || length < 2){
            return;
        }

        for(int i=1; i < length; i++){
            //Draw lines
            Gizmos.DrawLine(PathSequence[i-1].transform.position, PathSequence[i].transform.position);
        }

        if(pathType == PathTypes.loop){
            Gizmos.DrawLine(PathSequence[0].transform.position, PathSequence[length-1].transform.position);
        }
    }


    public IEnumerator<Transform> GetNextPathPoint(){
        int length = PathSequence.Length;
        
        // if no points, do nothing
        if(PathSequence == null || length < 1){
            yield break;
        }

        while(true){

            //Return the current point in PathSequence
            //and wait for next call of enumerator (prevents infinite loop)
            yield return PathSequence[movingTo].transform;


            //If there is only one point exit the coroutine
            if(length == 1){
                continue;
            }
            

            //If linear path move from start to end, then end to start, and repeat
            if(pathType == PathTypes.linear || pathType == PathTypes.unique_use){

                //If you are at the begining of the path
                if(movingTo <= 0){
                    movementDirection = 1; // Setting to 1 moves forward
                }

                //else if you are at the end of your path
                else if(movingTo >= length-1){

                    //if unique_use stop
                    if(pathType == PathTypes.unique_use){
                        continue;
                    }

                    movementDirection = -1; //Setting to -1 moves backwards
                }
            }
            
            //Change movingTo, forward or backwards
            movingTo = movingTo + movementDirection;

            //If looping path, move from start to end, then jump from end to start, and repeat
            if(pathType == PathTypes.loop){
                //If reached last point, moving forward
                if(movingTo >= length){
                    //Set the next point to the start
                    movingTo = 0;
                }

                //If reached first point, moving backwards
                if(movingTo < 0){
                    movingTo = length -1;
                }
            }


        }

    }

}
