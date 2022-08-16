using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CamoDetectionVisualDebug : MonoBehaviour
{
    CamoDetection camoDetection;

    [Tooltip("The object on which the CamoDetection script is applied")]
    [Header("Target")]
    [SerializeField] GameObject visionSource;

    [Space]

    [Header("Canvas Setup - Dependency Injections")]
    [SerializeField] Canvas debugUI;
    [SerializeField] RawImage generalTex;
    [SerializeField] RawImage targetTex;
    [SerializeField] RawImage previewBoxAllLayers;
    [SerializeField] RawImage previewBoxTargetLayer;
    [SerializeField] RawImage previewAveragedVision;
    [SerializeField] TMPro.TextMeshProUGUI visionSourceNameText;
    [SerializeField] TMPro.TextMeshProUGUI qualityRatingText;
    [SerializeField] TMPro.TextMeshProUGUI sensitivityIndicator;

    private void OnEnable()
    {
        debugUI.enabled = true;
        camoDetection = visionSource.GetComponent<CamoDetection>();
        visionSourceNameText.text = visionSource.name;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) ToggleDebugUI();
        sensitivityIndicator.text = camoDetection.SensitivityThreshold.ToString();
    }

    public void RefreshRefernces()
    {
        Debug.Log("Refreshing references...");
    }

    public void RefreshReferencesTrue()
    {
        generalTex.texture = camoDetection.PreviewAllLayers;
        targetTex.texture = camoDetection.PreviewTargetLayer;

        previewBoxAllLayers.texture = camoDetection.PreviewBoxAllLayers;
        previewBoxTargetLayer.texture = camoDetection.PreviewBoxTargetLayer;
        previewAveragedVision.texture = camoDetection.PreviewAveragedVision;

        qualityRatingText.text = camoDetection.CamoDetectionValue.ToString();
    }

    [ContextMenu("Toggle Debug UI")]
    private void ToggleDebugUI()
    {
        debugUI.enabled = !debugUI.enabled;
    }
}
