using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Material))]
public class OpticCamo : MonoBehaviour
{
    [SerializeField] private List<Texture2D> alternateTextures;
    [SerializeField] private Transform[] rayTracers = new Transform[4];
    [SerializeField] private bool debug;

    private Material opticCamoMat;

    private RaycastHit[] raycastHits = new RaycastHit[4];
    private Color[] colors = new Color[4];
    private Color[] oldColors = new Color[4];
    private Material[] sourceMat = new Material[4];

    private float colorTransition;
    private float emissionTransition;
    private int currentTexture;

    #region events

    private void Start()
    {
        opticCamoMat = GetComponent<Renderer>().material;
        alternateTextures.Add((Texture2D)opticCamoMat.GetTexture("_CamoRamp"));
        currentTexture = 0;
        opticCamoMat.SetTexture("_CamoRamp", alternateTextures[currentTexture]);
    }

    private void Update()
    {
        if (debug) ShowRays();
    }

    #endregion

    #region public methods

    public void PickColors(bool verbose = false)
    {
        for (int i = 0; i < rayTracers.Length; i++)
        {
            if (Physics.Raycast(rayTracers[i].position,
                                rayTracers[i].TransformDirection(Vector3.forward),
                                out raycastHits[i]))
            {
                sourceMat[i] = raycastHits[i].collider.GetComponent<Renderer>().material;

                if(sourceMat[i].mainTexture != null)
                {
                    if (verbose) Debug.Log("Texture is found");

                    Vector2 localUVs = raycastHits[i].textureCoord;
                    if (verbose) Debug.Log("raw float: " + localUVs.x + " : " + localUVs.y);
                    Texture2D texture = (Texture2D)sourceMat[i].mainTexture;
                    localUVs.x *= texture.width;
                    localUVs.y *= texture.height;

                    if (verbose) Debug.Log("float: " + localUVs.x + " : " + localUVs.y);
                    if (verbose) Debug.Log("int: " + (int)localUVs.x + " : " + (int)localUVs.y);

                    oldColors[i] = opticCamoMat.GetColor($"_Color0{i+1}");
                    colors[i] = texture.GetPixel((int)localUVs.x, (int)localUVs.y);
                }
                else
                {
                    if (verbose) Debug.Log("no texture is found");
                    colors[i] = sourceMat[i].color;
                }
            }
        }
    }


    public void SendColors()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            opticCamoMat.SetColor($"_Color0{i+1}", colors[i]);
            opticCamoMat.SetColor($"_OldColor0{i+1}", oldColors[i]);
        }
    }

    public void RollColors()
    {
        Color swap = colors[3];
        colors[3] = colors[2];
        colors[2] = colors[1];
        colors[1] = colors[0];
        colors[0] = swap;
    }

    public void FadeColors(float fadeSpeed, bool verbose = false)
    {
        colorTransition = 0f;
        StartCoroutine(LerpColorsIn(fadeSpeed, verbose));
        colorTransition = 1f;
    }

    public void Flash(Color flashColor, float flashSpeed, bool verbose = false)
    {
        opticCamoMat.SetColor("_FlashColor", flashColor);
        //StartCoroutine(LerpEmissionIn());
        StartCoroutine(LerpEmission(flashSpeed, verbose));
        emissionTransition = 0;
    }

    public void UpdateCurrentTexture()
    {
        if(currentTexture < alternateTextures.Count -1) currentTexture++;
        else  currentTexture = 0;        
        opticCamoMat.SetTexture("_CamoRamp", alternateTextures[currentTexture]);
    }

    #endregion

    #region private methods

    private void ShowRays()
    {
        for (int i = 0; i < rayTracers.Length; i++)
        {
            Debug.DrawRay(  rayTracers[i].position,
                            rayTracers[i].TransformDirection(Vector3.forward) * 5f,
                            Color.red);            
        }
    }

    private IEnumerator LerpColorsIn(float speed, bool verbose = false)
    {
        for (float t = speed; t > 0; t -= Time.fixedDeltaTime )
        {
            colorTransition = 1.0f - t / speed;
            opticCamoMat.SetFloat("_ColorTransition", colorTransition);
            if (verbose) Debug.Log($"colorTransition: {colorTransition}");
            yield return new WaitForFixedUpdate();
        }
    }


    private IEnumerator LerpEmission(float speed, bool verbose = false)
    {
        yield return LerpEmissionIn(speed);

        for (float t = speed; t > 0; t -= Time.fixedDeltaTime )
        {
            emissionTransition = t / speed;
            opticCamoMat.SetFloat("_EmissionTransition", emissionTransition);

            if (verbose) Debug.Log($"emissionTransition: {emissionTransition}");

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator LerpEmissionIn(float speed, bool verbose = false)
    {
        for (float t = speed; t > 0; t -= Time.fixedDeltaTime )
        {
            emissionTransition = 1.0f - t / speed;
            opticCamoMat.SetFloat("_EmissionTransition", emissionTransition);

            if (verbose) Debug.Log($"emissionTransition: {emissionTransition}");

            yield return new WaitForFixedUpdate();
        }
    }

    #endregion

}
