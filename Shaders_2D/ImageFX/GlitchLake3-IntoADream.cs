using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchLake3 : MonoBehaviour
{
    public AnimationState mask;
    public AnimationState anne;
    private GlitchControl glitchControl;

    // Start is called before the first frame update
    void Start()
    {
        glitchControl = GetComponent<GlitchControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anne.state == 1)
            glitchControl.canGlitch = false;
        else if (mask.state ==  1)
            glitchControl.canGlitch = true;
        else
            glitchControl.canGlitch = false;
    }
}
