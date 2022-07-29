using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Charactercontroller : MonoBehaviour
{

    Vector3 movDir;
    Animator anim;
    public GameObject player;
    public float speed = 8;
    public float gravity;
    bool jumping = false;
    public bool canmove = true;
    bool dj = false;
    int jumps;
    CharacterController controller;
    public Transform pivot;
    float rotSpeed=80f;
    public float jump = 10f;
    
   
   
   
     public int health;
    public int maxhealth = 10;
   

    bool death = false;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        
        movDir.y = 0;
        transform.position = GameObject.FindGameObjectWithTag("Start").transform.position;
        Debug.Log("we loaded");
        

       


    }
    
    
    void Update()
    {
       
            DontDestroyOnLoad(gameObject);
    

        if (canmove)
        {
            Movement();
            Attack();
        }
      
        if (health == 0)
        {

            StartCoroutine(Death());
            
           
        }

        if (health > 0)
        {
            anim.SetBool("death", false);
        }


    }

    void Movement()
    {

        

        float x = Input.GetAxis("Vertical");
        float z = Input.GetAxis("Horizontal");
        float yStore = movDir.y;

        movDir = (transform.forward * Input.GetAxis("Vertical") * speed) + (transform.right * Input.GetAxis("Horizontal") * speed);
        movDir = movDir.normalized * speed;
        movDir.y = yStore;


        if (controller.isGrounded)
        {
            anim.SetBool("falling", false);
            movDir.y = 0;
            jumps = 0;
            anim.SetBool("idle", false);

            anim.SetFloat("speed", (Mathf.Abs(x) + Mathf.Abs(z)));

            if (Input.GetKey(KeyCode.Space))
            {
                anim.SetBool("jumping", true);
                movDir.y = jump;
               
                Debug.Log(jumps + "jumps");
            }

            if (movDir.y < 1 && anim.GetBool("jumping") == true)
            {
                anim.SetBool("jumping", false);
            }         
            
            if (Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                movDir = (transform.right * Input.GetAxis("Horizontal")* speed);
                anim.SetBool("strafeL", true);
                

            }

            if (Input.GetKeyUp(KeyCode.JoystickButton4))
            
            {
                anim.SetBool("strafeL", false);
            }

            if (Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                movDir = (transform.right * Input.GetAxis("Horizontal") * speed);
                anim.SetBool("strafeR", true);
               

            }

            if (Input.GetKeyUp(KeyCode.JoystickButton5))

            {
                anim.SetBool("strafeR", false);
            }

        }

        else
        {
            anim.SetBool("falling", true);

            if (dj)
            {
                if (Input.GetKeyDown(KeyCode.Joystick1Button0) && jumps < 1)
                {
                    movDir.y = jump;
                    jumps++;
                    Debug.Log(jumps + "jumps");
                }
            }
        }

        movDir.y = movDir.y + (Physics.gravity.y * Time.deltaTime);
        controller.Move(movDir * Time.deltaTime);
        if(anim.GetBool("strafeL") == false && anim.GetBool("strafeR") == false)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
                Quaternion newRot = Quaternion.LookRotation(new Vector3(movDir.x, 0f, movDir.z));
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, newRot, rotSpeed * Time.deltaTime);
            }
        }

        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    void Attack()
    {
       
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Joystick1Button2))
        {

            anim.SetBool("attack", true);
            Debug.Log("reeee");
        }

        if (Input.GetKeyUp(KeyCode.Z) && anim.GetBool("attack") == true || Input.GetKeyUp(KeyCode.Joystick1Button2) && anim.GetBool("attack") == true)
        {
            anim.SetBool("attack", false);

        }

        if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Joystick1Button0))
        {

            anim.SetBool("attack2", true);
            Debug.Log("reeee");
        }

        if (Input.GetKeyUp(KeyCode.X) && anim.GetBool("attack2") == true || Input.GetKeyUp(KeyCode.Joystick1Button0) && anim.GetBool("attack2") == true)
        {
            anim.SetBool("attack2", false);

        }
    }

    IEnumerator Death()
    {
        death = true;
        anim.SetBool("death", true);

        health = 10;
        yield return new WaitForSeconds(0.1f);

        transform.position = GameObject.FindGameObjectWithTag("Start").transform.position;

        yield return new WaitForSeconds(1.2f);
        death = false;

    }

   
}
