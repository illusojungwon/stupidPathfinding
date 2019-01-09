using UnityEngine;
using System.Collections;

public class MainCameraAction : MonoBehaviour {

    
    //public float offsetX = 0.11f;
    //public float offsetY = 22f;
    //public float offsetZ = -6.02f;
  
    //public float h;
    //public float v;
    //public float y;
    //Vector3 cameraPosition;
    
    public string targetTagName;

    public static MainCameraAction instance = null;


    [SerializeField]
    private float distance = 4.0f;
    // the height we want the camera to be above the target
    [SerializeField]
    private float height = 2.0f;

    [SerializeField]
    private float rotationDamping;
    [SerializeField]
    private float heightDamping;

    void Awake() {
        

        targetTagName = "UNIT";
        instance = this;
    }

    void Update() {
        //h = h+Input.GetAxis("Horizontal");
        //v = v+Input.GetAxis("Vertical");

        //y = y+Input.GetAxis("Mouse ScrollWheel");
        
    }

    //게임상의 모든 Update 로직을 마친 후, 실행하는 마지막 Update사이클
    void LateUpdate()
    {
        
        if(targetTagName != null) {
            
            GameObject target = GameObject.FindGameObjectWithTag(targetTagName);
            
            
            var wantedRotationAngle = target.transform.eulerAngles.y;
            var wantedHeight = target.transform.position.y + height;

            var currentRotationAngle = transform.eulerAngles.y;
            var currentHeight = transform.position.y;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

            // Convert the angle into a rotation
            var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = target.transform.position;
            transform.position -= currentRotation * Vector3.forward * distance;

            // Set the height of the camera
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // Always look at the target
            transform.LookAt(target.transform);
        } else {
            //cameraPosition.Set(offsetX + h, offsetY + y, offsetZ + v);

            //transform.position = cameraPosition;
        }
        
    }

    public void MyPostRender(Camera cam) {
        Debug.Log("PostRender " + gameObject.name + " from camera " + cam.gameObject.name);
    }
}
