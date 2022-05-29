using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TankController : MonoBehaviour{
    
    
    public Transform[] rightSensors, leftSensors;
    public Transform[] rightIKs, leftIKs;
    
    public bool[] isAnimating = new bool[6];
    public float[] lerps = new float[6];

    public float sensorRange = 5f, movementSpeed = 1f, heightOffset = 2f, heightCorrectSpeed = 1f, legStepHeight, legSpeed;
    public float legTargetDistanceMax = 3f, rotationSensitivity = 1f, legContactDistance = 0.1f;


    private void OnDrawGizmos(){
        foreach(Transform t in leftSensors){
            Debug.DrawRay(t.position, t.forward * sensorRange, Color.magenta);
        }
        foreach(Transform t in rightSensors){
            Debug.DrawRay(t.position, t.forward * sensorRange, Color.magenta);
        }
    }

    private void Start(){
        SetInitPositionLeft();
        SetInitPositionRight();
    }

    private void Update(){
        GetTargetPositionLeft();
        GetTargetPositionRight();
        PlayerControl();
        CorrectBodyHeight();
        ControlBodyHeightOffset();
    }

    public void GetTargetPositionLeft(){
        for(int i = 0; i < 3; i++){
            if(Physics.Raycast(leftSensors[i].position, leftSensors[i].forward, out var hit, sensorRange, LayerMask.GetMask("Ground"))){
                
                if(Vector3.Distance(leftIKs[i].position, hit.point) > legTargetDistanceMax){
                    if(!isAnimating[i]) lerps[i] = 0f;
                    isAnimating[i] = true;
                    
                    //also set the corresponding right leg to animate
                    if(!isAnimating[i + 3]) lerps[i + 3] = 0f;
                    isAnimating[i + 3] = true;

                }else if(Vector3.Distance(leftIKs[i].position, hit.point) < legContactDistance){
                    isAnimating[i] = false;
                }

                if(isAnimating[i]){
                    leftIKs[i].position = Vector3.Lerp(leftIKs[i].position, hit.point, lerps[i]);
                    leftIKs[i].position += Vector3.up * Mathf.Sin(lerps[i] * Mathf.PI) * legStepHeight;
                    lerps[i] += Time.deltaTime * legSpeed;
                }
            }
        }
    }

    public void GetTargetPositionRight(){
        for(int i = 0; i < 3; i++){
            if(Physics.Raycast(rightSensors[i].position, rightSensors[i].forward, out var hit, sensorRange, LayerMask.GetMask("Ground"))){
                
                if(Vector3.Distance(rightIKs[i].position, hit.point) > legTargetDistanceMax){
                    if(!isAnimating[i + 3]) lerps[i + 3] = 0f;
                    isAnimating[i + 3] = true;

                    if(!isAnimating[i]) lerps[i] = 0f;
                    isAnimating[i] = true;

                }else if(Vector3.Distance(rightIKs[i].position, hit.point) < legContactDistance){
                    isAnimating[i + 3] = false;
                }

                if(isAnimating[i + 3]){
                    rightIKs[i].position = Vector3.Lerp(rightIKs[i].position, hit.point, lerps[i + 3]);
                    rightIKs[i].position += Vector3.up * Mathf.Sin(lerps[i + 3] * Mathf.PI) * legStepHeight;
                    lerps[i + 3] += Time.deltaTime * legSpeed;
                }
            }
        }
    }

    //set the left legs in their init position using the sensors
    public void SetInitPositionLeft(){
        for(int i = 0; i < 3; i++){
            if(Physics.Raycast(leftSensors[i].position, leftSensors[i].forward, out var hit, sensorRange, LayerMask.GetMask("Ground"))){
                leftIKs[i].position = hit.point;
            }
        }
    }

    public void SetInitPositionRight(){
        for(int i = 0; i < 3; i++){
            if(Physics.Raycast(rightSensors[i].position, rightSensors[i].forward, out var hit, sensorRange, LayerMask.GetMask("Ground"))){
                rightIKs[i].position = hit.point;
            }
        }
    }

    //the front is actually transform.right, shoulda have checked before placing the legs!
    public void PlayerControl(){
        //movement
        Vector3 move = transform.right * Input.GetAxis("Vertical") + transform.forward * -Input.GetAxis("Horizontal");
        transform.Translate(move * movementSpeed * Time.deltaTime, Space.World);
        //rotation
        
        float rotate = 0f;
        if(Input.GetKey("q")){
            rotate = -1f;
        }else if(Input.GetKey("e")){
            rotate = 1f;
        }

        transform.localRotation *= Quaternion.AngleAxis(rotate * rotationSensitivity, Vector3.up);
    }

    public void CorrectBodyHeight(){
        
        //float groundLevel = 0f;
        //if(Physics.Raycast(transform.position, Vector3.down, out var hit, 3f, LayerMask.GetMask("Ground"))){
            //groundLevel = hit.point.y;
        //}

        
        float targetHeight = 0f;
        for(int i = 0; i < 3; i++){
            targetHeight += leftIKs[i].position.y;
            targetHeight += rightIKs[i].position.y;
        }
        
        targetHeight /= 6f;
        targetHeight += heightOffset;

        
        transform.Translate(Vector3.up * (targetHeight-transform.position.y) * heightCorrectSpeed * Time.deltaTime);
    }

    //uses left shift and ctrl to change the height offset
    public void ControlBodyHeightOffset(){
        if(Input.GetKeyDown("left shift")){
            heightOffset += 0.25f;
        }else if(Input.GetKeyDown("left ctrl")){
            heightOffset -= 0.25f;
        }

        heightOffset = Mathf.Clamp(heightOffset, 1f, 2.25f);
    }

    private Vector3 GetXZ(Vector3 from){
        return new Vector3(from.x, 0f, from.z);
    }
}