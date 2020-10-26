using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

public class StateClass : MonoBehaviour
{
    public static StateClass sc;

    //Dont Destroy on Load
    void Awake()
    {
        if (sc == null)
        {
            DontDestroyOnLoad(gameObject);
            sc = this;
        }
        else if (sc != this)
        {
            Destroy(gameObject);
        }
    }

    //Class with object info to be stored
    [System.Serializable]
    public class StateData
    {
        public string scene;
        public string id;
        public int audio;
        public int animation;
        public int dialogue;
        public int pickup;
        public int light;
        public int[] dependency;
        public Vector3 position;

    }

    public List<StateData> saveListData = new List<StateData>();


    //JSON Add-On
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.GameObjects;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.GameObjects = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.GameObjects = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] GameObjects;
        }
    }

    private string allData;

    /* ---
      Most information that needs to be saved is non-continuous, and can be reduced to a state system;
      All components that need the state saved have a Public State Variable and an algorithm to determine its value;
      These State values are what is being saved to a file through JSON.
    ---*/


    //SAVE AND LOAD TO/FROM FILE

    //Save occurs on scene changes and on GoToMenu() -> SceneChange and TrackScene classes
    //Load gets the last scene where the player was (after Scene Change or when Menu), but positions the player in the 'beginning' of the scene
    public void saveDataToFile(string savefile, string scene, Vector3 position)
    {
        string path = Application.persistentDataPath + "/" + savefile;

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(scene);
        writer.WriteLine(DateTime.Now);
        writer.WriteLine(position);
        writer.WriteLine(allData);
        writer.Close();

        Debug.Log("Saved");
        //Re-import the file to update the reference in the editor
        //AssetDatabase.ImportAsset(path);
        //TextAsset asset = Resources.Load<TextAsset>("Save/save_state");

        //Print the text from the file
        //Debug.Log(asset.text);
    }

    private string position;

    public string readSceneFromFile(string loadFile)
    {
        string path = Application.persistentDataPath + "/" + loadFile;
        string scene = "";

        try
        {
            StreamReader reader = new StreamReader(path);
            scene = reader.ReadLine();
            reader.Close();
        }
        catch (Exception e)
        {
            scene = "error";
        }

        return scene;
    }

    public string[] readDataFromFile(string loadFile)
    {
        string path = Application.persistentDataPath + "/" + loadFile;
        string[] sceneAndDate = new string[2];
        position = "";
        //Read the text from directly from the test.txt file
        try
        {
            StreamReader reader = new StreamReader(path);
            sceneAndDate[0] = reader.ReadLine();
            sceneAndDate[1] = reader.ReadLine();
            position = reader.ReadLine();
            allData = reader.ReadLine();
            reader.Close();

            //Convert read List allData to a StateData[] again
            StateData[] sdata = JsonHelper.FromJson<StateData>(allData);
            saveListData = new List<StateData>(sdata);
        }
        catch(Exception e)
        {
            sceneAndDate[0] = "error";
        }

        return sceneAndDate;
    }



    public string getPosition()
    {
        return position;
    }

    //NOT BEING USED! Save is being made individually, imediatelly after any state has been changed
    //Go to 'saveState()'
    //Saves all states of the active scene (from rootobjects) at once
    public void saveData()
    {
        //Calls every root object in the current active scene
        foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            StateData saveData = new StateData();

            //Starts constructing saveData class with scene name and game object name (id);
            saveData.scene = SceneManager.GetActiveScene().name;
            saveData.id = g.name;

            //Searches every saved element to see if the element that its trying to save has already been saved
            for (int i = 0; i < saveListData.Count; i++)
            {
                if (saveListData[i].id == saveData.id && saveListData[i].scene == SceneManager.GetActiveScene().name)
                    saveListData.Remove(saveListData[i]); //Removes the saved state
            }

            //Checks for State components and constructs the rest of saveData class
            if (g.GetComponent<AudioState>() != null)
                saveData.audio = g.GetComponent<AudioState>().state;
            if(g.GetComponent<DialogTrigger>() != null)
                saveData.dialogue = g.GetComponent<DialogTrigger>().state;
            if (g.GetComponent<AnimationState>() != null)
                saveData.animation = g.GetComponent<AnimationState>().state;
            if (g.GetComponent<PickUp>() != null)
                saveData.pickup = g.GetComponent<PickUp>().state;
            if (g.GetComponent<TurnLightOff>() != null)
                saveData.light = g.GetComponent<TurnLightOff>().state;
            if (g.GetComponent<StateDependency>() != null)
                for (int i = 0; i < g.GetComponent<StateDependency>().states.Length; i++)
                    saveData.dependency[i] = g.GetComponent<StateDependency>().states[i];

            //Only adds to the list objects that have been saved for lesser memory usage
            if (saveData.audio != 0 || saveData.animation != 0 || saveData.dialogue != 0 || saveData.pickup != 0 ||
                saveData.dependency[0] != 0)
                saveListData.Add(saveData);
        }

        //Serializes the list to JSON
        allData = JsonHelper.ToJson(saveListData.ToArray());

    }


    //Load Data (Called in ChangeScene class right after the scene load)
    public void loadData()
    {
        //De-serializes from JSON
        if (allData != null)
        {
            StateData[] loadData = JsonHelper.FromJson<StateData>(allData);

            //For every element in the list loads the state from the class
            //CHANGE: TO USE LESSER MEMORY, LOOP FOR ALL ACTIVE SCENE GAME OBJECTS AND SEE IF MATCHES ANY ELEMENT ON THE LIST
            for (int i = 0; i < loadData.Length; i++)
            {
                if(GameObject.Find(loadData[i].id) != null)
                {
                    if (GameObject.Find(loadData[i].id).GetComponent<PickUp>() != null)
                        GameObject.Find(loadData[i].id).GetComponent<PickUp>().state = loadData[i].pickup;
                }

                if (GameObject.Find(loadData[i].id) != null && loadData[i].scene == SceneManager.GetActiveScene().name)
                {
                    if (GameObject.Find(loadData[i].id).GetComponent<AudioState>() != null)
                        GameObject.Find(loadData[i].id).GetComponent<AudioState>().state = loadData[i].audio;
                    if (GameObject.Find(loadData[i].id).GetComponent<DialogTrigger>() != null)
                        GameObject.Find(loadData[i].id).GetComponent<DialogTrigger>().state = loadData[i].dialogue;
                    if (GameObject.Find(loadData[i].id).GetComponent<AnimationState>() != null)
                        GameObject.Find(loadData[i].id).GetComponent<AnimationState>().state = loadData[i].animation;
                    if (GameObject.Find(loadData[i].id).GetComponent<TurnLightOff>() != null)
                        GameObject.Find(loadData[i].id).GetComponent<TurnLightOff>().state = loadData[i].light;
                    if (GameObject.Find(loadData[i].id).GetComponent<PositionSave>() != null)
                        GameObject.Find(loadData[i].id).GetComponent<Transform>().position = loadData[i].position;
                    if (GameObject.Find(loadData[i].id).GetComponent<StateDependency>() != null)
                        for (int j = 0; j < loadData[i].dependency.Length; j++)
                            GameObject.Find(loadData[i].id).GetComponent<StateDependency>().states[j] = loadData[i].dependency[j];
                }
            }
        }
    }

    public void resetSave()
    {
        saveListData.Clear();
        allData = null;
    }


    //Changes state individually by game object in 'real-time'
    //Being called everytime a state is changed (or the S function with true trigger called) by each object
    //Only the 'type' should be used, but due to dependencies and no impact in performance, there is some redundancy;
    //See saveData
    public void saveState(GameObject g, string type)
    {
        StateData saveData = new StateData();
        saveData.scene = SceneManager.GetActiveScene().name;
        saveData.id = g.name;

        for (int i = 0; i < saveListData.Count; i++)
        {
            if (saveListData[i].id == saveData.id && saveListData[i].scene == SceneManager.GetActiveScene().name)
                saveListData.Remove(saveListData[i]);
        }

        if (g.GetComponent<AudioState>() != null)
            saveData.audio = g.GetComponent<AudioState>().state;
        if (g.GetComponent<DialogTrigger>() != null)
            saveData.dialogue = g.GetComponent<DialogTrigger>().state;
        if (g.GetComponent<AnimationState>() != null)
            saveData.animation = g.GetComponent<AnimationState>().state;
        if (g.GetComponent<PickUp>() != null)
            saveData.pickup = g.GetComponent<PickUp>().state;
        if (g.GetComponent<TurnLightOff>() != null)
            saveData.light = g.GetComponent<TurnLightOff>().state;
        if (g.GetComponent<PositionSave>() != null)
            saveData.position = g.GetComponent<PositionSave>().position;
        if (g.GetComponent<StateDependency>() != null)
        {
            saveData.dependency = new int[g.GetComponent<StateDependency>().states.Length];
            for (int i = 0; i < g.GetComponent<StateDependency>().states.Length; i++)
                saveData.dependency[i] = g.GetComponent<StateDependency>().states[i];
        }

        saveListData.Add(saveData);
        allData = JsonHelper.ToJson(saveListData.ToArray());
        //Debug.Log(allData);
    }


    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }


    // Start is called before the first frame update
    void Start()
    {
        allData = JsonHelper.ToJson(saveListData.ToArray());
    }

}
