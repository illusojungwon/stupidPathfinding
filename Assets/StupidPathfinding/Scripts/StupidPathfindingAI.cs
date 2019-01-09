using UnityEngine;
using System.Collections.Generic;

public class StupidPathfindingAI : MonoBehaviour
{
    /*******************************************************************************************************************************
     *  본 알고리즘 및 소스는 저작권에 등록 되었습니다.
     *  무단배포/ 공유할 경우 저작권 침해에 대한 법적 책임이 발생할 수 있습니다.
     *  
     *  copy right @ 2017 by Woo jung won All Rights Reserved
     * ****************************************************************************************************************************/
    /* 
      [  ※ 4개의 요소로 목표지점까지 찾아가는 알고리즘 ]
        ※ 캐릭터 주위 8개의 "진행방향"
	        0	    1	    2
	        3   캐릭터	4
	        5	    6	    7

        ※ 짧은 거리 계산
	        - 진행방향 에서 목표지점 까지의 거리

        ※ 턴 방향
	        - 캐릭터가 이동중 장애물에 의해 방향을  틀때 필요
	            (장애물이 없을때는 9, 시계방향 일때는 1, 시계반대방향 일때는 0 으로 설정한다.)

        ※ 스택(Stack)
	        - 막힌 진행방향 과 턴 방향을 저장할 때 용도로 사용한다.

        1. 캐릭터 주위 8방향 위치에 장애물에 상관없이 짧은거리 계산해서 진행방향을 설정한다.

        2. 8방향 위치에 장애물에 막혔는지 설정한다. 막혔다면 Vector를 (0,0,0) 값으로 설정한다.

        3. 8방향이 모두 막혔는지 체크한다. 막혔다면 대기

        4. 캐릭터가 이동 중 장애물에 막혀 진행을 못할 경우
	        1) 스택(Stack)에 막힌 진행방향을 저장 한다.
	        2) 턴 방향을 설정하기 위해 8방향 중 짧은 거리의 방향을 가져온다.
		        - 막힌 진행방향 과 위에서 가져온 진행방향 으로 턴 방향을 설정한다.
		        - 스택(Stack)에 턴 방향을 저장 한다.

        5. 스택(Stack)에 막힌 진행방향이 있다면
	        1) 막힌 진행방향과 턴 방향으로 다음 이동 할 진행방향과 Vector를 가져온다.
			        ex) 막힌 진행방향이 4 이고 턴 방향이 1 이라고 가정할때, 4, 7, 6, 5, 3, 0, 1, 2 순으로 장애물에 걸리지 않은 하나를 가져온다.
			        ex) 막힌 진행방향이 4 이고 턴 방향이 0 이라고 가정할때, 4, 2, 1, 0, 3, 5, 6, 7 순으로 장애물에 걸리지 않은 하나를 가져온다.
		        - 막힌 진행방향과 가져온 진행방향이 같다면 스택(Stack)의 Top 값을 삭제한다.
	        2) 가져온 진행방향의 위치로 캐릭터를 이동한다.

        6. 진행방향이 안 막혔고, 스택(Stack)에 값이 없다면 짧은 거리 계산으로 만 이동한다.
	        ex) 진행방향이 0 이면 0, 1, 3 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 1 이면 0, 1, 2 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 2 이면 1, 2, 4 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 3 이면 0, 3, 5 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 4 이면 2, 4, 7 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 5 이면 3, 5, 6 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 6 이면 5, 6, 7 만 목표지점 까지 거리계산 한다.
	        ex) 진행방향이 7 이면 4, 6, 7 만 목표지점 까지 거리계산 한다.
    */

    private int progDirection;                       /* 진행방향 */
    private int trunDirection;			                /* 턴 방향 */
    private Stack<int> stackProgDirection;      /* 막힌 진행방향을 담는 스택 */
    private Stack<int> stackTrunDirection;		/* 턴 방향을 담는 스택 */
    private Vector3 nextPos;                        /*다음위치*/
    private Vector3 targetPos;                      /*도착위치*/
    private List<Vector3> eightPos;               /*8방향 Vector3 */
    private List<Vector3> testRayPos;            /*Ray 확인용 */
    
    public StupidPathfindingAI () {
        progDirection = 9;
        trunDirection = 9;
        stackProgDirection = new Stack<int>();
        stackTrunDirection = new Stack<int>();
        targetPos = Vector3.zero;
        nextPos = Vector3.zero;
        
        eightPos = new List<Vector3>();
    }
    
    public Vector3 GetNextPosition (Vector3 pTargetPos, Transform pCharacterTr) {
        targetPos = pTargetPos;
        if (nextPos == Vector3.zero) {
            nextPos = pCharacterTr.position;
        }

        float nextDist = Vector3.Distance(pCharacterTr.position, nextPos);
        
        if (nextDist < 0.3f) {
            GetEightDirectionVector(pCharacterTr);
            SetProgDirection();
            CloseBackProgDirection(progDirection);
            RayBasementToGround(pCharacterTr);
            SetCheckBlocked();
            if (stackProgDirection.Count > 0) {
                int peekStackProg = stackProgDirection.Peek(); // Peek 맨위에 객체를 삭제하지 않고 가져옴
                int peekStackTurn = stackTrunDirection.Peek();
                SetNextStackProgDirection(peekStackProg, peekStackTurn); 
            } else {
               SetNextProgDirection();
            }
        } 

        float finalDist = Vector3.Distance(pCharacterTr.position, pTargetPos);
        if (finalDist < 1f) {
            nextPos = Vector3.zero;
        }
     
        return nextPos;
    }

    /*캐릭터 주변 8방향 지점을 셋팅한다.*/
    private void GetEightDirectionVector (Transform pTransform) {
        //캐릭터 크기보다 1앞의 위치를 찾기위해
        float gab = 0.1f;
        
        Vector3 trHF = new Vector3(0, 0, ((pTransform.lossyScale.z / 2) + gab));
        Vector3 trHB = new Vector3(0, 0, ((pTransform.lossyScale.z / 2) + gab) * -1);
        Vector3 trHL = new Vector3(((pTransform.lossyScale.x / 2) + gab) * -1, 0, 0);
        Vector3 trHR = new Vector3(((pTransform.lossyScale.x / 2) + gab), 0, 0);

        Vector3 trF = new Vector3(0, 0, pTransform.lossyScale.z/2);
        Vector3 trB = new Vector3(0, 0, (pTransform.lossyScale.z/2) * -1);
        Vector3 trL = new Vector3((pTransform.lossyScale.x/2) * -1, 0, 0);
        Vector3 trR = new Vector3((pTransform.lossyScale.x/2), 0, 0);

        Vector3 unitF = pTransform.position + trHF + trF; // 앞) Z값 증가
        Vector3 unitB = pTransform.position + trHB + trB; // 뒤) Z값 감소
        Vector3 unitL = pTransform.position + trHL + trL; // 왼쪽) X값 감소
        Vector3 unitR = pTransform.position + trHR + trR; // 오른쪽) X값 증가

        Vector3 unitFL = unitF + trHL + trL; //앞의 왼쪽
        Vector3 unitFR = unitF + trHR + trR; //앞의 오른쪽
        Vector3 unitBL = unitB + trHL + trL; //뒤의 왼쪽  0
        Vector3 unitBR = unitB + trHR + trR; //뒤의 오른쪽  5
        
        eightPos.Clear();
        eightPos.Add(unitBL); //0  
        eightPos.Add(unitL); //1
        eightPos.Add(unitFL); //2
        eightPos.Add(unitB); //3
        eightPos.Add(unitF); //4
        eightPos.Add(unitBR); //5
        eightPos.Add(unitR); //6
        eightPos.Add(unitFR); //7
    }
    
    /*이동전 1번만실행 장애물상관없이 방향셋팅*/
    private void SetProgDirection () {
        if (progDirection == 9) {
            float dist = 99999f;
            for (int i = 0; i < eightPos.Count; i++) {
                float tempDist = Vector3.Distance(eightPos[i], targetPos);
                if (eightPos[i] != Vector3.zero) {
                    if (dist > tempDist) {
                        dist = tempDist;
                        progDirection = i;
                    }
                }
            }//end of for
        }
    }

    /*새로운 진행방향의 반대방향은 막는다.*/
    private void CloseBackProgDirection(int pNewProgDirection) {
        if (pNewProgDirection == 0) { eightPos[7] = Vector3.zero; }
        if (pNewProgDirection == 1) { eightPos[6] = Vector3.zero; }
        if (pNewProgDirection == 2) { eightPos[5] = Vector3.zero; }
        if (pNewProgDirection == 3) { eightPos[4] = Vector3.zero; }
        if (pNewProgDirection == 4) { eightPos[3] = Vector3.zero; }
        if (pNewProgDirection == 5) { eightPos[2] = Vector3.zero; }
        if (pNewProgDirection == 6) { eightPos[1] = Vector3.zero; }
        if (pNewProgDirection == 7) { eightPos[0] = Vector3.zero; }

    }

    /*장애물이 있는지 체크함*/
    private void RayBasementToGround (Transform pTransform) {
        //캐릭터 크기보다 1앞의 위치를 찾기위해
        float xMin = (pTransform.lossyScale.x/2) * -1;
        float xMax = (pTransform.lossyScale.x/2);
        float zMin = (pTransform.lossyScale.z/2) * -1;
        float zMax = (pTransform.lossyScale.z/2);

        List<Vector3> tempV = new List<Vector3>();
        Ray ray;
        RaycastHit hit2;
        int layerMask = 1;
        float len = pTransform.lossyScale.y + 3.5f;
        Vector3 point = new Vector3();

        testRayPos = new List<Vector3>(); //Ray 확인용

        for (int i = 0; i < eightPos.Count; i++) {
            if (eightPos[i] != Vector3.zero) { 
                // 중심축 Vector
                point.Set(eightPos[i].x, -3f, eightPos[i].z);
                ray = new Ray(point, Vector3.up);
                if (Physics.Raycast(ray.origin, ray.direction, out hit2, len, layerMask)) {
                    eightPos[i] = Vector3.zero;
                }
           
                    testRayPos.Add(eightPos[i]); //Ray 확인용
          
                // TOP 라인 Vector
                for (float n = zMin; n <= zMax; n = n + 0.1f) {
                    float x = eightPos[i].x + xMin;
                    float y = eightPos[i].y;
                    float z = eightPos[i].z + n;
                
                    point.Set(x, -3f, z);
                    ray = new Ray(point, Vector3.up);
                    if (Physics.Raycast(ray.origin, ray.direction, out hit2, len, layerMask)) {
                        eightPos[i] = Vector3.zero;
                    }
                    Vector3 v = new Vector3(x, y, z);

                    testRayPos.Add(v); //Ray 확인용
                }

                // LEFT 라인 Vector
                for (float n = xMin; n <= xMax; n = n + 0.1f) {
                    float x = eightPos[i].x + n;
                    float y = eightPos[i].y;
                    float z = eightPos[i].z + zMin;
                    point.Set(x, -3f, z);
                    ray = new Ray(point, Vector3.up);
                    if (Physics.Raycast(ray.origin, ray.direction, out hit2, len, layerMask)) {
                        eightPos[i] = Vector3.zero;
                    }

                    Vector3 v = new Vector3(x, y, z);
                    testRayPos.Add(v); //Ray 확인용
                }

                // RIGHT 라인 Vector
                for (float n = xMin; n <= xMax; n = n + 0.1f) {
                    float x = eightPos[i].x + n;
                    float y = eightPos[i].y;
                    float z = eightPos[i].z + zMax;
                    point.Set(x, -3f, z);
                    ray = new Ray(point, Vector3.up);
                    if (Physics.Raycast(ray.origin, ray.direction, out hit2, len, layerMask)) {
                        eightPos[i] = Vector3.zero;
                    }

                    Vector3 v = new Vector3(x, y, z);
                    testRayPos.Add(v); //Ray 확인용
                }

                // BOTTOM 라인 Vector
                for (float n = zMin; n <= zMax; n = n + 0.1f) {
                    float x = eightPos[i].x + xMax;
                    float y = eightPos[i].y;
                    float z = eightPos[i].z + n;
                    point.Set(x, -3f, z);
                    ray = new Ray(point, Vector3.up);
                    if (Physics.Raycast(ray.origin, ray.direction, out hit2, len, layerMask)) {
                        eightPos[i] = Vector3.zero;
                    }

                    Vector3 v = new Vector3(x, y, z);
                    testRayPos.Add(v); //Ray 확인용
                }
            }
        }// end of for

    }

    /*진행하던 방향이 막혔는지 체크, true 진행방향, 턴방향 스택에 담는다.*/
    private void SetCheckBlocked () {
        bool blocked = false;
        int newProgDirection = 9;
        float dist = 99999f;
        for (int i = 0; i < eightPos.Count; i++) {
            /*진행하던 방향이 막혔는지 체크*/
            if (progDirection == i && eightPos[i] == Vector3.zero) {     
                blocked = true;
            }

            /*턴 방향을 설정하기 위해 8방향 중 짧은 거리 계산 기준으로 진행방향을 가져온다.*/
            if (eightPos[i] != Vector3.zero) {
                float tempDist = Vector3.Distance(eightPos[i], targetPos);
                if (dist > tempDist) {
                    dist = tempDist;
                    newProgDirection = i;
                }
            }
        }//end of for
        
        /*진행하던 방향이 막혔다면 처리*/
        if (blocked) {
            stackProgDirection.Push(progDirection); //스택(Stack)에 막힌 진행방향을 담는다.
            SetTurnDirection(progDirection, newProgDirection); //스택(Stack)에 턴 방향을 담는다.
        }
    }
    
    /*막힌 진행방향 과 새로운 진행방향 으로 턴 방향을 설정 후 스택에 담는다.*/
    private void SetTurnDirection (int pProgDirection, int pNewProgDirection) {
        int tempTurnDirection = 9;
        if (pProgDirection == 0) {
            if (pNewProgDirection == 3 || pNewProgDirection == 5 || pNewProgDirection == 6 || pNewProgDirection == 7) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 1 || pNewProgDirection == 2 || pNewProgDirection == 4 || pNewProgDirection == 7) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 1) {
            if (pNewProgDirection == 0 || pNewProgDirection == 3 || pNewProgDirection == 5 || pNewProgDirection == 6) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 2 || pNewProgDirection == 4 || pNewProgDirection == 7 || pNewProgDirection == 6) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 2) {
            if (pNewProgDirection == 0 || pNewProgDirection == 1 || pNewProgDirection == 3 || pNewProgDirection == 5) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 4 || pNewProgDirection == 7 || pNewProgDirection == 6 || pNewProgDirection == 5) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 3) {
            if (pNewProgDirection == 5 || pNewProgDirection == 6 || pNewProgDirection == 7 || pNewProgDirection == 4) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 0 || pNewProgDirection == 1 || pNewProgDirection == 2 || pNewProgDirection == 4) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 4) {
            if (pNewProgDirection == 2 || pNewProgDirection == 1 || pNewProgDirection == 0 || pNewProgDirection == 3) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 7 || pNewProgDirection == 6 || pNewProgDirection == 5 || pNewProgDirection == 3) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 5) {
            if (pNewProgDirection == 6 || pNewProgDirection == 7 || pNewProgDirection == 4 || pNewProgDirection == 2) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 3 || pNewProgDirection == 0 || pNewProgDirection == 1 || pNewProgDirection == 2) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 6) {
            if (pNewProgDirection == 7 || pNewProgDirection == 4 || pNewProgDirection == 2 || pNewProgDirection == 1) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 5 || pNewProgDirection == 3 || pNewProgDirection == 0 || pNewProgDirection == 1) {
                tempTurnDirection = 1; //시계방향
            }
        }

        if (pProgDirection == 7) {
            if (pNewProgDirection == 4 || pNewProgDirection == 2 || pNewProgDirection == 1 || pNewProgDirection == 0) {
                tempTurnDirection = 0; //시계반대방향
            }

            if (pNewProgDirection == 6 || pNewProgDirection == 5 || pNewProgDirection == 3 || pNewProgDirection == 0) {
                tempTurnDirection = 1; //시계방향
            }
        }

        stackTrunDirection.Push(tempTurnDirection);
    }

    /*막힌 진행방향과 턴 방향으로 다음 이동 할 진행방향과 Vector를 가져온다.*/
    private void SetNextStackProgDirection (int pPeekStackProg, int pPeekStackTurn) {
        Vector3 tempNextPos = Vector3.zero;
        int tempNextProgDirection = 9;
        List<Vector3> tempPos = new List<Vector3>();
        if (pPeekStackProg == 0 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
        }

        if (pPeekStackProg == 0 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
        }

        if (pPeekStackProg == 1 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
        }

        if (pPeekStackProg == 1 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
        }

        if (pPeekStackProg == 2 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
        }

        if (pPeekStackProg == 2 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
        }

        if (pPeekStackProg == 3 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
        }

        if (pPeekStackProg == 3 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
        }

        if (pPeekStackProg == 4 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
        }

        if (pPeekStackProg == 4 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
        }

        if (pPeekStackProg == 5 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
        }

        if (pPeekStackProg == 5 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
        }

        if (pPeekStackProg == 6 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
        }

        if (pPeekStackProg == 6 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[7]);
        }

        if (pPeekStackProg == 7 && pPeekStackTurn == 0)
        { //시계반대방향으로 임시저장
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[4]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[6]);
        }

        if (pPeekStackProg == 7 && pPeekStackTurn == 1)
        { //시계방향으로 임시저장
            tempPos.Add(eightPos[7]);
            tempPos.Add(eightPos[6]);
            tempPos.Add(eightPos[5]);
            tempPos.Add(eightPos[3]);
            tempPos.Add(eightPos[0]);
            tempPos.Add(eightPos[1]);
            tempPos.Add(eightPos[2]);
            tempPos.Add(eightPos[4]);
        }

        for (int i = 0; i < tempPos.Count; i++) {
            if (tempPos[i] != Vector3.zero) {
                tempNextPos = tempPos[i];
                break;
            }
        }//end of for

        for (int i = 0; i < eightPos.Count; i++) {
            if (eightPos[i] == tempNextPos && pPeekStackProg == i) {
                int aa = stackProgDirection.Pop(); // // Pop 맨위에 객체를 삭제하고 가져옴
                int bb = stackTrunDirection.Pop();
            }

            if (eightPos[i] == tempNextPos) {
                tempNextProgDirection = i;
            }

        }//end of for

        nextPos = tempNextPos;
        progDirection = tempNextProgDirection;
    }

    /*진행방향이 안 막혔고, 스택(Stack)에 값이 없다면 짧은 거리 계산으로 만 이동한다. */
    private void SetNextProgDirection () {
        int newprogDirection = 9;
        float dist = 99999f;

        for (int i = 0; i < eightPos.Count; i++) {
            Vector3 tempV3 = new Vector3();
            if (i == 0) {
                if (progDirection == 2 || progDirection == 4 || progDirection == 5 || progDirection == 7 || progDirection == 6) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 1) {
                if (progDirection == 3 || progDirection == 4 || progDirection == 5 || progDirection == 7 || progDirection == 6) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 2) {
                if (progDirection == 0 || progDirection == 3 || progDirection == 5 || progDirection == 6 || progDirection == 7) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 3) {
                if (progDirection == 1 || progDirection == 2 || progDirection == 4 || progDirection == 7 || progDirection == 6) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 4) {
                if (progDirection == 0 || progDirection == 1 || progDirection == 3 || progDirection == 5 || progDirection == 6) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 5) {
                if (progDirection == 0 || progDirection == 1 || progDirection == 2 || progDirection == 4 || progDirection == 7) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 6) {
                if (progDirection == 0 || progDirection == 1 || progDirection == 2 || progDirection == 3 || progDirection == 4) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            if (i == 7) {
                if (progDirection == 0 || progDirection == 1 || progDirection == 3 || progDirection == 2 || progDirection == 5) {
                    tempV3 = Vector3.zero;
                } else {
                    tempV3 = eightPos[i];
                }
            }

            float tempDist = Vector3.Distance(tempV3, targetPos);
            if (tempV3 != Vector3.zero) {
                if (dist > tempDist) {
                    dist = tempDist;
                    newprogDirection = i;
                }
            }
        }//end of for
        progDirection = newprogDirection;
        nextPos = eightPos[progDirection];
    }

    /*Ray 확인용*/
    public List<Vector3> getTestRayPos()
    {
        return testRayPos;
    }





}
