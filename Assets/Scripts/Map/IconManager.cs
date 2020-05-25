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

    public Transform middle;
 

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
        float minimumDistance = distanceOfSight;
        float maximumDistance = 100.0F;
        
        float minimumDistanceScale = 1.0F;
        float maximumDistanceScale = 0.1F;
        
        

        if(GuiController.isMap)
            cam = mapcam;
        else
            cam = defcam;

        foreach(Transform child in iconTargetTransform.transform)
        {
            Image img = child.GetComponent<Image>();
            Image arrow = child.transform.Find("Arrow").GetComponent<Image>();
            // arrow.gameObject.transform.LookAt(middle);
            arrow.gameObject.transform.right = middle.position - arrow.gameObject.transform.transform.position;
            arrow.color = img.color;

             float minX = img.GetPixelAdjustedRect().width / 2+1;
            // Maximum X position: screen width - half of the icon width
            float maxX = Screen.width*0.81f - minX-1;

            // Minimum Y position: half of the height
            float minY = Screen.height*0.08f + img.GetPixelAdjustedRect().height / 2 +1;
            // Maximum Y position: screen height - half of the icon height
            float maxY = Screen.height*0.96f - img.GetPixelAdjustedRect().height / 2 -2;

            // Temporary variable to store the converted position from 3D world point to 2D screen point
            Vector2 pos = cam.WorldToScreenPoint(Drones.drones[i].transform.position);
            
            if(Vector3.Dot((Drones.drones[i].transform.position - cam.transform.position), cam.transform.forward) < 0)
            {
            // Check if the target is on the left side of the screen
            if(pos.x < Screen.width / 2)
            {
                // Place it on the right (Since it's behind the player, it's the opposite)
                pos.x = maxX;
            }
            else
            {
                // Place it on the left side
                pos.x = minX;
            }
            }

        // Limit the X and Y positions
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Update the marker's position
        img.transform.position = pos;

            // // Ziskam text so vzdialenostou
            Text text = child.GetComponentInChildren<Text>();
            // Ziskam vzdialenost
            float dist = Vector3.Distance(Drones.drones[0].transform.position,Drones.drones[i].transform.position);
            text.text = Mathf.Round(dist) + "m";

            // // NASTAVITELNE

            if(dist < 10.0F){
                child.gameObject.SetActive(false);
            } else {
                child.gameObject.SetActive(true);
            }

            float norm = (dist - minimumDistance) / (maximumDistance - minimumDistance);
            norm = Mathf.Clamp01(norm);
            
            var minScale = Vector3.one * maximumDistanceScale;
            var maxScale = Vector3.one * minimumDistanceScale;
            
            child.transform.localScale = Vector3.Lerp(maxScale, minScale, norm);

            
            // // if(!Drones.drones[i].GetComponent<Renderer>().isVisible){
            // //     child.gameObject.SetActive(false);
            // // }

            // // if(Drones.drones[i].GetComponent<Renderer>().isVisible){
            // //     child.gameObject.SetActive(true);
            // // }
            Vector3 iconPos = cam.WorldToScreenPoint(Drones.drones[i].transform.position);
            _onScreen = cam.pixelRect.Contains( iconPos ) && iconPos.z > cam.nearClipPlane;
            
            // && dist > 20.0
            if(_onScreen && i!= 0){
                img.enabled = true;
                arrow.enabled = false;
            } else {
                img.enabled = false;
                arrow.enabled = true;
            }

            // if(_onScreen && i== 0 && GuiController.isMap)
            //     child.gameObject.SetActive(true);
            // child.transform.position = iconPos;
            // i++;
            // if(child.transform.position != iconPos)
            //     child.transform.position = iconPos;
            i++;
        }
         i=0;
    }
}
