using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnnemyVision : MonoBehaviour
{
    [Header("Vision and Tracking Settings")]
    [SerializeField] private Camera generalView;
    [SerializeField] private Camera targetedView;
    [SerializeField] private GameObject target;

    [SerializeField] [Range(.01f, .5f)] private float bounds = .2f;
    [SerializeField] [Range(.01f, .05f)] private float boundsMargin = .05f;

    [SerializeField] private TMPro.TextMeshProUGUI qualityRatingText;

    [Header("Debugging Tools")]
    private bool debug = true;
    [SerializeField] private bool verbose = false;
    [SerializeField] private bool useCompleteView = false;
    [SerializeField] private Canvas debugUI;
    [SerializeField] private RawImage previewBoxAllLayers;
    [SerializeField] private RawImage previewBoxTargetLayer;
    [SerializeField] private RawImage previewAveragedVision;
    
    private RenderTexture generalRender;
    private RenderTexture targetRender;
    private Texture2D generalRenderTex;
    private Texture2D targetRenderTex;
    private Vector3 viewTargetPos;

    private void Start()
    {
        generalRender = generalView.targetTexture;
        targetRender = targetedView.targetTexture;
        debug = true;
        debugUI.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) ToggleDebugMode();

        viewTargetPos = generalView.WorldToViewportPoint(target.transform.position);
        if(     viewTargetPos.x > 0 && viewTargetPos.x < 1
            &&  viewTargetPos.y > 0 && viewTargetPos.y < 1  )
        {
            //if(debug) Debug.Log("Target position is in ennemy's view");
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                ReadVision();
                RateCamoQuality();
            }
        }
    }

    private void ToggleDebugMode()
    {
        debugUI.enabled = !debugUI.enabled;
        debug = !debug;
    }


    private void ReadVision()
    {
        Rect cropSource;
        int targetTexBounds;

        if(useCompleteView)
        {
            targetTexBounds = 0;

            generalRenderTex = new Texture2D(   generalRender.width,
                                                generalRender.height,
                                                TextureFormat.RGB24,
                                                true);
            
            targetRenderTex = new Texture2D(    targetRender.width,
                                                targetRender.height,
                                                TextureFormat.RGB24,
                                                true);
            
            cropSource = new Rect (0, 0, targetRender.width, targetRender.height);
        }
        else
        {
            // transform targetviewposition from 0 -> 1 coords to match render's coords
            Vector2 center = new Vector2(   (viewTargetPos.x * generalRender.width),
                                            (viewTargetPos.y * generalRender.height));

            int centerX = (int)center.x;
            int centerY = (int)center.y;
            
            // do the same for the target bounds
            targetTexBounds = (int)((bounds + boundsMargin) * generalRender.width);

            // as the current origin is top left, adapt y coords to make it from bottom left
            centerY = generalRender.height - centerY;

            if (debug && verbose)
            {
                if(debug) Debug.Log($"{viewTargetPos.x}, {viewTargetPos.y}");
                if(debug) Debug.Log($"Center Positions: {centerX}, {centerY}");
            }

            // find origin and destination of crop rectangle inside of the enemy's fov
            // and clamp it to the render texture's bounds
            int originX = centerX - targetTexBounds;
            int originY = centerY - targetTexBounds;
            if (originX < 0) originX = 0;
            if (originY < 0) originY = 0;

            int destX = centerX + targetTexBounds;
            int destY = centerX + targetTexBounds;
            if (destX > generalRender.width) destX = generalRender.width;
            if (destY > generalRender.height) destY = generalRender.height;

            if (debug && verbose)
            {
                Debug.Log($"target origins: {originX}, {originY}");
                Debug.Log($"target destinations: {destX}, {destY}");
                Debug.Log($"fed destinations: {destX - originX}, {destY + generalRender.height - originY}");
            }   

            // create the two textures with the now adapted values
            generalRenderTex = new Texture2D(   2*targetTexBounds,
                                                2*targetTexBounds,
                                                TextureFormat.RGB24,
                                                true);

            targetRenderTex = new Texture2D(    2*targetTexBounds,
                                                2*targetTexBounds,
                                                TextureFormat.RGB24,
                                                true);

            // build crop rectangle with correct coord values
            // as the Rect() cnstr uses width and not dest as an argument
            // we have to correct for it
            cropSource = new Rect(  originX,
                                    originY,
                                    destX - originX,
                                    destY + generalRender.height - originY);

        }

        // set render targets to correct sources and read their values
        RenderTexture.active = generalRender;
        generalRenderTex.ReadPixels(cropSource, 0, 0);
        if (debug) generalRenderTex.Apply();

        RenderTexture.active = targetRender;
        targetRenderTex.ReadPixels(cropSource, 0, 0);
        if (debug) targetRenderTex.Apply();

        if (debug)
        {
            Debug.Log($"bounds: {targetTexBounds}; origin: {cropSource.min}; destination: {cropSource.max}");
            Debug.Log($"cropped texture width: {generalRenderTex.width}");
            previewBoxAllLayers.texture = generalRenderTex;
            previewBoxTargetLayer.texture = targetRenderTex;
        }
    }

    void RateCamoQuality() {

        float camoRating;
        string camoRatingText;

        Color[] generalPixels = generalRenderTex.GetPixels();
        Color[] targetPixels = targetRenderTex.GetPixels();
        Color[] averagedPixels = new Color[generalPixels.Length];
        Texture2D averagedTexture = new Texture2D(  generalRenderTex.width,
                                                    generalRenderTex.height,
                                                    TextureFormat.RGB24,
                                                    true);

        int generalPixelsCount = 0;
        int targetPixelsCount = 0;

        float generalPixelsValue = 0;
        float targetPixelsValue = 0;

        // averages colors to greyscale with luminance weights
        for (int i = 0; i < generalPixels.Length; i++)
        {
            if (targetPixels[i] != new Color(0,0,0,1))
            {
                targetPixelsCount++;
                targetPixelsValue   += 0.299f * targetPixels[i].r
                                    + 0.587f * targetPixels[i].g
                                    + 0.114f * targetPixels[i].b;
            }

            generalPixelsCount++;
            generalPixelsValue  += 0.299f * generalPixels[i].r
                                + 0.587f * generalPixels[i].g
                                + 0.114f * generalPixels[i].b;
        }

        float averageTarget = targetPixelsValue / targetPixelsCount;
        float averageGeneral = generalPixelsValue / generalPixelsCount;

        camoRating = Mathf.Abs(averageGeneral - averageTarget);

        if (debug)
        {
            // writes down all relevant information
            Debug.Log($"targetPixelsCount: {targetPixelsCount}");
            Debug.Log($"generalPixelsCount: {generalPixelsCount}");
            Debug.Log($"averageTarget: {averageTarget}");
            Debug.Log($"averageGeneral: {averageGeneral}");

            // creates a texture and assigns it to the visualization in debug UI
            for (int mip = 0; mip < 3; mip++)
            {
                for (int i = 0; i < generalPixels.Length; i++)
                {
                    Color c;
                    if (targetPixels[i] == new Color (0,0,0,1))
                    {
                        c = new Color(averageGeneral, averageGeneral, averageGeneral, 1);
                    }
                    else
                    {
                        c = new Color(averageTarget, averageTarget, averageTarget, 1);

                    }
                    averagedPixels[i] = c;
                }
                averagedTexture.SetPixels(averagedPixels, mip);
            }
            averagedTexture.Apply();
            previewAveragedVision.texture = averagedTexture;
        }

        // dispatches between different levels of quality
        if (camoRating < .1f) camoRatingText = "Excellent";
        else if (camoRating < .2f) camoRatingText = "Good";
        else if (camoRating < .3f) camoRatingText = "Mediocre";
        else camoRatingText = "Poor";
        qualityRatingText.text = camoRatingText;
        Debug.Log(camoRating + " " + camoRatingText);
    }

}

// TODO
// analysis from RGB and simple luminance to HSV values
// deciding which thresholds are pertinent for each HSV channel
// refactor code afterwards to separate controls and underlying logic
// refactor to separate coordinates adaptation logic, textures grabbing and camo rating


