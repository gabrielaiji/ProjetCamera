using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    public MovementPath path;
    public float speed = 1; // La vitesse de deplacement de cet objet
    public float MaxDistanceToGoal = .1f; // La distance a laquelle on veut qu'on considere qu'on a atteint le point

    private IEnumerator<Transform> pointInPath; //Utilise pour referencer les points retournes par paht.GetNextPathPoint

    public bool onGameStart = false;
    private bool enabled = false;


    // Start is called before the first frame update
    void Start()
    {
        if(onGameStart){
            initialize();
            enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(enabled){

            // On verifie s'il y a un chemin assigne et si
            // le chemin est non nul
            if(pointInPath == null || pointInPath.Current == null){
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, pointInPath.Current.position, Time.deltaTime * speed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, pointInPath.Current.rotation, Time.deltaTime * speed*10);

            // On verifie si on est assez proche du point suivant, pour passer
            // au suivant
            var distanceSquared = (transform.position - pointInPath.Current.position).sqrMagnitude;

            if( distanceSquared < MaxDistanceToGoal*MaxDistanceToGoal){
                pointInPath.MoveNext();
            }
        }
    }

    public void initialize(){
        
        // On verifie s'il y a un chemin assigne et si
        // le chemin est non nul
        if(path == null){
            Debug.LogError("No path assigned to follow", gameObject);
            return;
        }

        
        // On cree une reference vers une instance de la corouttine GetNextPathPoint
        pointInPath = path.GetNextPathPoint();

        // On recupere le point suivant (ici le premier point)
        pointInPath.MoveNext();

        // On verifie s'il y a bien un point vers lequel se deplacer
        if(pointInPath.Current == null){
            Debug.LogError("No points to follow", gameObject);
            return;
        }

        // On place l'objet actuel à la position du premier point
        transform.position = pointInPath.Current.position;
        enabled = true;
    }

    public void setMovementPath(MovementPath path){
        this.path = path;
    }
}
