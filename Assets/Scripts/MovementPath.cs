using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPath : MonoBehaviour
{
    public enum PathTypes {linear, loop, unique_use}

    private PathTypes pathType;
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
        //path.Reverse();
        PathSequence = path.ToArray();
        pathType = PathTypes.unique_use;
    }

    public void setPathType(PathTypes pathType){
        this.pathType = pathType;
    }

    //Permet de tracer les lignes entres les points dans l'editeur Unity
    public void OnDrawGizmos() {

        int length = PathSequence.Length;
        
        // Si vide ou un seul point, ne rien faire
        if(PathSequence == null || length < 2){
            return;
        }

        for(int i=1; i < length; i++){
            // On trace la ligne
            Gizmos.DrawLine(PathSequence[i-1].transform.position, PathSequence[i].transform.position);
        }

        if(pathType == PathTypes.loop){
            // Si il s'agit d'une boucle on peut trace un trait entre le dernier et le premier point
            Gizmos.DrawLine(PathSequence[0].transform.position, PathSequence[length-1].transform.position);
        }
    }


    public IEnumerator<Transform> GetNextPathPoint(){
        int length = PathSequence.Length;
        
        // Si pas de point, ne rien faire
        if(PathSequence == null || length < 1){
            yield break;
        }

        while(true){

            // Retourne le point courant dans la PathSequence
            // et attend pour le prochain appel de l'enumerateur (pour empecher des boucles infini)
            yield return PathSequence[movingTo].transform;


            //S'il y a un seul point on sort de la coroutine
            if(length == 1){
                continue;
            }
            

            // Si le mouvement est linéaire on va du début à la fin, puis de la fin au début
            // et on repete
            // Si le mouvement est d'utilisation unique, on va juste une fois, du début
            // a la fin.
            if(pathType == PathTypes.linear || pathType == PathTypes.unique_use){

                // Si on est au point 0
                if(movingTo <= 0){
                    movementDirection = 1; // On renvoie le point 1 (le 2e)
                }

                // Si on a la fin du chemin et le mouvement est lineaire
                // on change de direction
                else if(movingTo >= length-1){

                    // Si utilisation unique on arrete
                    if(pathType == PathTypes.unique_use){
                        continue;
                    }

                    movementDirection = -1; //On change la direction
                }
            }
            
            // On change le point vers lequel il faut maintenant se diriger
            movingTo = movingTo + movementDirection;

            // Si le mouvement est une boucle, on va du debut a la fin, et on recommence
            if(pathType == PathTypes.loop){
                // Si on atteint le dernier point
                if(movingTo >= length){
                    //On recommence le mouvement
                    movingTo = 0;
                }

                // Pour un peu de robustesse
                if(movingTo < 0){
                    movingTo = length -1;
                }
            }


        }

    }

}
