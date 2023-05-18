using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    public bool display;
    public MeshRenderer position;
    public MeshRenderer direction;
    // Start is called before the first frame update
    void Start()
    {
        enableRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDisplay(bool disp){
        this.display = disp;
        enableRenderer();
    }

    public void enableRenderer(){
        if(display){
            position.enabled = true;
            direction.enabled = true;
        }
        else{
            position.enabled = false;
            direction.enabled = false;
        }
    }
}
