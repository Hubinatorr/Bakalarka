using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class a : MonoBehaviour
{
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        Rect camRect = cam.rect;
        camRect.xMax = 0.81f; // 90% of viewport
        camRect.yMin = 0.08f;
        camRect.yMax = 0.96f;
        cam.rect = camRect;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
