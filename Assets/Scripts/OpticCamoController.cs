using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OpticCamo))]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Material))]
public class OpticCamoController : MonoBehaviour
{
    [Header("Fade between colors effect")]
    [SerializeField] private float fadeSpeed = 1f;

    [Header("Flash effect settings")]
    [SerializeField] private bool enableFlashEffect = true;
    [SerializeField] private float flashSpeed = .2f;

    [Header("Flash effect colors")]
    [SerializeField] private Color updateCamoFlash;
    [SerializeField] private Color switchCamoTexFlash;
    [SerializeField] private Color rollCamoFlash;

    [Header("Enable verbose debugging")]
    [SerializeField] private bool debug = false;

    private Shader opticCamoShader;
    private OpticCamo opticCamo;
    private bool isShaderCorrect;

    private void Start()
    {
        opticCamo = GetComponent<OpticCamo>();
        opticCamoShader = GetComponent<Renderer>().material.shader;
        isShaderCorrect = opticCamoShader == Shader.Find("Shader Graphs/TestCamo");

        if(!isShaderCorrect)
        {
            Debug.Log("WARNING: Material's shader is either missing or not a TestCamo shader.");
        } 
    }

    private void Update()
    {
        if (isShaderCorrect)
        {
            if (Input.GetKeyDown(KeyCode.Space))  UpdateCamoColors();
            if (Input.GetKeyDown(KeyCode.Tab)) RollCamoColors();
            if (Input.GetKeyDown(KeyCode.Return)) SwitchCamoTex();

        }
    }

    private void UpdateCamoColors()
    {
        opticCamo.PickColors(debug);
        opticCamo.SendColors();

        if(enableFlashEffect) opticCamo.Flash(updateCamoFlash, flashSpeed, debug);
        opticCamo.FadeColors(fadeSpeed, debug);
    }

    private void SwitchCamoTex()
    {
        if(enableFlashEffect) opticCamo.Flash(switchCamoTexFlash, flashSpeed, debug);
        opticCamo.UpdateCurrentTexture();
    }

    private void RollCamoColors()
    {
        opticCamo.RollColors();
        opticCamo.SendColors();

        if(enableFlashEffect) opticCamo.Flash(rollCamoFlash, flashSpeed, debug);
        opticCamo.FadeColors(fadeSpeed, debug);
    }

}
