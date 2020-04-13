using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    public Transform iconTargetTransform;
    public GameObject iconPrefab;

    public Camera cam;
    // Start is called before the first frame update
    private bool _onScreen;

    
    private int i = 0;
    void Start()
    {
        foreach(GameObject item in Drones.drones)
        {
            GameObject icon = (GameObject)Instantiate(iconPrefab);
            icon.transform.SetParent(iconTargetTransform);
            icon.name = "icon" + item.name;
            icon.transform.localScale = new Vector3(1,1,1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Transform child in iconTargetTransform.transform)
        {
            // if(!Drones.drones[i].GetComponent<Renderer>().isVisible){
            //     child.gameObject.SetActive(false);
            // }

            // if(Drones.drones[i].GetComponent<Renderer>().isVisible){
            //     child.gameObject.SetActive(true);
            // }

            Vector3 iconPos = cam.WorldToScreenPoint(Drones.drones[i].transform.position);
            _onScreen = cam.pixelRect.Contains( iconPos ) && iconPos.z > cam.nearClipPlane;
            if(_onScreen){
                child.gameObject.SetActive(true);
            } else {
                child.gameObject.SetActive(false);
            }
            child.transform.position = iconPos;
            i++;
            if(child.transform.position != iconPos)
                child.transform.position = iconPos;

        }
         i=0;
    }
}
