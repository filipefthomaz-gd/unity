using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchControl : MonoBehaviour
{
    public float maxGlitch;
    public float minGlitch = 0;
    public float glitchVarDuration = 3;
    public float lerpGlitchNoise;
    public CustomImageEffect CIE;
    public Vector4 displacementAmount;

    public float timeBetweenGlitches;
    public float betweenGlitchesTimeNoise;

    public bool canGlitch = false;

    private bool isLoading;
    private string scene = "DL3Journey2";

    private Material mat;
    private bool isChangingGlitch;
    private bool up;
    private float glitch;

    void Awake()
    {
        mat = Instantiate(CIE.material);
        mat.SetFloat("_GlitchEffect", minGlitch);
        mat.SetVector("_DisplacementAmount", displacementAmount);

    }

    // Start is called before the first frame update
    void Start()
    {
        CIE.material = mat;
        up = true;
        glitch = minGlitch;
    }


    public IEnumerator ChangeGlitch(bool up)
    {
        isChangingGlitch = true;
        float noise = Random.Range(-lerpGlitchNoise, lerpGlitchNoise);

        for (float t = 0.0f; t < glitchVarDuration; t += 0.005f)
        {
            if(up)
                glitch = Mathf.SmoothStep(minGlitch, maxGlitch + noise, t / glitchVarDuration);
            else
                glitch = Mathf.SmoothStep(maxGlitch, minGlitch, t / glitchVarDuration);

            mat.SetFloat("_GlitchEffect", glitch);
            CIE.material = mat;
            yield return new WaitForSeconds(0.005f);
        }

        if (up)
        {
            mat.SetFloat("_GlitchEffect", maxGlitch);
            CIE.material = mat;
        }
        else
        {
            mat.SetFloat("_GlitchEffect", minGlitch);
            CIE.material = mat;
            yield return new WaitForSeconds(timeBetweenGlitches + Random.Range(-betweenGlitchesTimeNoise, betweenGlitchesTimeNoise));
        }
        isChangingGlitch = false;
    }

    public IEnumerator LoadSceneGlitch(float duration)
    {
        GameObject.Find("John").GetComponent<PlayerControlor>().enabled = false;
        float actualMaxGlitch = maxGlitch;
        float actualMinGlitch = minGlitch;
        float actualDisplacementX = displacementAmount.x;
        float actualDisplacementY = displacementAmount.y;
        Color startColor = GetComponent<SpriteRenderer>().color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        for (float t = 0.0f; t < duration; t += 0.05f)
        {
            maxGlitch = Mathf.Lerp(actualMaxGlitch, 0.7f, t / duration);
            minGlitch = Mathf.Lerp(actualMinGlitch, 0.1f, t / duration);
            float displacementX = Mathf.Lerp(actualDisplacementX, 0.6f, t / duration);
            float displacementY = Mathf.Lerp(actualDisplacementY, 0.25f, t / duration);
            //GetComponent<SpriteRenderer>().color = Color.Lerp(startColor, endColor, t / duration);


            mat.SetVector("_DisplacementAmount", new Vector4(displacementX, displacementY, displacementAmount.z, displacementAmount.w));
            CIE.material = mat;
            yield return new WaitForSeconds(0.05f);
        }   
        
        GetComponent<SpriteRenderer>().color = endColor;
        GameObject.Find("BackgroundMusic").GetComponents<AudioSource>()[0].volume = 0;
        GameObject.Find("BackgroundMusic").GetComponents<AudioSource>()[1].volume = 0;
        GameObject.Find("SoundEffects1").GetComponents<AudioSource>()[0].volume = 0;
        GameObject.Find("SoundEffects1").GetComponents<AudioSource>()[1].volume = 0;
        GameObject.Find("SoundEffects2").GetComponents<AudioSource>()[0].volume = 0;
        GameObject.Find("SoundEffects2").GetComponents<AudioSource>()[1].volume = 0;
        GameObject.Find("SoundEffects3").GetComponents<AudioSource>()[0].volume = 0;
        GameObject.Find("SoundEffects3").GetComponents<AudioSource>()[1].volume = 0;
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("TrackScene").GetComponent<TrackScene>().LoadScene(scene, false);
    }

    public void LoadSceneGlitchFunction(float duration)
    {
        StartCoroutine(LoadSceneGlitch(duration));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canGlitch)
        {
            if (maxGlitch == 0)
            {
                canGlitch = false;
                mat.SetFloat("_GlitchEffect", 0);
                CIE.material = mat;
            }

            if (!isChangingGlitch)
            {
                StartCoroutine(ChangeGlitch(up));
                up = !up;
            }
        }
        else
        {
            mat.SetFloat("_GlitchEffect", 0);
            CIE.material = mat;
        }
    }
}
