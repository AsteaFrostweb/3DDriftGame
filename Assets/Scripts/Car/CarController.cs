using UnityEngine;
using System.Collections.Generic;
using Car;
using Game.Utility;
using System.Linq;
using System;
using Assets.Scripts.Utility;
using UnityEngine.Rendering;
using Unity.VisualScripting;

namespace Car
{
    public enum Axel { Front, Rear }

    [System.Serializable]
    public struct Wheel
    {
        public GameObject model;
        public WheelCollider collider;
        public Axel axel;
    }
}

public class CarController : MonoBehaviour
{
    private Rigidbody carRB;

    //--------------INPUTS-----------------
    [Header("Needed Objects")]
    [SerializeField] private GameObject brakelightObject;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private float gravityScale;
    [Header("Driving Parameters")]
    [SerializeField] private float maxSpeed = 40.0f;
    [SerializeField] private float acceleration = 100.0f;
    [SerializeField] private float brakingAcceleration = 300.0f;
    [SerializeField] private float maxTurnAngle = 45.0f;
    [SerializeField] private float turnSpeed = 2.0f;
    [SerializeField] private float turnDampeningRate = 2.0f;
    [Header("Drifting Parameters")]
    [SerializeField] private float driftAngleThreshold = 10.0f;
    [SerializeField] private float movingSpeedThreshold = 0.1f;
    [Header("Grip Parameters")]
    [SerializeField] private float baseGrip = 2.0f;
    [SerializeField] private float driftGripReduction = 0.5f;
    [SerializeField] private float brakingGripReduction = 0.5f;

    //--------------OUTPUTS-----------------
    [Header("Outputs")]
    [SerializeField]
    private float currentForwardSpeed = 0f;
    [SerializeField]
    private float currentSteeringAngle = 0f;
    [SerializeField]
    private float currentDriftAngle = 0f;
    [SerializeField]
    private bool isDrifting = false;
    [SerializeField]
    private bool isMoving = false;

    [Header("Wheels")]
    [SerializeField] private List<Car.Wheel> wheels;

    //Input Getters
    public float Acceleration { get { return acceleration; }  }
    public float BrakingAcceleration { get { return brakingAcceleration;  } }
    public float MaxTurnAngle { get { return maxTurnAngle; } }
    public float TurnSpeed { get { return turnSpeed; } }
    public float TurnDampeningRate { get { return turnDampeningRate; } }






    // Output Getters
    public float CurrentForwardSpeed { get { return currentForwardSpeed; } }
    public float CurrentDriftAngle { get { return currentDriftAngle; } }
    public float CurrentSteeringAngle { get { return currentSteeringAngle; } }
    public bool IsDrifting { get { return isDrifting; } }
    public bool IsMoving { get { return isMoving; } }



    //PlayerInput variables
    public float horizontalInput { get; private set; }
    public float verticalInput { get; private set; }
    public bool isBraking { get; private set; }



    //Events
    public event Action OnCrash;
    public event Action OnBrakeStart;
    public event Action OnDriftStart;
    public event Action OnBrakeEnd;
    public event Action OnDriftEnd;

    //ChangeTrackers
    ChangeTracker<bool> brakingCT;
    ChangeTracker<bool> driftingCT;


    private Vector3 positionLastFrame;
    public bool ControlLocked { get; set; } = true;

    private void Start()
    {
        positionLastFrame = transform.position;
        Physics.gravity = new Vector3(0f,gravityScale, 0f);

        //Subscribing to events
        OnBrakeStart += () => HandleBrakeStart();
        OnBrakeEnd += () => HandleBrakeEnd();
        OnDriftStart += () => HandleDriftStart();
        OnDriftEnd += () => HandleDriftEnd();

        //initalizing change tackers
        brakingCT = new ChangeTracker<bool>(() => isBraking);
        driftingCT = new ChangeTracker<bool>(() => IsDrifting);

        //Getting and initializing the car rigidbody
        carRB = GetComponent<Rigidbody>();       
        carRB.centerOfMass = centerOfMass.localPosition;
    }

 

    private void Update()
    {      
        GetInput();
        GetEvents(); 
        UpdateOuputs();
        AnimateWheels();
    }   

    private void FixedUpdate()
    {        
        Brake();
        Move();
        Steer();
    }

    private void GetInput()
    {
        if (ControlLocked) return;
        
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetButton("Brake");
        
    }
    public void UpdateOuputs() 
    {
        currentForwardSpeed = GameMath.GetAxialSpeed(GameMath.Axis.Forward, carRB, transform);
        currentDriftAngle = GetDriftAngle();
        currentSteeringAngle = wheels.First(w => w.axel == Axel.Front).collider.steerAngle;    
        isMoving = carRB.velocity.magnitude >= movingSpeedThreshold;
        isDrifting = (currentDriftAngle >= driftAngleThreshold) && isMoving;

        positionLastFrame = transform.position;
    }
    private void GetEvents() 
    {
        if (brakingCT.Update()) //returns true if the valuer is different than last time
        {
            if (isBraking) { OnBrakeStart?.Invoke(); } 
            else { OnBrakeEnd?.Invoke(); }
        }
        if (driftingCT.Update()) //returns true if the valuer is different than last time
        {
            if (isDrifting) { OnDriftStart?.Invoke(); }
            else { OnDriftEnd?.Invoke(); }
        }

    }
    private void Move()
    {
        if (carRB.velocity.magnitude < maxSpeed)
        {
            foreach (Car.Wheel wheel in wheels)
            {
                wheel.collider.motorTorque = verticalInput * acceleration;
            }
        }
        else 
        {
            foreach (Car.Wheel wheel in wheels)
            {
                wheel.collider.motorTorque = 0f;
            }
        }
      
    }

    private void Steer()
    {
        foreach (Car.Wheel wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                float targetSteerAngle = maxTurnAngle * horizontalInput;
                if (horizontalInput == 0)
                {
                    wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed * turnDampeningRate);
                }
                else 
                {
                    wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
                }
            }
        }
    }

    private void Brake() 
    {
        if (isBraking)
        {            
            foreach (Car.Wheel wheel in wheels)
            {
                wheel.collider.brakeTorque = brakingAcceleration;
            }
        }
        else 
        {
            foreach (Car.Wheel wheel in wheels)
            {
                wheel.collider.brakeTorque = 0f;
            }
        }
    }


    private void HandleBrakeStart() 
    {
        brakelightObject.SetActive(true);
        SetGrip();
    }
    private void HandleBrakeEnd()
    {
        brakelightObject.SetActive(false);
        SetGrip();
    }
    private void HandleDriftStart()
    {
        SetGrip();
    }
    private void HandleDriftEnd()
    {
        SetGrip();
    }

    private void AnimateWheels()
    {
        foreach (Car.Wheel wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;

            wheel.collider.GetWorldPose(out pos, out rot);
            wheel.model.transform.position = pos;
            wheel.model.transform.rotation = rot;
           
        }
    }

    private void SetGrip() 
    {
        float grip = GetGrip();
        foreach (Wheel wheel in wheels)
        {
            WheelCollider collider = wheel.collider;
            WheelFrictionCurve Sfriction = collider.sidewaysFriction;
            Sfriction.stiffness = grip;
            WheelFrictionCurve Ffriction = collider.sidewaysFriction;
            Ffriction.stiffness = grip;
            collider.sidewaysFriction = Sfriction;
            collider.forwardFriction = Ffriction;
        }
    }
  


    private void OnCollisionEnter(Collision collision)
    {        
        if (collision.collider.tag == "Barrier")
        {
            OnCrash?.Invoke();
        }
    }



    //Funcitions
    private float GetGrip() 
    {
        float grip = baseGrip;
        if(isDrifting) grip -= driftGripReduction;
        if(isBraking) grip -= brakingGripReduction;
        return grip;
    }

    private float GetDriftAngle() 
    {
        float angle = Vector3.Angle(transform.forward.normalized, carRB.velocity.normalized);
        if ((180 - angle) < angle) 
        {
            angle = 180 - angle;
        }
        return angle;
    }
}
