using UnityEngine;
using System.Collections;

public class Done_WeaponController : MonoBehaviour
{
	public GameObject shot;
	public Transform shotSpawn;
	public float fireRate;
	public float delay;

	void Start ()
	{
		InvokeRepeating ("Fire", delay, fireRate);
	}

	void Fire ()
	{
		EventManager.LogEvent(Time.time, tag, "PlayerControl", "Shoot", "PositionX", shotSpawn.position.x);
		EventManager.LogEvent(Time.time, tag, "PlayerControl", "Shoot", "PositionY", shotSpawn.position.y);
		Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
		GetComponent<AudioSource>().Play();
	}
}
