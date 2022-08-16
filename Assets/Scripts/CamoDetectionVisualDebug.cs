using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CamoDetectionVisualDebug : MonoBehaviour
{
    [Tooltip("The object on which the CamoDetection script is applied")]
    [Header("Target")]
    [SerializeField] GameObject visionSource;
    private CamoDetection camoDetection;

    [Space]

    [Header("Canvas Setup - Dependency Injections")]
    [SerializeField] private Canvas debugUI;
    [SerializeField] private RawImage generalTex;
    [SerializeField] private RawImage targetTex;
    [SerializeField] private RawImage previewBoxAllLayers;
    [SerializeField] private RawImage previewBoxTargetLayer;
    [SerializeField] private RawImage previewAveragedVision;
    [SerializeField] private TMPro.TextMeshProUGUI visionSourceNameText;
    [SerializeField] private TMPro.TextMeshProUGUI qualityRatingText;

    private void OnEnable()
    {
        camoDetection = visionSource.GetComponent<CamoDetection>();
        visionSourceNameText.text = visionSource.name;
    }

    public void RefreshReferences()
    {
        generalTex.texture = camoDetection.PreviewAllLayers;
        targetTex.texture = camoDetection.PreviewBoxTargetLayer;

        previewBoxAllLayers.texture = camoDetection.PreviewBoxAllLayers;
        previewBoxTargetLayer.texture = camoDetection.PreviewBoxTargetLayer;
        previewAveragedVision.texture = camoDetection.PreviewAveragedVision;
    }

    [ContextMenu("Toggle Debug UI")]
    private void ToggleDebugUI()
    {
        debugUI.enabled = !debugUI.enabled;
    }
}
