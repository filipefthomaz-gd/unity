using UnityEngine;

public class MaskChanger : MonoBehaviour
{
    // Scroll main texture based on time

    float scrollSpeed = 0.5f;
    public float scale;
    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer> ();
    }

    void Update()
    {
        // Animates main texture scale in a funky way!
        ////float scaleX = Mathf.Cos(Time.time) * 0.5f + 1;
        //float scaleY = Mathf.Sin(Time.time) * 0.5f + 1;
        rend.material.SetTextureOffset("_MaskTex", new Vector2(100, 0));
    }
}