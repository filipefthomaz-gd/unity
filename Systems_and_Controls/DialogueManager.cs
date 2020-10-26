using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using TMPro;
using UnityEngine.UI;
using System;

public class DialogueManager : MonoBehaviour
{
    //ATTENTION: TO REVERT BACK TO NORMAL DIALOGUE (NO CLOUD), JUST REMOVE ALL THE CLOUD ELEMENTS, GO BACK TO TEXTDIPLAY ON THE VIEWPORT FUNCTION
    //AND REPLACE THE TEXTDISPLAY ON THE CANVAS PREFAB
    public TextMeshProUGUI textDisplay;
    public float secondsBetweenLines;
    public float typingSpeed;
    public Canvas canvas;
    public Camera camera;
    public GameObject eventSystem;
    public Image cloud;

    public GameObject[] m_Button;

    public ButtonBehavior buttonBehavior;
    public DialogueAnimation dAnimation;

    private bool CR_text = false;
    private bool CR_audio = false;
    static public bool inDialogue = false;
    private bool clicked = true;

    private Vector2 canvasFromViewPort;

    public string player = "John";

    public float cloudOpacity = 0.52f;

    private string actualKey;
    private string step;
    private int index = 0;
    private int index2 = 0;

    private int choice = -1;

    private string dialoguePath;
    private JsonData fullDialogue;
    private JsonData lineToParse;

    private bool inMove = false;

    private AudioSource audio;
    private TextShower textShower;

    private bool canSkipLine = false;

    private Vector2 cloudOriginalSize;

    private string tempGO = "";
    private Vector3 tempCamPos = new Vector3(1000, 1000, 0);
    private Vector3 tempGOPos = new Vector3(1000, 1000, 0);

    static public float actualResolution = 1.77778f;

    static public bool dialogueEffect = true;
    static public bool autoPlay = true;

    // Initialization. Guarantees deactivated buttons, deactivated eventsystem, deactivated inDialogue, faded dialogueBox and correct dialogue volume
    private void Start()
    {
        buttonBehavior.deactivateButtons(m_Button);
        autoPlay = MenuFunctions.dialogueAutoplay;

        eventSystem = GameObject.Find("DialogueEventSystem");
        eventSystem.SetActive(false);

        inDialogue = false;

        cloud = GameObject.Find("DialogueImage").GetComponent<Image>();
        cloud.color = new Color(cloud.color.r, cloud.color.g, cloud.color.b, 0);
        cloudOriginalSize = cloud.rectTransform.sizeDelta;

        audio = GetComponent<AudioSource>();
        audio.volume = Audio_Level.dialogueVolumeFraction;

        textShower = GetComponent<TextShower>();

    }

    //In Move Setter function
    public bool isinMove(){
        return inMove;
    }

    ////////////////////////
    /// BUTTON FUNCTIONS ///
    ////////////////////////

    void toDoOnClick(int choice)
    {
        clicked = true;
        index = choice + 1;
        lineToParse = ParseStep(lineToParse, fullDialogue);
        //Remove all Listeners!
        m_Button[0].GetComponent<Button>().onClick.RemoveListener(ClickedButton1);
        m_Button[1].GetComponent<Button>().onClick.RemoveListener(ClickedButton2);
        m_Button[2].GetComponent<Button>().onClick.RemoveListener(ClickedButton3);
        eventSystem.SetActive(false);
        buttonBehavior.deactivateButtons(m_Button);
    }

    void ClickedButton1(){
        toDoOnClick(0);
    }

    void ClickedButton2(){
        toDoOnClick(2);
    }

    void ClickedButton3(){
        toDoOnClick(4);
    }



    ////////////////////////////
    /// JSON PARSE FUNCTIONS ///
    ////////////////////////////

    //Detects if line is an option, a line (and which format) or an end of dialogue (EOD)
    private string FindType(JsonData currentStep)
    {
        string key = "JsonData object";
        string array = "JsonData array";
        if (currentStep.ToString() == key)
        {
            foreach (JsonData token in currentStep.Keys)
                actualKey = token.ToString();

            if (actualKey == "EOD")
                return "EOD";

            else if (actualKey.Contains("?"))
                return "options";

            else if (currentStep[0].ToString() == array)
                return "key_array";

            else
                return "key_phrase";
        }
        return "phrase";
    }



    private string oldSpeaker = "";

    //FindSpeakear Coroutine. It detects the new speaker tag (actualKey), changes the color and position of dialogue if necessary, and delievers the line
    private IEnumerator findSpeaker(JsonData currentStep, string fullLine, bool phrase)
    {
        //If it's a phrase it means that the speaker is the same as before, so bypass everything regarding to new speaker
        if (!phrase)
        {
            foreach (JsonData token in currentStep.Keys)
                actualKey = token.ToString();

            //Fade Out of the cloud. Prevent new line from advancing (CR_text)
            //Fade In occurs naturally if faded out in line
            if(oldSpeaker != actualKey && oldSpeaker != "")
            {
                CR_text = true;
                haltViewPortToCanvas = true;
                StartCoroutine(FadeUI(0, cloud));
                yield return new WaitForSeconds(0.45f);
                CR_text = false;
                haltViewPortToCanvas = false;
            }

            viewPortToCanvas(GameObject.Find(actualKey));
            characterColor(GameObject.Find(actualKey));
        }

        //Set old speaker and deliever line;
        oldSpeaker = actualKey;
        lineDeliever(fullLine);
        yield return null;
    }


    //Receives the full correct string, including special characters / functions
    private void lineDeliever(string fullLine)
    {
        StopAllCoroutines();
        //Adds a cloud below the textDisplay
        StartCoroutine(FadeUI(cloudOpacity, cloud));

        //Deals with ':anim' special functions (animations, special sounds and scene changes
        string spokenLine = dAnimation.dialogueAnimation(fullLine);

        if (fullLine.Contains(" :audio "))
        {
            string[] fullLineSplit = spokenLine.Split(new string[] { " :audio " }, System.StringSplitOptions.None);
            AudioClip audioClip = Resources.Load<AudioClip>("Sound/" + dialoguePath+"/"+fullLineSplit[1]);
            StartCoroutine(Play(audioClip));

            string finalLine = hasEmphasis(fullLineSplit[0]);

            //Dialogue Effect turns slow fade in ON
            if(dialogueEffect)
                StartCoroutine(Type(finalLine));

            else
                StartCoroutine(SmoothType2(finalLine));
        }
        else
        {
            string finalLine = hasEmphasis(spokenLine);

            //Dialogue Effect turns slow fade in ON
            if (dialogueEffect)
                StartCoroutine(Type(finalLine));

            else
                StartCoroutine(SmoothType2(finalLine));
        }

        canSkipLine = true;
    }


    //Deals with :i (and futurely :b and :u) special functions
    string hasEmphasis(string line)
    {
        if (line.Contains(":i "))
        {
            string[] fullLineSplit = line.Split(new string[] { ":i " }, System.StringSplitOptions.None);
            textDisplay.fontStyle = FontStyles.Italic;
            return fullLineSplit[1];
        }

        else
        {
            textDisplay.fontStyle = FontStyles.Normal;
            return line;
        }

    }

    private bool haltViewPortToCanvas;

    //Parsing function. It firstly calls the FindType function and then decides what to do based on it;
    public JsonData ParseStep(JsonData lineToParse, JsonData fullDialogue)
    {
        canSkipLine = false;
        CR_audio = false;
        audio.Stop();
        StopAllCoroutines();
        JsonData currentStep = lineToParse[index];
        step = FindType(currentStep); //Gets the type of line in the dialogue file;

        //Possibly redundant settings, but guarantee that the dialogue text is empty and that the dialogue box can move before a new line is set;
        textDisplay.text = "";
        haltViewPortToCanvas = false;

        //END OF DIALOGUE - Resets everything and fades out the dialoguebox (cloud);
        if (step == "EOD")
        {
            StartCoroutine(FadeUI(0, cloud));
            audio.clip = null;
            inDialogue = false;
            inMove = false;
            index = 0;
            index2 = 0;
            return currentStep[0];
        }

        //Types of YAML/JSON Dialogue Input
        else if (step == "key_phrase")
        {
            StartCoroutine(findSpeaker(currentStep, currentStep[0].ToString(), false));

            index2++;
            index = index2;
            return fullDialogue;
        }

        //Options means that it's a choice for the player, so there is no dialogue being said, but mostly calling button functions
        else if (step == "options")
        {
            /*foreach (JsonData token in currentStep.Keys)
            {
                actualKey = token.ToString();
                actualKey = actualKey.Replace("?", "");
            }*/

            haltViewPortToCanvas = true;
            StartCoroutine(FadeUI(0, cloud)); //First element of fade function is the final opacity, so in this case it's a fade out;
            buttonBehavior.activateButtons(m_Button);

            eventSystem.SetActive(true);

            for (int i = 0; i < currentStep[0].Count/2; i++)
                m_Button[i].transform.Find("ButtonText").gameObject.GetComponent<TextMeshProUGUI>().text = currentStep[0][i*2].ToString();
            clicked = false;

            m_Button[0].GetComponent<Button>().onClick.AddListener(ClickedButton1);
            m_Button[1].GetComponent<Button>().onClick.AddListener(ClickedButton2);
            if (currentStep[0].Count / 2 == 3)
                m_Button[2].GetComponent<Button>().onClick.AddListener(ClickedButton3);
            else
                m_Button[2].SetActive(false);

            return currentStep[0];

        }

        else if (step == "key_array")
        {
            StartCoroutine(findSpeaker(currentStep, currentStep[0][0].ToString(), false));
            index = 1;
            return currentStep[0];
        }

        else //(step == "phrase")
        {
            StartCoroutine(findSpeaker(currentStep, currentStep.ToString(), true));
            index++;
            return lineToParse;
        }

    }

    private JsonData nextDialogue;

    //Load Dialogue function
    public bool loadDialogue(string path)
    {
        //Try to load the dialogue with the expected language
        //If failed, use English subtitles
        try
        {
            var jsonTextFile = Resources.Load<TextAsset>("Dialogues/" + MenuFunctions.language + "/" + path);
            fullDialogue = JsonMapper.ToObject(jsonTextFile.text);
        }
         catch (Exception e)
        {
            var jsonTextFile = Resources.Load<TextAsset>("Dialogues/" + "EN/" + path);
            fullDialogue = JsonMapper.ToObject(jsonTextFile.text);
        }

        lineToParse = fullDialogue;
        dialoguePath = path;
        if(!inMove)
            inMove = true;

        return true;
    }


    public JsonData firstDialogue()
    {
        if (!inDialogue)
        {
            inDialogue = true;

            lineToParse = ParseStep(lineToParse, fullDialogue);
            return lineToParse;
        }
        return "";
    }


    public JsonData nextSentence()
    {
        //Goes for the next line if it's in dialogue and a button choice has been made (clicked)
        //It also needs to either wait for the type and audio co-routines to be over, or these are bypassed using the C key;
        if (((Controls.interact && canSkipLine) || (!CR_text && !CR_audio && autoPlay)) && inDialogue && clicked && !ChangeScene.onSettings)
            lineToParse = ParseStep(lineToParse, fullDialogue);

        return lineToParse;
    }



    //////////////////////////
    /// TEXT BOX FUNCTIONS ///
    //////////////////////////


    //Type and Play Functions
    //Old Typing function that just adds to the dialogue box the text letter by letter, without any fading or smoothing effect
    public IEnumerator RoughType(string line)
    {
        CR_text = true;

        foreach (char letter in line.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(secondsBetweenLines);
        CR_text = false;
    }

    public IEnumerator SmoothType2(string line)
    {
        CR_text = true;


        Color start = new Color(textDisplay.color.r, textDisplay.color.g, textDisplay.color.b, 0);
        Color end = new Color(textDisplay.color.r, textDisplay.color.g, textDisplay.color.b, 1);
        textDisplay.color = start;
        textDisplay.text = line;
        textDisplay.ForceMeshUpdate();


        float duration = 0.2f;

        if (cloud.color.a < 0.1)
            duration = 0.0f;

        if (textDisplay.textBounds.extents.y < 29 && textDisplay.textBounds.extents.x < 128)
            StartCoroutine(ChangeSize(0.75f, duration));
        else
            StartCoroutine(ChangeSize(1f, duration));

        for (float t = 0.0f; t < 0.5f; t += 0.03f)
        {
            textDisplay.color = Color.Lerp(start, end, t / 0.5f);
            yield return new WaitForSeconds(0.03f);
        }
        textDisplay.color = end;

        yield return new WaitForSeconds(secondsBetweenLines);
        CR_text = false;
    }

    //Fade in TextMeshPro line. Function is in the TextShower Component
    public IEnumerator Type(string line)
    {
        CR_text = true; //CR_text means 'in the type-coroutine'
        textDisplay.text = line;
        float duration = 0.2f;

        //The variables of AnimateVertexColors are the TMP Text itself and the Color that is combining with the original one for the fade;
        //A white color does not mess with the RGB, and only with the A;
        StartCoroutine(textShower.AnimateVertexColors(textDisplay, Color.white));


        if (cloud.color.a < 0.1)
            duration = 0.0f;

        if (textDisplay.textBounds.extents.y < 29 && textDisplay.textBounds.extents.x < 128)
            StartCoroutine(ChangeSize(0.75f, duration));
        else
            StartCoroutine(ChangeSize(1f, duration));

        //isRangeMax is the variable that ends the AnimateVertexColors Coroutine, so the Type CR has to wait for the previous CR.
        while (!textShower.isRangeMax)
            yield return null;

        yield return new WaitForSeconds(secondsBetweenLines); //Added time for people to read the lines after the Typing is complete
        CR_text = false; //Coroutine ended
    }


    //Adds an audio to a line as a Coroutine
    public IEnumerator Play(AudioClip audioClip)
    {
        CR_audio = true;

        audio.clip = audioClip;
        audio.Play();
        while (audio.isPlaying || ChangeScene.audioPause)
        {
            yield return null;
        }
        audio.Stop();
        CR_audio = false;
    }


    // Placement Function (IT IS FUNCTIONING FOR AN INITIAL PLACEMENT OF THE DIALOGUE BOX IN THE LEFT-BOTTOM CORNER, WITH THE ORIGIN ON ITS OWN CENTER)
    public void viewPortToCanvas(GameObject gameobject)
    {
        if (gameobject.gameObject.name != tempGO || Camera.main.transform.position != tempCamPos ||
            gameobject.gameObject.transform.position != tempGOPos)
        {
            tempCamPos = Camera.main.transform.position;
            tempGO = gameobject.gameObject.name;
            tempGOPos = gameobject.gameObject.transform.position;
            Rigidbody2D rb2D = gameobject.GetComponent<Rigidbody2D>();
            SpriteRenderer spR = gameobject.GetComponent<SpriteRenderer>();

            //Vectors needed to convert Viewport into Canvas Screen
            Vector2 HalfCamera = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize * (16f / 9f) / actualResolution);
            Vector2 Conversion = new Vector2(canvas.pixelRect.xMax / (2 * HalfCamera.x), canvas.pixelRect.yMax / (2 * HalfCamera.y));
            Vector2 camCorrection = new Vector2(camera.transform.position.x, camera.transform.position.y);

            //Getting Position and Size of Objects
            Vector2 worldObjectPosition = (rb2D.position - camCorrection + HalfCamera) * Conversion;
            Vector2 objectSize = new Vector2(spR.bounds.size.x, spR.bounds.size.y);
            Vector2 worldObjectSize = objectSize * Conversion;

            //Translating Text position to above object with RigidBody and SpriteRenderer;
            //First term for both x and y relate to the fact that the cloud sprite and the dialogue have (0,0) not exactly on the left-bottom corner but on the middle of the cloud image
            canvasFromViewPort = new Vector2(worldObjectPosition.x - 10 * canvas.scaleFactor,
                                             cloudOriginalSize.y / 2 * canvas.scaleFactor + worldObjectPosition.y
                                             + worldObjectSize.y / 2 - 20 * canvas.scaleFactor); //(+0,+2)*scaleFactor for textDisplay


            //cloud.rectTransform.sizeDelta = new Vector2(textDisplay.textBounds.extents.y * 2.5f * 5/2, textDisplay.textBounds.extents.y * 2.5f);
            cloud.rectTransform.position = canvasFromViewPort;
        }

    }

    private Color fadedColor;

    //Sets up the color for each Character according to the Character Component added to each speaker
    public void characterColor(GameObject gameobject)
    {
        if (gameobject.gameObject.name == player)
        {
            textDisplay.color = Color.LerpUnclamped(Color.white, Color.black, 0.9f);
            textDisplay.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
        }


        else
        {
            textDisplay.color = gameobject.GetComponent<Characters>().dialogColor;
            textDisplay.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.3f);
        }

        //For the fade in TextMeshPro to work, the initial color must be faded out but with the correct RGB
        fadedColor = new Color(textDisplay.color.r, textDisplay.color.g, textDisplay.color.b, 0);
        textDisplay.color = fadedColor;
    }


    //FadeUI element function (Same fundamental code as Sprite Renderer Fade used in the class Characters, for instance)
    public IEnumerator FadeUI(float opacity, Image image)
    {
        Color colorStart = image.color;
        Color colorEnd = new Color(colorStart.r, colorStart.g, colorStart.b, opacity);

        /*if (opacity > 0.1f)
            StartCoroutine(ChangeSize(1.0f, 0.3f));
        else
            StartCoroutine(ChangeSize(0.1f, 0.3f));*/

        for (float t = 0.0f; t < 0.4f; t += 0.05f)
        {
            image.color = Color.Lerp(colorStart, colorEnd, t / 0.4f);
            yield return new WaitForSeconds(0.05f);
        }
        image.color = colorEnd;
    }

    //Changes Size of the Cloud-Dialogue Text Box;
    public IEnumerator ChangeSize(float sizeFraction, float duration)
    {
        Vector2 currentSize = cloud.rectTransform.sizeDelta;
        Vector2 finalSize = cloudOriginalSize * sizeFraction;
        for (float t = 0.0f; t < duration; t += 0.025f)
        {
            cloud.rectTransform.sizeDelta = Vector2.Lerp(currentSize, finalSize, t / duration);
            yield return new WaitForSeconds(0.025f);
        }
        cloud.rectTransform.sizeDelta = finalSize;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (inDialogue && !haltViewPortToCanvas)
            viewPortToCanvas(GameObject.Find(actualKey));
   }


}
