using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceableObjectHandler : MonoBehaviour
{
    bool active = false;

    void onBounceStart() {
        active = true;
    }

    void onBounceStop() {
        active = false;
    }
}
