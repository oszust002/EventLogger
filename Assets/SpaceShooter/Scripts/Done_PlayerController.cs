using UnityEngine;
using System.Collections;

[System.Serializable]
public class Done_Boundary
{
    public float xMin, xMax, zMin, zMax;
}

public class Done_PlayerController : MonoBehaviour
{
    public float speed;
    public float tilt;
    public Done_Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;

    private float nextFire;

    private void Awake()
    {
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.LeftArrow).WithModifiers(EventModifiers.Control),
            "Player", "MoveLeft");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.LeftArrow),
            "Player", "MoveLeft");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.RightArrow).WithModifiers(EventModifiers.Control),
            "Player", "MoveRight");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.RightArrow),
            "Player", "MoveRight");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.UpArrow).WithModifiers(EventModifiers.Control),
            "Player", "MoveUp");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.UpArrow),
            "Player", "MoveUp");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.DownArrow).WithModifiers(EventModifiers.Control),
            "Player", "MoveDown");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.DownArrow),
            "Player", "MoveDown");
        EventManager.AddKeyAlias(KeyRepresentation.Create(KeyCode.LeftControl),
            "Player", "Shoot");
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            EventManager.LogEvent(Time.time, tag, "PlayerControl", "Shoot", "PositionX", shotSpawn.position.x);
            EventManager.LogEvent(Time.time, tag, "PlayerControl", "Shoot", "PositionY", shotSpawn.position.z);
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        GetComponent<Rigidbody>().velocity = movement * speed;

        GetComponent<Rigidbody>().position = new Vector3
        (
            Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
        );

        GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
    }
}