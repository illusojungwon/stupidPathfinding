using UnityEngine;
using System.Collections;

public class ObjOnMouseDown : MonoBehaviour {
    public GameObject wall01;
    public GameObject wall02;
    public GameObject wall03;
    public GameObject wall04;
    public GameObject wall05;
    public GameObject character;
    public GameObject finish;
    int cnt = 0;
    // Use this for initialization
    void Start () {
    
    }
	
	// Update is called once per frame
	void Update () {
	
	}

  
     void OnMouseDown() {
       
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if(Physics.Raycast(ray, out hit)) {
            float dist = Vector3.Distance(character.transform.position, hit.point);
            float dist2 = Vector3.Distance(finish.transform.position, hit.point);
            if(cnt < 10)
            {
                if (dist > 4f && dist2 > 3f)
                {
                    int b = Random.Range(1, 3);
                    Vector3 p = new Vector3(hit.point.x, 1, hit.point.z);
                    if (b == 1) {
                        Instantiate(wall01, p, new Quaternion());
                        cnt++;
                    } else {
                        Instantiate(wall02, p, new Quaternion());
                        cnt++;
                    } 
                }
            }
        }
    }
}
