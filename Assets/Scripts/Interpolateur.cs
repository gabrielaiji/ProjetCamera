using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interpolateur : MonoBehaviour
{

    public MovementPath path;
    public FollowPath camera;

    public float pas;

    public GameObject pointPrefab;
    public bool display = true;

    private List<float> T;
    private List<float> tToEval;

    private List<GameObject> points_interpoles = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Check if there is a path assigned
        if(path == null){
            Debug.LogError("No path assigned to follow", gameObject);
            return;
        }

        //Check if there is a camera assigned
        if(camera == null){
            Debug.LogError("No Camera assigned to follow", gameObject);
            return;
        }

        foreach(GameObject point in path.getPathList()){
            CameraView camView = point.GetComponent<CameraView>();
            camView.setDisplay(this.display);
        }

        int nbElem = path.PathSequence.Length;
        (T,tToEval) = buildParametrisationTchebycheff(nbElem, pas);

        applyNevilleParametrisation(path.getPathList(), T, tToEval);
        // MovementPath path_interpole = new MovementPath(points_interpoles);
        path.setPath(points_interpoles);

        foreach(GameObject point in points_interpoles){
            CameraView camView = point.GetComponent<CameraView>();
            camView.setDisplay(this.display);
        }
        //camera.setMovementPath(points_interpoles);
        camera.initialize();

    /*
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
        transform.position = pointInPath.Current.position;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //////////////////////////////////////////////////////////////////////////
    // fonction : buildParametrisationTchebycheff                           //
    // semantique : construit la parametrisation basée sur Tchebycheff      //
    //              et les échantillons de temps selon cette parametrisation//
    // params :                                                             //
    //          - int nbElem : nombre d'elements de la parametrisation      //
    //          - float pas : pas d'échantillonage                          //
    // sortie :                                                             //
    //          - List<float> T : parametrisation Tchebycheff               //
    //          - List<float> tToEval : echantillon sur la parametrisation  //
    //////////////////////////////////////////////////////////////////////////
    (List<float>, List<float>) buildParametrisationTchebycheff(int nbElem, float pas)
    {
        // Vecteur des pas temporels
        List<float> T = new List<float>();
        // Echantillonage des pas temporels
        List<float> tToEval = new List<float>();

        // Construction des pas temporels
        for(int i=0; i<nbElem; i++){

            float num = (2*i+1)*Mathf.PI;
            float den = (2*nbElem+2);
            T.Add(nbElem*Mathf.Cos(num/den));
        }

        // Construction des échantillons
        for(float j = T.Min(); j <= T.Max(); j+=pas){
            tToEval.Add(j);
        }

        return (T, tToEval);
    }

    //////////////////////////////////////////////////////////////////////////
    // fonction : applyNevilleParametrisation                               //
    // semantique : applique la subdivion de Neville aux points (x,y)       //
    //              placés en paramètres en suivant les temps indiqués      //
    // params :                                                             //
    //          - List<Vector3> points : liste des points                   //
    //          - List<float> T : temps de la parametrisation               //
    //          - List<float> tToEval : échantillon des temps sur T         //
    // sortie : rien                                                        //
    //////////////////////////////////////////////////////////////////////////
    void applyNevilleParametrisation(List<GameObject> points, List<float> T, List<float> tToEval)
    {
        //List<Transform> ligne = new List<Transform>();
        for (int i = 0; i < tToEval.Count; ++i)
        {
            // Appliquer neville a l'echantillon i
            Vector3 pos;
            Quaternion rot;
            (pos, rot) = neville(points, T, tToEval[i]);
            GameObject newPointView =  Instantiate(pointPrefab, pos, rot) as GameObject;
            //Debug.LogError(newPoint);
            points_interpoles.Add(newPointView);
        }

    }

    //////////////////////////////////////////////////////////////////////////
    // fonction : neville                                                   //
    // semantique : calcule le point atteint par la courbe en t sachant     //
    //              qu'elle passe par les (X,Y,Z) en T                      //
    // params :                                                             //
    //          - List<Vector3> points : liste des points                   //
    //          - List<float> T : liste des temps de la parametrisation     //
    //          - t : temps ou on cherche le point de la courbe             //
    // sortie : point atteint en t de la courbe                             //
    //////////////////////////////////////////////////////////////////////////
    private (Vector3, Quaternion) neville(List<GameObject> points, List<float> T, float t)
    {
        int n = points.Count;
        Vector3[,] Position_Inter = new Vector3[n-1,n-1];
        Quaternion[,] Rot_Inter = new Quaternion[n-1,n-1];

        for(int i=0; i<n-1; i++){
            for(int j=0;j<n-1-i; j++){
                int tg = j;
                int td = i+j+1;

                float TG = (T[td] - t)/(T[td] - T[tg]);
                float TD = (t - T[tg])/(T[td] - T[tg]);


                Vector3 position_gauche;
                Vector3 position_droit;

                Quaternion rotation_gauche;
                Quaternion rotation_droit;

                if(i==0){
                    position_gauche = points[j].transform.position;
                    rotation_gauche = points[j].transform.rotation;

                    position_droit = points[j+1].transform.position;
                    rotation_droit = points[j+1].transform.rotation;
                }
                else{
                    position_gauche = Position_Inter[i-1,j];
                    rotation_gauche = Rot_Inter[i-1,j];

                    position_droit = Position_Inter[i-1,j+1];
                    rotation_droit = Rot_Inter[i-1,j+1];

                }
                Vector3 position_inter = TG*position_gauche+TD*position_droit;
                Quaternion rotation_inter = Quaternion.Slerp(rotation_gauche, rotation_droit, TD);
                
                //GameObject newPointView =  Instantiate(pointPrefab, position_inter, rotation_inter) as GameObject;
                //PointsInter[i,j] = newPointView;
                
                Position_Inter[i,j] = position_inter;
                Rot_Inter[i,j] = rotation_inter;

            }
        }
        return (Position_Inter[n-2,0],Rot_Inter[n-2,0]) ;
    }

}
