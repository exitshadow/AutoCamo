using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyVision : MonoBehaviour
{
    [SerializeField] private Camera generalView;
    [SerializeField] private Camera targetedView;
    [SerializeField] private GameObject target;

    [SerializeField] private bool debug;

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
            if(debug) Debug.Log("Target position is in ennemy's view");
        }
    }

    private void CalculateTargetBounds( int bound,
                                        int boundmargin,
                                        out Vector2 origin,
                                        out Vector2 destination)
    {
        float minX = viewTargetPos.x - bound - boundmargin;
        float minY = viewTargetPos.y - bound - boundmargin;
        if (minX < 0) minX = 0;
        if (minY < 0) minY = 0;
        origin = new Vector2(minX, minY);

        float maxX = viewTargetPos.x + bound + boundmargin;
        float maxY = viewTargetPos.y + bound + boundmargin;
        if (maxX > 1) maxX = 1;
        if (maxY > 1) maxY = 1;
        destination = new Vector2(maxX, maxY);
    }

    private void ReadVision(Vector2 from, Vector2 to)
    {
        int boundBoxWidth = (int)(to.x - from.x) * generalRender.width;
        int boundBoxHeight = (int)(to.y - from.y) * generalRender.height;

        int originX = (int)(from.x * generalRender.width);
        int originY = (int)(from.y * generalRender.height);

        int destX = (int)(to.x * generalRender.width);
        int destY = (int)(to.y * generalRender.height);

        generalRenderTex = new Texture2D(   boundBoxWidth,
                                            boundBoxHeight,
                                            TextureFormat.RGB24,
                                            true);
        
        targetRenderTex = new Texture2D(    boundBoxWidth,
                                            boundBoxHeight,
                                            TextureFormat.RGB24,
                                            true);
        
        Rect cropSource = new Rect (originX, originY, destX, destY);

        Graphics.SetRenderTarget(generalRender);
        generalRenderTex.ReadPixels(cropSource, 0, 0);

        Graphics.SetRenderTarget(targetRender);
        targetRenderTex.ReadPixels(cropSource, 0, 0);
        
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
