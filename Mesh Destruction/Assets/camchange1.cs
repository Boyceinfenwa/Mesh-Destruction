using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camchange1 : MonoBehaviour
{

    public Camera main,SWcam, doorB, doorA;
    public GameObject player, button, door;
    Animator panim, danim;
    Vector3 buttonpos;
    bool finished, open = false;
    public bool platformsactive;
    public GameObject[] platforms;
   public bool regular;
    bool pressed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        doorA.gameObject.SetActive(false);
        SWcam.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        main = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider playercollider = player.GetComponent<Collider>();
        panim = player.GetComponent<Animator>();

        danim = door.GetComponent<Animator>(); 

        if (other == playercollider)
        {
            Debug.Log(finished);
            if(platformsactive)
            {
                if(!finished)
                StartCoroutine(PlatformActivate());
            }

            if(!platformsactive)
            {
                open = true;
                if (open)
                {
                    if (!finished)
                    {
                        StartCoroutine(buttonpress());
                    }


                }
            }
                

        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    IEnumerator buttonpress()
    {
        
       if (regular&&pressed == false)
        {
            buttonpos = button.transform.position;
            buttonpos.y = buttonpos.y -  0.2f;
            button.transform.position = buttonpos;
             //yield return new WaitForSeconds(1);
           // GameObject.FindGameObjectWithTag("Level").GetComponent<Level>().switches = GameObject.FindGameObjectWithTag("Level").GetComponent<Level>().switches - 1;
            pressed = true;
        }
       
        //SWcam.gameObject.SetActive(false);
        if (!regular)
        {
            buttonpos = button.transform.position;
            buttonpos.y = buttonpos.y - 0.2f;
            button.transform.position = buttonpos;
            yield return new WaitForSeconds(1);
            StartCoroutine(dooropen());
        }
    }

    IEnumerator dooropen()
    {
        main.gameObject.SetActive(false);
        doorA.gameObject.SetActive(true);
        danim.SetBool("is open", true);
        finished = true;

        yield return new WaitForSeconds(5);
        main.gameObject.SetActive(true);
        doorA.gameObject.SetActive(false);
    }
    IEnumerator PlatformActivate()
    {
        buttonpos = button.transform.position;
        buttonpos.y = buttonpos.y - 0.2f;
        button.transform.position = buttonpos;
        main.gameObject.SetActive(false);
        doorB.gameObject.SetActive(true);
        

        yield return new WaitForSeconds(5);
        main.gameObject.SetActive(true);
        doorB.gameObject.SetActive(false);
        finished = true;
    }
}
