using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipMessageVRA : MonoBehaviour
{
    private Exp myExp;
    public GameObject Text;

    // Start is called before the first frame update
    void Start()
    {
        myExp = GetComponentInParent<Exp>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!(Text.activeSelf ^ myExp.GetVRExpState()))
        {
            Text.SetActive(!myExp.GetVRExpState());
        }
    }
}
