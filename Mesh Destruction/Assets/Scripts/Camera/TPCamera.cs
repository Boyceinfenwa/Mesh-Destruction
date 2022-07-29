using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPCamera : MonoBehaviour
{

    public Transform target;
    public Vector3 offset;
    public float rotSpeed;
    public Transform pivot;
    public GameObject camObj;
    public float clamoAngle;
    public float iptsens;
    public float camdistanceXplayer;
    public float camdistanceYplayer;
    public float camdistanceZplayer;
    public float smoothX;
    public float smoothY;
    public float finalXinput;
    public float finalZinput;
    public float nsew = 0;
    int times = 1;
    float rotX;
    float rotY;


    // Start is called before the first frame update
    void Start()
    {
        //offset = target.position - transform.position;

        // pivot.transform.position = target.transform.position;
        // pivot.parent = null;
        //pivot.transform.parent = target.transform; 

        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    void Update()
    {
        float InputX = Input.GetAxis("CameraX");
        float InputZ = Input.GetAxis("CameraY");
        finalXinput = InputX;
        finalZinput = InputZ;

        rotY += finalXinput * iptsens * Time.deltaTime;
        rotX += finalZinput * iptsens * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clamoAngle, clamoAngle);
        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);

        transform.rotation = localRotation;


        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown("joystick button 6"))
        {
            switch (times)
            {
                default:
                    break;
                case 1:
                    nsew = 0;
                    rotY = nsew;
                    times++;
                    break;
                case 2:
                    nsew = 90;
                    rotY = nsew;
                    times++;
                    break;
                case 3:
                    nsew = 180;
                    rotY = nsew;
                    times++;
                    break;
                case 4:
                    nsew = 270;
                    rotY = nsew;
                    times = 1;
                    break;



            }
        }

        ///if (Input.GetKeyDown(KeyCode.Z)|| Input.GetKeyDown(""))
        ///  {

        //  }
    }

    // Update is called once per frame
    void LateUpdate()
    {

        CameraUpdater();
        // pivot.transform.position = target.transform.position; 
        // x rot
        //float horizontal = Input.GetAxis("Mouse X") * rotSpeed;
        //pivot.Rotate(0, horizontal, 0);
        // get y and rot pivot 
        // float vertical = Input.GetAxis("Mouse Y") * rotSpeed;
        // pivot.Rotate(-vertical, 0, 0);

        // limit camera rotation
        /* if(pivot.rotation.eulerAngles.x > 45f  && pivot.rotation.x < 180f)
         {
             pivot.rotation = Quaternion.Euler(45f, 0, 0);
         }*/


        // move cam based on player rot 
        // float desiredYangle = pivot.eulerAngles.y;
        // float desiredXangle = pivot.eulerAngles.x;

        // Quaternion rotation = Quaternion.Euler(desiredXangle, desiredYangle, 0);
        // transform.position = target.position - (rotation*offset);
        //transform.position = target.position - offset;
        // if(transform.position.y < target.position.y)
        //  {
        //  transform.position = new Vector3(transform.position.x, target.position.y -0.5f, transform.position.z);
        //}
        // transform.LookAt(target);


    }

    void CameraUpdater()
    {
        Transform target = camObj.transform;

        float step = rotSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

    }
}
