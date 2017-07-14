using UnityEngine;
using System.Collections;

public class ChaseCamera : MonoBehaviour {

    public float speed = 20f;	
	// Update is called once per frame
	void Update () {		
        this.transform.position = new Vector3(transform.position.x + (speed * Time.smoothDeltaTime), transform.position.y, transform.position.z);
	}
}
