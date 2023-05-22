using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interpolateur : MonoBehaviour
{

    public enum ParametrisationTypes {Distance, Tchebycheff};

    public ParametrisationTypes parametrisationType;

    public MovementPath path;
    public FollowPath cameraPath;

    public float pas = 0.01f;

    public GameObject pointPrefab;
    
    // Booleen pour savoir si on affiche les points par lesquels
    // la camera passe
    public bool display = true;

    private List<float> T;
    private List<float> tToEval;

    //Liste qui va contenir les points interpoles
    private List<GameObject> points_interpoles = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //On verifie si un chemin (path) a bien ete assigne
        if(path == null){
            Debug.LogError("No path assigned to follow", gameObject);
            return;
        }

        //On verifie si une camera (camera) a bien ete assigne
        if(cameraPath == null){
            Debug.LogError("No Camera assigned to follow", gameObject);
            return;
        }


        //Cette boucle permet d'afficher ou non les points originaux de la camera
        foreach(GameObject point in path.getPathList()){
            CameraView camView = point.GetComponent<CameraView>();
            camView.setDisplay(this.display);
        }

        // Parametrisation
        int nbElem = path.PathSequence.Length;
        if(parametrisationType == ParametrisationTypes.Tchebycheff){
            (T,tToEval) = buildParametrisationTchebycheff(nbElem, pas);
        }
        else if(parametrisationType == ParametrisationTypes.Distance){
            (T,tToEval) = buildParametrisationDistance(nbElem, pas);
        }

        //Application de l'algorithme de Neuville
        applyNevilleParametrisation(path.getPathList(), T, tToEval);
        path.setPath(points_interpoles);
        path.setPathType(MovementPath.PathTypes.unique_use);


        //Cette boucle permet d'afficher ou non les points interpoles de la camera
        foreach(GameObject point in points_interpoles){
            CameraView camView = point.GetComponent<CameraView>();
            camView.setDisplay(this.display);
        }
        
        //On commence le mouvement de la camera
        cameraPath.initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //////////////////////////////////////////////////////////////////////////
    // fonction : buildParametrisationDistance                              //
    // semantique : construit la parametrisation sur les distances et les   //
    //              échantillons de temps selon cette parametrisation       //
    // params :                                                             //
    //          - int nbElem : nombre d'elements de la parametrisation      //
    //          - float pas : pas d'échantillonage                          //
    // sortie :                                                             //
    //          - List<float> T : parametrisation distances                 //
    //          - List<float> tToEval : echantillon sur la parametrisation  //
    //////////////////////////////////////////////////////////////////////////
    (List<float>, List<float>) buildParametrisationDistance(int nbElem, float pas)
    {
        // Vecteur des pas temporels
        List<float> T = new List<float>();
        // Echantillonage des pas temporels
        List<float> tToEval = new List<float>();

        List<GameObject> points = path.getPathList();

        // Construction des pas temporels
        T.Add(0);
        for(int i=1; i<nbElem; i++){
            Vector3 pos1 = points[i-1].transform.position;
            Vector3 pos2 = points[i].transform.position;

            Vector3 diff = pos1-pos2;

            float distance = diff.magnitude;
            T.Add(T[i-1]+distance);
        }

        // Construction des échantillons
        for(int j = 0; j*pas < T[T.Count-1]; j++){
            tToEval.Add(j*pas);
        }
        tToEval.Add(T[T.Count-1]);

        return (T, tToEval);
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

        return (T,tToEval );
    }

    //////////////////////////////////////////////////////////////////////////
    // fonction : applyNevilleParametrisation                               //
    // semantique : applique la subdivion de Neville aux points             //
    //              placés en paramètres en suivant les temps indiqués      //
    // params :                                                             //
    //          - List<GameObject> points : liste des points a interpoler   //
    //          - List<float> T : temps de la parametrisation               //
    //          - List<float> tToEval : échantillon des temps sur T         //
    // sortie : rien                                                        //
    //////////////////////////////////////////////////////////////////////////
    void applyNevilleParametrisation(List<GameObject> points, List<float> T, List<float> tToEval)
    {
        
        for (int i = 0; i < tToEval.Count; ++i)
        {
            // Appliquer neville a l'echantillon i
            Vector3 pos;
            Quaternion rot;
            (pos, rot) = neville(points, T, tToEval[i]);
            GameObject newPointView =  Instantiate(pointPrefab, pos, rot) as GameObject;
            points_interpoles.Add(newPointView);
        }

        if(parametrisationType == ParametrisationTypes.Tchebycheff){
            points_interpoles.Reverse();
        }

    }

    //////////////////////////////////////////////////////////////////////////
    // fonction : neville                                                   //
    // semantique : calcule le point atteint par la courbe en t sachant     //
    //              qu'elle passe par les points en T                       //
    // params :                                                             //
    //          - List<GameObject> points : liste des points a interpoler   //
    //          - List<float> T : liste des temps de la parametrisation     //
    //          - t : temps ou on cherche le point de la courbe             //
    // sortie :                                                             //
    //          (Vector3 position interpole, Quaternion rotation interpole) //
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
                
                Position_Inter[i,j] = position_inter;
                Rot_Inter[i,j] = rotation_inter;

            }
        }
        return (Position_Inter[n-2,0],Rot_Inter[n-2,0]) ;
    }



}
