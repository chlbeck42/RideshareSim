using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CarController : MonoBehaviour
{
    //woah for once i used comments. what kind of place is this. 

    public float topSpeed = 80f; // (Unused) Top speed to cap acceleration. May be done by drag eventually. 80 KPH / 60ish MPH
    public float acceleration = 20f; // Vehicle acceleration. 
    public float braking = 48f;// Vehicle braking acceleration
    public float curviness = 1f; // (unused) Curves acceleration based on speed. Negative = less top end, positive = less low end. May need acceleration tweaks.
    public Transform carTf; //Transform of the vehicle
    public Transform tempTf;//Transform used for temp stuff
    public float drag = 1f; //(unused) Amount of "drag". Increases with square of speed.
    public float currentSpeed = 0f;//Current vehicle speed scalar
    private Vector3 rotationEuler = Vector3.zero; //Vector3 representing the desired rotation for tf.Rotate
    private Vector3 currentVector = Vector3.zero; //Current vehicle velocity vector
    private float smoothedRot = 0; //Rotation variable. Smooths out turning and essentially simulates the position of the steering wheel. 
    public bool reverse = false; //Vehicle reverse state
    private int counter = 0; //Generic counter variable to prevent actions from happening multiple times on one keypress. 
    
    // Update is called once per frame... or is it?
    void FixedUpdate()
    {
        //Counter:
        if (counter < 50){
            counter++;
        }
        //Switch Gears:
        if (Input.GetKey("left shift") && counter == 50)
        {
            reverse = !reverse;
            counter = 0;
            Debug.Log(reverse);
        }
        
        //Find current direction of vehicle and convert currentSpeed to currentVector
        currentVector = new Vector3(Mathf.Sin(carTf.eulerAngles.y * Mathf.Deg2Rad) * currentSpeed, 0, Mathf.Cos(carTf.eulerAngles.y * Mathf.Deg2Rad) * currentSpeed);
        //Apply current velocity to carTf
        carTf.position += currentVector;
        
        //Calculates drag based on basic fancy math.
        drag = MathF.Pow(2f,currentSpeed+1) / 64f;
        //Apply drag to vehicle
        if (currentSpeed > drag / 50) {
            currentSpeed -= drag / 50;
        }else if (currentSpeed < drag / -50)
        {
            currentSpeed += drag / 50;
        }else
        {
            currentSpeed = 0f;
        }
        //Check if throttle key pressed, if so, increase currentSpeed by acceleration/50 * deltatime  (fixed timestep)
        if (Input.GetKey("w"))
        {
            if (!reverse)
            {
                currentSpeed += (acceleration / (50)) * Time.deltaTime;
            }
            else
            {
                currentSpeed -= (acceleration / (50)) * Time.deltaTime;
            }
        }
        //Check if brake key is pressed, if so,:
        //check if moving,  if so, decrease currentspeed by braking/50*deltatime, otherwise, keep currentSpeed 0
        if (Input.GetKey("s"))
        {
            if (currentSpeed > 0.005)
            {
                currentSpeed -= (braking / (50)) * Time.deltaTime;
            }
            else if(currentSpeed < -.005)
            {
                    currentSpeed += (braking / (50)) * Time.deltaTime;
            }
            else
            {
                currentSpeed = 0;
            }
        }
        
        //Reset rotation euler to prevent uncapped rotation
        rotationEuler = Vector3.zero;
        //10 * Mathf.Clamp((currentSpeed * 50), 0f, 4f)
        if (Input.GetKey("a") && smoothedRot > -90) //Turn Left
        {
            smoothedRot -= 1.8f;
            //Double change rate if currently turning opposite way
            if(smoothedRot > 0)
            {
                smoothedRot -= 1.8f;
            }
        }
        else if (Input.GetKey("d") && smoothedRot < 90) // Turn Right
        {
            smoothedRot += 1.8f;
            //Double change rate if currently turning opposite way
            if (smoothedRot < 0)
            {
                smoothedRot += 1.8f;
            }
        } else //Center the wheel when no turn is applied
        {
            if (!Input.GetKey("left ctrl"))//Allows for fine control while holding left ctrl
            {
                if (smoothedRot > 3.5)
                {
                    smoothedRot -= 3.6f;
                }
                else if (smoothedRot < -3.5)
                {
                    smoothedRot += 3.6f;
                }
                else
                {
                    smoothedRot = 0f;
                }
            }
        }
        rotationEuler.y = smoothedRot*(Mathf.Clamp((Mathf.Abs(currentSpeed) * 50), 0f, 8f)/8);//Apply the wheel position as a function of speed (prevents turning at stops)
        carTf.Rotate(rotationEuler*Time.deltaTime);//Rotate based on deltaTime
    }
}
