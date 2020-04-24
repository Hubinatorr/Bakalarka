using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    public Transform iconTargetTransform;
    public GameObject iconPrefab;

    public Camera cam;
    public Camera defcam;
    public Camera mapcam;
    // Start is called before the first frame update
    private bool _onScreen;

    public GameObject Drone;


    public int distanceOfSight = 50;
    
    private int i = 0;
    void Start()
    {
        foreach(GameObject item in Drones.drones)
        {
            GameObject icon = (GameObject)Instantiate(iconPrefab);
            icon.transform.SetParent(iconTargetTransform);
            icon.name = "icon" + item.name;
            icon.transform.localScale = new Vector3(1,1,1);
            icon.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float minimumDistance = 50.0F;
        float maximumDistance = 100.0F;
        
        float minimumDistanceScale = 1.0F;
        float maximumDistanceScale = 0.1F;
        
        

        if(GuiController.isMap)
            cam = mapcam;
        else
            cam = defcam;

        foreach(Transform child in iconTargetTransform.transform)
        {
            Text text = child.GetComponentInChildren<Text>();
            float dist = Vector3.Distance(Drones.drones[0].transform.position,Drones.drones[i].transform.position);
            text.text = Mathf.Round(dist) + "m";
            
            float norm = (dist - minimumDistance) / (maximumDistance - minimumDistance);
            norm = Mathf.Clamp01(norm);
            
            var minScale = Vector3.one * maximumDistanceScale;
            var maxScale = Vector3.one * minimumDistanceScale;
            
            child.transform.localScale = Vector3.Lerp(maxScale, minScale, norm);
            // if(!Drones.drones[i].GetComponent<Renderer>().isVisible){
            //     child.gameObject.SetActive(false);
            // }

            // if(Drones.drones[i].GetComponent<Renderer>().isVisible){
            //     child.gameObject.SetActive(true);
            // }
            Vector3 iconPos = cam.WorldToScreenPoint(Drones.drones[i].transform.position);
            _onScreen = cam.pixelRect.Contains( iconPos ) && iconPos.z > cam.nearClipPlane;
            if(_onScreen && i!= 0){
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }

            if(_onScreen && i== 0 && GuiController.isMap)
                child.gameObject.SetActive(true);
            child.transform.position = iconPos;
            i++;
            if(child.transform.position != iconPos)
                child.transform.position = iconPos;

        }
         i=0;
    }
}
