using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnnemyVision : MonoBehaviour
{
    [SerializeField] private Camera generalView;
    [SerializeField] private Camera targetedView;
    [SerializeField] private GameObject target;

    [SerializeField] [Range(.01f, .5f)] private float bounds = .2f;
    [SerializeField] [Range(.01f, .05f)] private float boundsMargin = .05f;

    [SerializeField] private bool debug;
    [SerializeField] private bool useCompleteView;

    [SerializeField] private RawImage previewGeneral;
    [SerializeField] private RawImage previewTargeted;

    private RenderTexture generalRender;
    private RenderTexture targetRender;
    private Texture2D generalRenderTex;
    private Texture2D targetRenderTex;
    private Vector3 viewTargetPos;

    private void Start()
    {
        generalRender = generalView.targetTexture;
        targetRender = targetedView.targetTexture;
    }

    private void Update()
    {
        viewTargetPos = generalView.WorldToViewportPoint(target.transform.position);
        if(     viewTargetPos.x > 0 && viewTargetPos.x < 1
            &&  viewTargetPos.y > 0 && viewTargetPos.y < 1  )
        {
            //if(debug) Debug.Log("Target position is in ennemy's view");
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                ReadVision();
            }
        }
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

            if(debug) Debug.Log($"{viewTargetPos.x}, {viewTargetPos.y}");
            if(debug) Debug.Log($"Center Positions: {centerX}, {centerY}");

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

            if (debug)
            {
                Debug.Log($"fed origins: {originX}, {originY}");
                Debug.Log($"fed destinations: {destX}, {destY}");
                Debug.Log($"wrongly adapted destinations: {destX - originX}, {destY - originY}");
                Debug.Log($"correct destinations: {destX - originX}, {destY + generalRender.height - originY}");
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
        generalRenderTex.Apply();

        RenderTexture.active = targetRender;
        targetRenderTex.ReadPixels(cropSource, 0, 0);
        targetRenderTex.Apply();
        
        if(debug) Debug.Log($"bounds: {targetTexBounds}; origin: {cropSource.min}; destination: {cropSource.max}");
        if(debug) Debug.Log($"cropped texture width: {generalRenderTex.width}");

        previewGeneral.texture = generalRenderTex;
        previewTargeted.texture = targetRenderTex;

        Color[] generalPixels = generalRenderTex.GetPixels();
        Color[] targetPixels = targetRenderTex.GetPixels();

        int generalPixelsCount = 0;
        int targetPixelsCount = 0;

        float generalPixelsValue = 0;
        float targetPixelsValue = 0;

        for (int i = 0; i < generalPixels.Length; i++)
        {
            if (targetPixels[i] != new Color(0,0,0,1))
            {
                targetPixelsCount++;
                targetPixelsValue += targetPixels[i].r + targetPixels[i].g + targetPixels[i].b;
            }

            generalPixelsCount++;
            generalPixelsValue += generalPixels[i].r + generalPixels[i].g + generalPixels[i].b;
        }

        float averageTarget = targetPixelsValue / targetPixelsCount;
        float averageGeneral = generalPixelsValue / generalPixelsCount;

        // if (debug)
        // {
        //     Debug.Log($"targetPixelsCount: {targetPixelsCount}");
        //     Debug.Log($"generalPixelsCount: {generalPixelsCount}");
        //     Debug.Log($"averageTarget: {averageTarget}");
        //     Debug.Log($"averageGeneral: {averageGeneral}");

        // }

    }

        // todo list
        // //  fetch render textures for both the general view that renders everything
        // //  and the targeted view that only renders the player

        // //  on action, read both pixels from both textures
        //  adapt size of bounding box according to target's distance from camera
        //  create a buffer array for the player's texture as to store only initialized pixels (?)
        //  average both textures to a single big float
        //  compare both floats and decide of a camo threshold efficiency


}
