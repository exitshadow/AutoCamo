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
            // if(debug) Debug.Log("Target position is in ennemy's view");
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                ReadVision();
            }
        }
    }



    private void ReadVision()
    {
        if(useCompleteView)
        {
            generalRenderTex = new Texture2D(   generalRender.width,
                                                generalRender.height,
                                                TextureFormat.RGB24,
                                                true);
            
            targetRenderTex = new Texture2D(    targetRender.width,
                                                targetRender.height,
                                                TextureFormat.RGB24,
                                                true);
            
            Rect cropSource = new Rect (0, 0, targetRender.width, targetRender.height);

            RenderTexture.active = generalRender;
            generalRenderTex.ReadPixels(cropSource, 0, 0);

            RenderTexture.active = targetRender;
            targetRenderTex.ReadPixels(cropSource, 0, 0);

            if(debug) Debug.Log($"bounds: none; origin: {cropSource.min}; destination: {cropSource.max}");
            if(debug) Debug.Log($"cropped texture width: {generalRenderTex.width}");
        }
        else
        {
            // transform targetviewposition from 0 -> 1 coords to match render's coords
            Vector2 center = new Vector2(   (viewTargetPos.x * generalRender.width),
                                            (viewTargetPos.y * generalRender.height));
            
            if(debug) Debug.Log(center);

            int centerX = (int)center.x;
            int centerY = (int)center.y;
            if(debug) Debug.Log($"{centerX}, {centerY}");
            // do the same for the target bounds
            int targetTexBounds = (int)((bounds + boundsMargin) * generalRender.width);


            // find origin and destination of crop rectangle inside of the enemy's fov
            int originX = centerX - targetTexBounds;
            int originY = centerY - targetTexBounds;
            if (originX < 0) originX = 0;
            if (originY < 0) originY = 0;
            if (debug) Debug.Log($"{originX}, {originY}");

            int destX = centerX + targetTexBounds;
            int destY = centerX + targetTexBounds;
            if (destX > generalRender.width) destX = generalRender.width;
            if (destY > generalRender.height) destX = generalRender.height;

            // create the two textures with the now adapted values
            generalRenderTex = new Texture2D(   destX - originX,
                                                destY - originY,
                                                TextureFormat.RGB24,
                                                true);

            targetRenderTex = new Texture2D(    destX - originX,
                                                destY - originY,
                                                TextureFormat.RGB24,
                                                true);

            // build crop rectangle with correct coord values
            Rect cropSource = new Rect(originX, originY, destX, destY);

            // set render targets to correct sources and read their values
            RenderTexture.active = generalRender;
            generalRenderTex.ReadPixels(cropSource, 0,0);

            RenderTexture.active = targetRender;
            targetRenderTex.ReadPixels(cropSource, 0, 0);
            
            if(debug) Debug.Log($"bounds: {targetTexBounds}; origin: {originX}, {originY}; destination: {destX}, {destY}");
            if(debug) Debug.Log($"cropped texture width: {generalRenderTex.width}");
        }

        if (debug) previewGeneral.texture = generalRenderTex;
        if (debug) previewTargeted.texture = targetRenderTex;

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

        if (debug)
        {
            Debug.Log($"targetPixelsCount: {targetPixelsCount}");
            Debug.Log($"generalPixelsCount: {generalPixelsCount}");
            Debug.Log($"averageTarget: {averageTarget}");
            Debug.Log($"averageGeneral: {averageGeneral}");

        }

    }

        // TODO
        // alternatively, as we do have the target position, we can draw a rectangle around
        // it to grossly encompass the player's dimensions in ennemy's vision

        // the quality of the camouflage is determined by adding all pixels values from
        // this bounding rectangle and average it, then testing it against the averaged result
        // of the texture currently output by the target's OpticCamo Shader.

        // alternatively, we could also find the player sihouette and read this instead,
        // against all the rest of the bounding rect. this should be more accurate.
        // use the render layers?

        // as these operations are rather expensive, they should happen only when the player is
        // within the ennemy's field of view, not hidden by an object, and not in the Update()

        // to determine if the player is hidden by an object, the ennemy's camera position is
        // used as a source for regular random raycasts within the ennemy's vision, if it hits
        // the player, it will start to read its vision for a camo check.

        // this should be sufficient to emulate the accuracy of an enemy's vision and modulate
        // levels of difficulty.

        // todo list
        // //  fetch render textures for both the general view that renders everything
        // //  and the targeted view that only renders the player

        // //  on action, read both pixels from both textures
        //  create a buffer array for the player's texture as to store only initialized pixels (?)
        //  average both textures to a single big float
        //  compare both floats and decide of a camo threshold efficiency


}
