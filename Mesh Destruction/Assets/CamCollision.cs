using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCollision : MonoBehaviour
{
    public float mindist;
    public float maxdist;
    public float smooth = 10.0f;
    Vector3 dollyDir;
    public Vector3 dollyDirAjst;
    public float distacnce;

    // Start is called before the first frame update
    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distacnce = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredCampos = transform.parent.TransformPoint(dollyDir * maxdist);
        RaycastHit hit;

        if (Physics.Linecast(transform.parent.position, desiredCampos, out hit))
        {
            distacnce = Mathf.Clamp((hit.distance * 0.1f), mindist, maxdist);

        }
        else
        {
            distacnce = maxdist;

        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distacnce, Time.deltaTime * smooth);

        if (Input.GetKey("joystick button 4"))
        {
            Debug.Log("guess this works");

        }
    }
}
