using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class DialogueManager : MonoBehaviour
{
    public Text textDisplay;
    public GameObject[] buttons;

    private JsonData dialogue;
    private JsonData currentLayer;
    private int index;
    private string speaker;

    private bool inDialogue;

    //SIMPLE DIALOGUE MANAGER:

    //1. Loads a JSON file. Every 'identation level' is hereby called a layer. We start at no identation (layer 0)
    //2. Iterates through lines in layer 0, detecting Speaker gameobject and printing Line
    //3. If a '?' is found as Speaker, we are in the presence of branching, and the layer changes;
    //4. Repeat iteration through new layer until 'EOD' or '?'
    //5. 'EOD' as Speaker ends the dialogue


    //OTHER IMPORTANT CLASSES FOR DIALOGUE MANAGER:

    //DialogueTrigger: Added to every GameObject / Collider that has a or multiple dialogues. It is dialogue trigger that calls the DialogueManager functions;
    //Character: Added to every GameObject that speaks. Contains character-specific variables, such as color of its text, specific sound (if not voice-acted), etc.

    //Function that will be called by Dialogue Trigger. Loads and maps the json file, and initiates dialogue;
    public bool loadDialogue(string path)
    {
        if (!inDialogue) //Safe-guard statement
        {
            index = 0;
            var jsonTextFile = Resources.Load<TextAsset>("Dialogues/" + path);
            dialogue = JsonMapper.ToObject(jsonTextFile.text);
            currentLayer = dialogue;
            inDialogue = true;
            return true;
        }
        return false;
    }

    //Specific Speaker keys (if not EOD nor ?, then it is a spoken line)
    private string findLineType(string speakerString)
    {
        if (speakerString == "EOD")
            return "EndOfDialogue";
        else if (speakerString == "?")
            return "Question";
        else
            return "Line";
    }

    //Print Line Function
    public bool printLine()
    {
        if (inDialogue)
        {
            JsonData line = currentLayer[index];
            foreach (JsonData key in line.Keys)
                speaker = key.ToString();

            string lineType = findLineType(speaker); //Checks if 'Speaker' key is an EOD, ? or normal

            //if EOD than dialogue is ended
            if (lineType == "EndOfDialogue")
            {
                inDialogue = false;
                textDisplay.text = "";
                return false;
            }

            //If Type is Question than find how many options there are and activate according buttons
            else if (lineType == "Question")
            {
                JsonData options = line[0]; //line[0] will be an JsonData array with elements [0, 1, ..., n of options-1]
                textDisplay.text = "";
                for (int optionsNumber = 0; optionsNumber < options.Count; optionsNumber++)
                {
                    activateButton(buttons[optionsNumber], options[optionsNumber]);
                }
            }

            //If it's a line, than print the line
            else if (lineType == "Line")
            {
                dialogueTextColor(speaker); //This code simply adds color depending on the speaker
                textDisplay.text = speaker + ": " + line[0].ToString();
                index++;
            }
        }
        return true;
    }

    //Adds color to the text depending on the speaker name (color and other variables stored on each unique Character elements)
    private void dialogueTextColor(string character)
    {
        textDisplay.color = GameObject.Find(character).GetComponent<Character>().getDialogueColor();
    }


    //Button Functions

    // Deactivates the dialogue choice buttons
    private void deactivateButtons()
    {
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
            button.GetComponentInChildren<Text>().text = "";
            button.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    //Activates a single button
    private void activateButton(GameObject button, JsonData choice)
    {
        button.SetActive(true);
        button.GetComponentInChildren<Text>().text = choice[0][0].ToString();
        button.GetComponent<Button>().onClick.AddListener(delegate { toDoOnClick(choice); });
    }

    //When a certain button is clicked (listener to this function added on activateButton() it changes the layer, deactivates the button, and immediately prints the next line
    void toDoOnClick(JsonData choice)
    {
        currentLayer = choice[0];
        index = 1; //index = 0 is the text that is added to the button; index = 1 is the first line to be 'spoken'
        printLine();
        deactivateButtons();
    }


    //On Start, we deactivate all buttons, just as a safe-guard measure
    private void Start()
    {
        deactivateButtons();
    }

}
