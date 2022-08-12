using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyVision : MonoBehaviour
{
    [SerializeField] private Camera view;
    [SerializeField] private GameObject target;

    private RenderTexture viewRender;
    private Texture2D currentViewRender;
    private Vector3 viewTargetPos;

    private void Start()
    {
        viewRender = view.targetTexture;
    }

    private void Update()
    {
        viewTargetPos = view.WorldToViewportPoint(target.transform.position);
        if(     viewTargetPos.x > 0 && viewTargetPos.x < 1
            &&  viewTargetPos.y > 0 && viewTargetPos.y < 1  )
        {
            Debug.Log("Target position is in ennemy's view");
        }
    }

    private void ReadVision()
    {
        currentViewRender = new Texture2D(  viewRender.width,
                                            viewRender.height,
                                            TextureFormat.RGB24,
                                            true);

        currentViewRender.ReadPixels(new Rect (0,0, viewRender.width, viewRender.height), 0, 0);

        // TODO
        // alternatively, as we do have the target position, we can draw a rectangle around
        // its position to grossly encompass the player's dimensions in ennemy's vision

        // the quality of the camouflage is determined by adding all pixels values from
        // this bounding rectangle and average it, then testing it against the averaged result
        // of the texture currently output by the target's OpticCamo Shader.

        // as these operations are rather expensive, they should happen only when the player is
        // within the ennemy's field of view, not hidden by an object, and not in the Update()

        // to determine if the player is hidden by an object, the ennemy's camera position is
        // used as a source for regular random raycasts within the ennemy's vision, if it hits
        // the player, it will start to read its vision for a camo check.

        // this should be sufficient to emulate the accuracy of an enemy's vision and modulate
        // levels of difficulty.

    }

}
