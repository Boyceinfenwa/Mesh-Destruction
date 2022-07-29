using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    // Start is called before the first frame update

    Vector3 movepos;
    float speed = 5f;
    float ypos = 0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotate();
    }

    void Rotate()
    {
        if(Input.GetKey(KeyCode.C))
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            transform.Rotate(Vector3.forward, speed +10  * Time.deltaTime);
        }
        
    }

    private void Move()
    {
        bool up = false;
        bool down = false;

        if (Input.GetKey(KeyCode.Q) && !up)
        {
            ypos = ypos + 0.1f;
            up = true;
        }
        else
            up = false;

        if (Input.GetKey(KeyCode.E)&& !down)
        {
            ypos = ypos - 0.1f;
            down = true;
        }
        else
            down = false;

        movepos = new Vector3(Input.GetAxis("Horizontal"), ypos, Input.GetAxis("Vertical"));
        transform.position += movepos * Time.deltaTime * speed;
    }
}
