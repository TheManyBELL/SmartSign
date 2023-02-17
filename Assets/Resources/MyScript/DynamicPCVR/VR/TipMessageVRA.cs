using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipMessageVRA : MonoBehaviour
{
    public Exp exp_script;
    public AllPlacementVRANew place_script;

    public GameObject prompt_text;
    public GameObject mode_text;
    public GameObject state_text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!exp_script)
        {
            if (!GameObject.Find("SmartSignA(Clone)/VR")) return;

            exp_script = GameObject.Find("SmartSignA(Clone)/VR").GetComponent<Exp>();
            place_script = GameObject.Find("SmartSignA(Clone)/VR").GetComponent<AllPlacementVRANew>();
        }

        if (!(prompt_text.activeSelf ^ exp_script.GetVRExpState()))
        {
            prompt_text.SetActive(!exp_script.GetVRExpState());
        }

        mode_text.GetComponent<Text>().text =
            place_script.currentSymbolMode.Equals(SymbolMode.ARROW) ? "Mode: Line" :
            (place_script.currentSymbolMode.Equals(SymbolMode.Axes) ? "Mode: Axes" : "Mode: Split");
        
    }
}
