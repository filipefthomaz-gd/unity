using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    /*
      -- This Script controls the footstep audioclips and footstep volume of the character per trigger;
      Could make a single manager that received multiple collider triggers and multiple audioclip sets;
      For now you add this script to a GameObject with a single collider trigger and you can set its footsteps while the character in contact with it;
      The actual Play footstep audio function is in ControlManager, attached to the player;
      Without or when exiting a trigger, if no other exists, revert to default footsteps (Also added on ControlManager) --
    */

    //Footstep Audio Set and Volume to be played in this trigger
    //AudioClips will be randomized for natural sounding (see ControlManager)
    public AudioClip[] footstepClips;
    public float footstepVolumeFactor;
    public string playerName = "John";

    private ControlManager controlManager;
    private AudioClip[] tempGrass;
    private float tempVolumeFactor;

    //Change ControlManager audioClips and Volume when entering and exiting the collider trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == playerName)
        {
            controlManager.footstep = footstepClips;
            controlManager.footstepVolumeFactor = footstepVolumeFactor;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == playerName)
        {
            controlManager.footstep = tempGrass;
            controlManager.footstepVolumeFactor = tempVolumeFactor;
        }
    }

    //Load the default audioClips and Volume to write them back into ControlManager if needed
    void Start()
    {
        controlManager = GameObject.Find(playerName).GetComponent<ControlManager>();
        tempGrass = controlManager.footstep;
        tempVolumeFactor = controlManager.footstepVolumeFactor;
    }

}
