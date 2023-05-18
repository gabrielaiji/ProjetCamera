using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    public enum MovementTypes{MoveTowards, LerpTowards}

    private MovementTypes movementType = MovementTypes.MoveTowards;
    public MovementPath path;
    public float speed = 1; // Speed object is moving
    public float MaxDistanceToGoal = .1f; // How close does it have to be to the point to be considered at point

    private IEnumerator<Transform> pointInPath; //Used to reference points returned from MyPath.GetNextPathPoint

    private bool enabled = false;


    // Start is called before the first frame update
    void Start()
    {
        /*
        initialize();
        */
    }

    // Update is called once per frame
    void Update()
    {
        if(enabled){
            //Check if there is a path with a point in it
            if(pointInPath == null || pointInPath.Current == null){
                return;
            }

            //if moving towards
            if(movementType == MovementTypes.MoveTowards){
                transform.position = Vector3.MoveTowards(transform.position, pointInPath.Current.position, Time.deltaTime * speed);
                //Vector3 angles = pointInPath.Current.rotation.eulerAngles;
                //angles.y +=90;
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(angles), Time.deltaTime * speed*10);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, pointInPath.Current.rotation, Time.deltaTime * speed*10);

            }

            //else if lerping
            else if(movementType == MovementTypes.LerpTowards){
                transform.position = Vector3.Lerp(transform.position, pointInPath.Current.position, Time.deltaTime * speed);
                transform.rotation = Quaternion.Lerp(transform.rotation, pointInPath.Current.rotation, Time.deltaTime * speed);
            }

            //Check if close enough to the next point to start moving to the next step
            // .sqrMagnitude squares
            var distanceSquared = (transform.position - pointInPath.Current.position).sqrMagnitude;

            if( distanceSquared < MaxDistanceToGoal*MaxDistanceToGoal){
                pointInPath.MoveNext();
            }
        }
    }

    public void initialize(){
        //Check if there is a path assigned
        if(path == null){
            Debug.LogError("No path assigned to follow", gameObject);
            return;
        }

        //Sets up a reference to an instance of the coroutine GetNextPathPoint
        pointInPath = path.GetNextPathPoint();

        //Get the next point in the path to move to (here, the first)
        pointInPath.MoveNext();

        //Check if there is a point to move to
        if(pointInPath.Current == null){
            Debug.LogError("No points to follow", gameObject);
            return;
        }

        //Set the pos of this object to the pos of our starting point
        transform.position = pointInPath.Current.position;
        enabled = true;
    }

    public void setMovementPath(MovementPath path){
        this.path = path;
        movementType = MovementTypes.MoveTowards;
    }
}
