using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;

public class TipMessageVRA : MonoBehaviour
{
    public Exp exp_script;
    public AllPlacementVRANew place_script;

    public GameObject prompt_text;
    public GameObject mode_text;
    public GameObject state_text;

    public SteamVR_Action_Boolean switchSymbolMode;
    public GameObject image;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (switchSymbolMode.GetStateDown(SteamVR_Input_Sources.LeftHand))      // VR端开始画标识, AR端结束, 左手扳机键
        {
            if (!image.activeInHierarchy) image.SetActive(true);
            if (!mode_text.activeInHierarchy) mode_text.SetActive(true);
        }

        if (switchSymbolMode.GetStateUp(SteamVR_Input_Sources.LeftHand))
        {
            if (image.activeInHierarchy) image.SetActive(false);
            if (mode_text.activeInHierarchy) mode_text.SetActive(false);

        }

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
            (place_script.currentSymbolMode.Equals(SymbolMode.Axes) ? "Mode: Axes" : 
            (place_script.currentSymbolMode.Equals(SymbolMode.SPLIT) ? "Mode: Split" : "Mode:Oral"));

        state_text.GetComponent<Text>().text = place_script.message;
    }
}
