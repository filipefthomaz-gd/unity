using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This scripts bypass the Animator 'Head' gameobject rotation
//When a target is close (see IsHeadTarget.cs), the Animator 'head' is bypassed, and head looks at the target
public class HeadAnimationToTarget: MonoBehaviour
{
    //List of targets that are within a 'lookable' range.
    //Do not add or remove anything manually; Targets are automatically added and removed via the IsHeadTarget script
    public List<Transform> targetObj;
    public List<float> xzDistance;
    public List<Vector3> angleToTarget;

    //Head and rotation components;
    public Transform head;
    public float headRotSpeed;
    private Vector3 originalHeadRot;

    //This variables are needed in order to bypass Animator on head
    //To bypass an Animation curve, changes need to be done on LateUpdate()
    private Vector3 temporaryEuler;
    private Vector3 temporaryLocalEuler;

    // Start is called before the first frame update
    void Start()
    {
        //Setting the initial head rotations (MAY NEED TO BE PLACED IN THE FIRST LERP CALL)
        temporaryEuler = head.eulerAngles;
        temporaryLocalEuler = head.localEulerAngles;

        //Setting the original Head Rotation and Adding a null Target (this null Target is equivalent to a no target)
        originalHeadRot = head.localEulerAngles;
        targetObj.Add(null);
        xzDistance.Add(100);
        angleToTarget.Add(Vector3.zero);
    }

    // LateUpdate happens after Update (where Animator happens)
    void LateUpdate()
    {
        //Instantiate target and distance. Target = 0 is no target;
        float closerDistance = 100;
        int closerTarget = 0;

        //Check which target is closest and return that target.
        for(int i = 0; i < targetObj.Count;  i++)
        {
            if (xzDistance[i] < closerDistance)
            {
                closerDistance = xzDistance[i];
                closerTarget = i;
            }   
        }

        //Perform the actual head rotation Lerp
        headTargetMovement(targetObj[closerTarget], angleToTarget[closerTarget]);
    }

    
    //Move the head to Target or back to Original Position
    void headTargetMovement(Transform target, Vector3 angleToTarget)
    {
        //Rotation to target
        if (target != null)
        {
            
            //float localYVariation = (angleToTarget.y - head.root.eulerAngles.y - 2*360) - (Mathf.Ceil((angleToTarget.y - head.root.eulerAngles.y - 2*360) / 360)-1)*360;

            //Lerping the head movement
            //if (localYVariation < 90 || localYVariation > 270)
                head.eulerAngles = new Vector3(Mathf.LerpAngle(temporaryEuler.x, angleToTarget.x, Time.deltaTime * headRotSpeed),
                   Mathf.LerpAngle(temporaryEuler.y, angleToTarget.y, Time.deltaTime * headRotSpeed),
                   0.0f);
            /*else
                head.localEulerAngles = new Vector3(Mathf.LerpAngle(temporaryLocalEuler.x, originalHeadRot.x, Time.deltaTime * headRotSpeed/2),
                 Mathf.LerpAngle(temporaryLocalEuler.y, originalHeadRot.y, Time.deltaTime * headRotSpeed/2),
                 0.0f);*/


        }

        //Move back to original position if no target exist
        else if(Mathf.Abs(temporaryLocalEuler.x-originalHeadRot.x) > 0.01f &&
            Mathf.Abs(temporaryLocalEuler.y - originalHeadRot.y) > 0.01f)
        {
            head.localEulerAngles = new Vector3(Mathf.LerpAngle(temporaryLocalEuler.x, originalHeadRot.x, Time.deltaTime * headRotSpeed/2),
                 Mathf.LerpAngle(temporaryLocalEuler.y, originalHeadRot.y, Time.deltaTime * headRotSpeed/2),
                 0.0f);
        }

        //Record the actual rotation of the head. It's going to be replaced in the following Update, so we need a copy of it.
        temporaryEuler = head.eulerAngles;
        temporaryLocalEuler = head.localEulerAngles;
        
    }


}
