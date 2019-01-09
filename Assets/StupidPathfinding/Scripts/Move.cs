using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Move : MonoBehaviour
{
    public GameObject finishObj; //목표지점
  
    private int speed = 6;
    new Transform transform; //성능을위해..
    private CharacterController controller;
    private float rotateSpeed = 3.0F;
    private float gravity = 0f; //중력
    StupidPathfindingAI sPathAI;

  
    void Start () {
        transform = this.GetComponent<Transform>();
        controller = GetComponent<CharacterController>();
        sPathAI = new StupidPathfindingAI();

    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.R)) {
            Application.LoadLevel(Application.loadedLevel);
        } else {
            Vector3 nPos = sPathAI.GetNextPosition(finishObj.transform.position, transform);
            //testRayView();
           
            if (nPos != Vector3.zero)  {
                Vector3 moveDirection = new Vector3();
                moveDirection = nPos - transform.position;
                moveDirection *= speed;
                moveDirection.y -= gravity * Time.deltaTime * transform.up.y;
                Rotate(moveDirection);
                controller.Move(moveDirection * Time.deltaTime);
            }
          
        }
        
    }
    
    //캐릭터 회전
    void Rotate(Vector3 dir) {
        if (dir == Vector3.zero) return;
        Quaternion rot = transform.rotation;
        Quaternion toTarget = Quaternion.LookRotation(dir);
        rot = Quaternion.Slerp(rot, toTarget, rotateSpeed * Time.deltaTime);
        Vector3 euler = rot.eulerAngles;
        euler.z = 0;
        euler.x = 0;
        rot = Quaternion.Euler(euler);
        transform.rotation = rot;
    }

    void testRayView()
    {
        List<Vector3> testV = sPathAI.getTestRayPos();
        for (int i = 0; i < testV.Count; i++)
        {
            float len = transform.lossyScale.y + 3.5f;
            Vector3 point = new Vector3();
            point.Set(testV[i].x, -3f, testV[i].z);
            Debug.DrawRay(point, Vector3.up * len, Color.yellow);
        }//end of for
    }


}
