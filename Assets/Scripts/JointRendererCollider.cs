using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JointRendererCollider : MonoBehaviour
{
    public Situations currentSituation;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall" && currentSituation != Situations.END_LEVEL)
        {
            currentSituation = Situations.WALL_TRIGGERED;
        }

        if (other.tag == "GrayObstacle" && currentSituation != Situations.END_LEVEL)
        {
            currentSituation = Situations.GREY_TRIGGERED;
        }

        if (other.tag == "RedObstacle" && currentSituation != Situations.END_LEVEL)
        {
            currentSituation = Situations.RED_TRIGGERED;
        }

        if (other.tag == "FinishLine")
        {
            currentSituation = Situations.END_LEVEL;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "GrayObstacle")
        {
            currentSituation = Situations.WALL_TRIGGERED;
        }

        if (other.tag == "RedObstacle")
        {
            currentSituation = Situations.WALL_TRIGGERED;
        }

        if (other.tag == "Wall")
        {
            currentSituation = Situations.NOT_TRIGGERED;
        }


    }
}
