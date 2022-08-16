using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnnemyVision : MonoBehaviour
{
    #region private fields
    private enum RatingMode {Luminance, HSV};
    private RenderTexture generalRender;
    private RenderTexture targetRender;
    private Texture2D generalRenderTex;
    private Texture2D targetRenderTex;
    private Vector3 viewTargetPos;
    private bool debug;
    #endregion

    #region inspected fields
    [Header("Vision and Tracking Settings")]
    [SerializeField] RatingMode ratingMode;
    [SerializeField] [Range(.01f, .5f)] private float bounds = .2f;
    [SerializeField] [Range(.01f, .05f)] private float boundsMargin = .05f;

    [Header("Debugging Settings")]
    [SerializeField] private bool verbose = false;
    [SerializeField] private bool useCompleteView = false;
    #endregion

    #region dependency injections
    [Header("Dependency Injections / General [Compulsory]")]
    [SerializeField] private Camera generalView;
    [SerializeField] private Camera targetedView;
    [SerializeField] private GameObject target;
    [SerializeField] private TMPro.TextMeshProUGUI qualityRatingText;

    [Header("Dependency Injections / Debugging Tools [Recommended]")]
    [SerializeField] private Canvas debugUI;
    [SerializeField] private RawImage previewBoxAllLayers;
    [SerializeField] private RawImage previewBoxTargetLayer;
    [SerializeField] private RawImage previewAveragedVision;
    #endregion

    #region properties
    public float CamoDetectionValue { get; private set; }
    #endregion
    
    #region event methods
    private void Start()
    {
        generalRender = generalView.targetTexture;
        targetRender = targetedView.targetTexture;
        debug = true;
        debugUI.enabled = true;
        CamoDetectionValue = 0;
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
                CamoDetectionValue = RateCamoQuality();
            }
        }
    }
    #endregion

    #region private methods
    private void ToggleDebugMode()
    {
        debugUI.enabled = !debugUI.enabled;
        debug = !debug;
    }


    private void ReadVision()
    {
        Rect cropSource;
        int targetTexBounds;
        int texWidth, texHeight;
        int originX = 0;
        int originY = 0;
        int cropSourceWidth, croupsourceHeight;

        if(useCompleteView)
        {
            targetTexBounds = 0;
            texWidth = generalRender.width;
            texHeight = generalRenderTex.height;
            cropSourceWidth = targetRender.width;
            croupsourceHeight = targetRender.height;
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
            originX = centerX - targetTexBounds;
            originY = centerY - targetTexBounds;
            if (originX < 0) originX = 0;
            if (originY < 0) originY = 0;

            int destX = centerX + targetTexBounds;
            int destY = centerX + targetTexBounds;
            if (destX > generalRender.width) destX = generalRender.width;
            if (destY > generalRender.height) destY = generalRender.height;

            cropSourceWidth = destX - originX;
            croupsourceHeight = destY + generalRender.height - originY;

            texWidth =  2*targetTexBounds;
            texHeight = 2*targetTexBounds;

            if (debug && verbose)
            {
                Debug.Log($"target origins: {originX}, {originY}");
                Debug.Log($"target destinations: {destX}, {destY}");
                Debug.Log($"fed destinations: {destX - originX}, {destY + generalRender.height - originY}");
            }   
        }

        // create the two textures with the now adapted values
        generalRenderTex = new Texture2D(   texWidth,
                                            texHeight,
                                            TextureFormat.RGB24,
                                            true);

        targetRenderTex = new Texture2D(    texWidth,
                                            texHeight,
                                            TextureFormat.RGB24,
                                            true);
        
        // creates the crop rectangle inside of the view
        // from which it is going to pass data to the two
        // new textures just created
        cropSource = new Rect(  originX,
                                originY,
                                cropSourceWidth,
                                croupsourceHeight);
        
        // sets render targets to correct sources and passes their values
        RenderTexture.active = generalRender;
        generalRenderTex.ReadPixels(cropSource, 0, 0);
        if (debug) generalRenderTex.Apply();

        RenderTexture.active = targetRender;
        targetRenderTex.ReadPixels(cropSource, 0, 0);
        if (debug) targetRenderTex.Apply();

        // logs bunches of debugging data
        if (debug && verbose)
        {
            Debug.Log($"bounds: {targetTexBounds}; origin: {cropSource.min}; destination: {cropSource.max}");
            Debug.Log($"cropped texture width: {generalRenderTex.width}");
            previewBoxAllLayers.texture = generalRenderTex;
            previewBoxTargetLayer.texture = targetRenderTex;
        }
    }

    private float RateCamoQuality() {

        float camoRating;
        string camoRatingText;

        Color[] generalPixels = generalRenderTex.GetPixels();
        Color[] targetPixels = targetRenderTex.GetPixels();
        Color[] averagedPixels = new Color[generalPixels.Length];

        int generalPixelsCount = 0;
        int targetPixelsCount = 0;

        Color averageGeneral;
        Color averageTarget;

        if (ratingMode == RatingMode.Luminance) camoRating = RateBasedOnLuminance();
        else camoRating = RateBasedOnHSV();

        if (debug) DisplayAveragedColors();

        return camoRating;
        

        #region local functions
        float RateBasedOnHSV()
        {
            float camoRatingHSV = 0;

            float generalPixelsHue = 0;
            float generalPixelsSaturation = 0;
            float generalPixelsValue = 0;

            float targetPixelsHue = 0;
            float targetPixelsSaturation = 0;
            float targetPixelsValue = 0;

            for (int i = 0; i < generalPixels.Length; i++)
            {
                // accumulates values for the background minus target
                if (generalPixels[i] != targetPixels[i])
                {
                    float gH, gS, gV;

                    generalPixelsCount++;

                    Color.RGBToHSV(generalPixels[i], out gH, out gS, out gV);
                    generalPixelsHue += gH;
                    generalPixelsSaturation += gS;
                    generalPixelsValue += gV;
                }

                // accumulates values for the target only
                if (targetPixels[i] != new Color(0,0,0,1))
                {
                    float tH, tS, tV;

                    targetPixelsCount++;

                    Color.RGBToHSV(targetPixels[i], out tH, out tS, out tV);
                    targetPixelsHue += tH;
                    targetPixelsSaturation += tS;
                    targetPixelsValue += tV;
                }
            }

            // averages the accumulated values by the number of pixels
            generalPixelsHue /= generalPixelsCount;
            generalPixelsSaturation /= generalPixelsCount;
            generalPixelsValue /= generalPixelsCount;

            targetPixelsHue /= targetPixelsCount;
            targetPixelsSaturation /= targetPixelsCount;
            targetPixelsValue /= targetPixelsCount;

            // casts the colors and assigns them to the preview colors
            averageGeneral = Color.HSVToRGB(generalPixelsHue,
                                            generalPixelsSaturation,
                                            generalPixelsValue);

            averageTarget = Color.HSVToRGB( targetPixelsHue,
                                            targetPixelsSaturation,
                                            targetPixelsValue);

            // assigns ratings to the difference between background and target averages
            float camoRatingH = Mathf.Abs(generalPixelsHue - targetPixelsHue);
            float camoRatingS = Mathf.Abs(generalPixelsSaturation - targetPixelsSaturation);
            float camoRatingV = Mathf.Abs(generalPixelsValue - targetPixelsValue);

            // simple rating based on HSV averages
            camoRatingHSV = (camoRatingH + camoRatingS + camoRatingV) / 3;

            return camoRatingHSV;
        }

        // averages colors to greyscale with luminance weights
        float RateBasedOnLuminance()
        {
            float camoRatingLum = 0;

            float generalPixelsAverageLum = 0;
            float targetPixelsAverageLum = 0;

            for (int i = 0; i < generalPixels.Length; i++)
            {
                if (targetPixels[i] != new Color(0,0,0,1))
                {
                    targetPixelsCount++;
                    targetPixelsAverageLum     += 0.299f * targetPixels[i].r
                                                + 0.587f * targetPixels[i].g
                                                + 0.114f * targetPixels[i].b;
                }

                generalPixelsCount++;
                generalPixelsAverageLum    += 0.299f * generalPixels[i].r
                                            + 0.587f * generalPixels[i].g
                                            + 0.114f * generalPixels[i].b;
            }

            float averageTargetLum = targetPixelsAverageLum / targetPixelsCount;
            float averageGeneralLum = generalPixelsAverageLum / generalPixelsCount;

            averageGeneral = new Color(averageGeneralLum, averageGeneralLum, averageGeneralLum, 1);
            averageTarget = new Color(averageTargetLum, averageTargetLum, averageTargetLum, 1);

            camoRatingLum = Mathf.Abs(averageGeneralLum - averageTargetLum);

            if (debug && verbose)
            {
                Debug.Log($"targetPixelsCount: {targetPixelsCount}");
                Debug.Log($"generalPixelsCount: {generalPixelsCount}");
                Debug.Log($"averageTarget: {averageTargetLum}");
                Debug.Log($"averageGeneral: {averageGeneralLum}");
            }

            return camoRatingLum;
        }

        // creates a texture and assigns it to the visualization in debug UI
        void DisplayAveragedColors()
        {
            Texture2D averagedTexture = new Texture2D(  generalRenderTex.width,
                                                        generalRenderTex.height,
                                                        TextureFormat.RGB24,
                                                        true);

            for (int mip = 0; mip < 3; mip++)
            {
                for (int i = 0; i < generalPixels.Length; i++)
                {
                    Color c;
                    if (targetPixels[i] == new Color (0,0,0,1))
                    {
                        c = averageGeneral;
                    }
                    else
                    {
                        c = averageTarget;
                    }
                    averagedPixels[i] = c;
                }
                averagedTexture.SetPixels(averagedPixels, mip);
            }
            averagedTexture.Apply();
            previewAveragedVision.texture = averagedTexture;
            
            // dispatches between different levels of quality
            if (camoRating < .1f) camoRatingText = "Excellent";
            else if (camoRating < .2f) camoRatingText = "Good";
            else if (camoRating < .3f) camoRatingText = "Mediocre";
            else camoRatingText = "Poor";
            qualityRatingText.text = camoRatingText;
            
            if (debug && verbose) Debug.Log(camoRating + " " + camoRatingText);
        }
        #endregion
    }
    #endregion

}


// TODO
// // analysis from RGB and simple luminance to HSV values
// deciding which thresholds are pertinent for each HSV channel
// refactor code afterwards to separate controls and underlying logic
// refactor to separate coordinates adaptation logic, textures grabbing and camo rating


