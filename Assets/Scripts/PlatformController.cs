using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RayCastController
{
    //maybe change it to not be public
    public Vector3[] localWayPoints;
    Vector3[] globalWaypoints;



    public float speed;
    int fromWaypointIndex;
    float percentBetweenWaypoints;



    public float waitTime;
    float nextMoveTime;
    [Range(0,2)]
    public float easeAmmount;



    public bool cyclic;



    public LayerMask passengerMask;



    List <PassengerMovement> passengerMovements;



    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D> ();



    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;


        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }



    private void OnDrawGizmos()
    {
        if (localWayPoints != null)
        {
            Gizmos.color = Color.red;
            float size = 0.3f;



            for (int i = 0; i < localWayPoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWayPoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }



    float Ease(float x)
    {
        float a = easeAmmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }



    void MovePassengers(bool beforeMovePlatform)
    {
        foreach(PassengerMovement passenger in passengerMovements)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }



    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovements = new List<PassengerMovement>();



        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);



        //vertically moving platform
        if (velocity.y != 0)
        {
            float rayLenght = Mathf.Abs(velocity.y) + skinWidth;



            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLenght, passengerMask);


                Debug.DrawRay(rayOrigin, Vector2.up * directionY* rayLenght*20, Color.green);



                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);



                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;



                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }



        //horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLenght = Mathf.Abs(velocity.x) + skinWidth;



            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLenght, passengerMask);



                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLenght * 20, Color.green);



                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);



                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;



                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }



        //when platform is moving horizontall or downward
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLenght = skinWidth * 2;



            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLenght, passengerMask);



                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);



                        float pushX = velocity.x;
                        float pushY = velocity.y;



                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }



    Vector3 CalculatePlatformMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }



        fromWaypointIndex %= globalWaypoints.Length;



        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += speed/distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);



        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);



        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;


            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;



                    System.Array.Reverse(globalWaypoints);
                }
            }



            nextMoveTime = Time.time + waitTime;
        }



        return newPos - transform.position;
    }



    public override void Start()
    {
        base.Start();



        globalWaypoints = new Vector3[localWayPoints.Length];
        for (int i = 0; i < localWayPoints.Length; i++)
        {
            globalWaypoints[i] = localWayPoints[i] + transform.position;
        }
    }


    private void FixedUpdate()
    {
        UpdateRaycastOrigins();



        //fixed update??
        Vector3 velocity = CalculatePlatformMovement();



        CalculatePassengerMovement(velocity);



        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }
}