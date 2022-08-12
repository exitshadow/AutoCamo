using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyVision : MonoBehaviour
{
    [SerializeField] private Camera generalView;
    [SerializeField] private Camera targetedView;
    [SerializeField] private GameObject target;

    private RenderTexture generalRender;
    private Texture2D generalRenderTex;
    private RenderTexture targetRender;
    private Texture2D targetRenderTex;
    private Vector3 viewTargetPos;

    private void Start()
    {
        generalRender = generalView.targetTexture;
    }

    private void Update()
    {
        viewTargetPos = generalView.WorldToViewportPoint(target.transform.position);
        if(     viewTargetPos.x > 0 && viewTargetPos.x < 1
            &&  viewTargetPos.y > 0 && viewTargetPos.y < 1  )
        {
            Debug.Log("Target position is in ennemy's view");
        }
    }

    private void ReadVision()
    {
        generalRenderTex = new Texture2D(  generalRender.width,
                                            generalRender.height,
                                            TextureFormat.RGB24,
                                            true);

        generalRenderTex.ReadPixels(new Rect (0,0, generalRender.width, generalRender.height), 0, 0);

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
        //  fetch render textures for both the general view that renders everything
        //  and the targeted view that only renders the player

        //  on action, read both pixels from both textures
        //  create a buffer array for the player's texture as to store only initialized pixels (?)
        //  average both textures to a single big float
        //  compare both floats and decide of a camo threshold efficiency

    }

}
