using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RayCastHandler : MonoBehaviour
{
    // Update is called once per frame

    //Middle point 
    public GameObject Middle;

    public float Radius = 100;

    public TextMeshProUGUI topDistance, bottomDistance, leftDistance, rightDistance,topleftDistance,toprightDistance,bottomleftDistance,bottomrightDistance;


    private enum sites{
        left,topleft,top,topright,right,bottomright,bottom,bottomleft
    }
    void Start()
    {
        topDistance.text = "";
        bottomDistance.text = "";
        leftDistance.text = "";
        rightDistance.text = "";
        topleftDistance.text = "";
        toprightDistance.text = "";
        bottomleftDistance.text = "";
        bottomrightDistance.text = "";
    }


    void ManageRays(Vector3 first,  sites site){
        RaycastHit hit;
        Ray ray1 = new Ray(transform.position, first);
        // Ray ray2 = new Ray(transform.position, second);
        // Ray ray3 = new Ray(transform.position, third);
        float mindistance = 1000;
        if(Physics.SphereCast(ray1,1, out hit)){
            if(hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 10)
                mindistance = hit.distance;
        }  
        // if(Physics.Raycast(ray2, out hit)){
        //     if(hit.collider.tag == "terrain"|| hit.collider.tag == "building"){
        //         distance2 = hit.distance;
        //     }
        // }  

        // if(Physics.Raycast(ray3, out hit)){
        //     if(hit.collider.tag == "terrain"|| hit.collider.tag == "building"){
        //         distance3 = hit.distance;
        //     }
        // }  

        // if(distance2 < mindistance)
        //     mindistance = distance2;
        // if(distance3 < mindistance)
        //     mindistance = distance3;
        

        switch(site){
            case sites.bottom:
                bottomDistance.text = ((int)mindistance).ToString();
                break;
            case sites.bottomleft:
                bottomleftDistance.text = ((int)mindistance).ToString();
                break;
            case sites.bottomright:
                bottomrightDistance.text = ((int)mindistance).ToString();
                break;
            case sites.top:
                topDistance.text = ((int)mindistance).ToString();
                break;
            case sites.topleft:
                topleftDistance.text = ((int)mindistance).ToString();
                break;
            case sites.topright:
                toprightDistance.text = ((int)mindistance).ToString();
                break;
            case sites.left:
                leftDistance.text = ((int)mindistance).ToString();
                break;
            case sites.right:
                rightDistance.text = ((int)mindistance).ToString();
                break;
        }
    }

    void Update()
    {
        // Left
        // ManageRays( new Vector3(-1,0,1), Vector3.left, new Vector3(-1,0,-1),sites.left);
        ManageRays(-transform.right, sites.left);

        // Right
        // ManageRays(Vector3.right,new Vector3(1,0,1),new Vector3(1,0,-1),sites.right);
        ManageRays(transform.right, sites.right);
        
        
        // Right Upper
        // ManageRays(new Vector3(1,1,1),new Vector3(1,1,-1),new Vector3(1,1,0),sites.topright);
        ManageRays(transform.right+transform.up,sites.topright);

        // Left Lower
        // ManageRays(-new Vector3(1,1,1),-new Vector3(1,1,-1),-new Vector3(1,1,0),sites.bottomleft);
        ManageRays(-(transform.right+transform.up),sites.bottomleft);

        // Left Upper
        ManageRays( transform.up-transform.right,sites.topleft);

        // Right Lower
        ManageRays(-transform.up+transform.right,sites.bottomright);
        
        // Up
        ManageRays(transform.up,sites.top);

        // Down
        ManageRays(-transform.up,sites.bottom);
        
    }
}
