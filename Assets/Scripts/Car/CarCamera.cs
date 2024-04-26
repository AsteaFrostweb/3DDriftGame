using UnityEditor;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public float smoothSpeed = 5f; // Speed of rotation smoothing
    public float smoothRot = 5f; // Speed of rotation smoothing
    public float angle;

    [SerializeField]
    private Transform target; // The car to follow
    private Rigidbody targetRigidbody; // Reference to the car's Rigidbody component

    private void Start()
    {
        target = GameObject.Find("Player1")?.transform;       


    }

    private void Update()
    {
        if (target == null) {target = GameObject.Find("Player1")?.transform; return;}

        if (targetRigidbody == null) { targetRigidbody = target.GetComponent<Rigidbody>(); return; }// Get the Rigidbody component from the target     


        float angle = GetAngle(); //Gets the cars signed drift angle
        float targetAngleY = angle + target.rotation.eulerAngles.y;
        float difference = targetAngleY - target.rotation.eulerAngles.y;


        transform.position = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);

        if (target.GetComponent<Rigidbody>().velocity.magnitude > 1f)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngleY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, (smoothRot) * Time.deltaTime); 
        }
       
    }

    private float GetAngle() 
    {
        Vector3 targetForward = target.forward.normalized;
        Vector3 velocityForward = targetRigidbody.velocity.normalized;
        return Vector3.SignedAngle(targetForward, velocityForward, transform.up);
    }
}
