using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsHeadTarget : MonoBehaviour
{
    public HeadAnimationToTarget headLookTo;
    public Transform head;
    public float minimumDistance;
    private Vector3 angleToTarget;
    private float xzDistance;
    private bool isInTargetList;
    private float localYVariation;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        headTargetMovement();

        //If is close to character
        if (xzDistance < minimumDistance && (localYVariation < 90 || localYVariation > 270))
        {
            //Loop through all targets close to player
            for (int i = 0; i < headLookTo.targetObj.Count; i ++)
            {
                //If one of them is already this target, then just update the distance and angle
                if (headLookTo.targetObj[i] == transform)
                {
                    isInTargetList = true;
                    headLookTo.xzDistance[i] = xzDistance;
                    headLookTo.angleToTarget[i] = angleToTarget;
                    return;
                }
            }

            //If not yet on the target list, append it
            if (!isInTargetList)
            {
                headLookTo.targetObj.Add(transform);
                headLookTo.angleToTarget.Add(angleToTarget);
                headLookTo.xzDistance.Add(xzDistance);
            }
        }

        //If not close to the character, should remove from TargetList
        else
        {
            headLookTo.targetObj.Remove(transform);
            headLookTo.angleToTarget.Remove(angleToTarget);
            headLookTo.xzDistance.Remove(xzDistance);
            isInTargetList = false;
        }

    }

    //Determine angle that head needs to turn to line up with this specific target
    void headTargetMovement()
    {
        //Distance from player to target
        float xDistance = transform.position.x - head.position.x;
        float yDistance = transform.position.y - head.position.y;
        float zDistance = transform.position.z - head.position.z;
        xzDistance = Mathf.Sqrt(Mathf.Pow(xDistance, 2) + Mathf.Pow(zDistance, 2));

        //World Angle to target
        angleToTarget.y = Mathf.Atan(xDistance / zDistance);
        angleToTarget.y *= Mathf.Rad2Deg;
        if (zDistance < 0 && xDistance > 0)
            angleToTarget.y += 180;

        else if (zDistance < 0 && xDistance < 0)
            angleToTarget.y -= 180;

        angleToTarget.x = Mathf.Atan(yDistance / xzDistance);
        angleToTarget.x *= -Mathf.Rad2Deg;


        angleToTarget.z = 0;

        localYVariation = (angleToTarget.y - head.root.eulerAngles.y - 2 * 360) - (Mathf.Ceil((angleToTarget.y - head.root.eulerAngles.y - 2 * 360) / 360) - 1) * 360;

    }
}
