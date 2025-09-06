using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlacingEffect : MonoBehaviour
{
    public float getYPos = 13.5f;
    public float startYPos = 25.5f;
    private bool startToMove = false;
    private float descendValue = 0;
    private float deductYValue = 0.2f;
    
    [Space(10)]
    [Header("沙子材质")]
    public Material sandMaterial;
    [Header("房子材质")]
    public Material houseMaterial;
    
    [Header("落地粒子效果")]
    public ParticleSystem boomPs;
    // Start is called before the first frame update
    
    private MeshRenderer meshRenderer;
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void StartToPlace()
    {
        this.transform.position = new Vector3(this.transform.position.x, startYPos, this.transform.position.z);
        startToMove = true;
        
        meshRenderer.material = sandMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
           // this.transform.DOMoveY(getYPos, 0.3f);
           
        }

        if (startToMove)
        {
            this.transform.position = this.transform.position - new Vector3(0, deductYValue , 0);
            this.transform.position = this.transform.position + new Vector3(0, 0.001f, 0);
            //deductYValue += 0.005f;
            descendValue += 0.2f;
            if (descendValue >= 12f)
            {
                startToMove = false; StartCoroutine(StartToShake());
                meshRenderer.material = houseMaterial;
                boomPs.Play();
                //
                GlobalAudioSystem.Instance.PlayHouseSetClick();
            } 
        }
    } 

    IEnumerator StartToShake()
    {
        yield return new WaitForSeconds(0.25f);
        this.transform.DOShakePosition(0.5f, 0.3f);
     //   this.transform.DOShakeScale()
    }
}
