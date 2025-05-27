using UnityEngine;
using Pathfinding;

public class FlyingEnemyAI : MonoBehaviour
{
    public Transform target;



    public float flyingEnemySpeed = 200f;
    public float nextWayPointDistance = 3f;



    public Transform enemyGFX;



    Path path;
    int currentWayPoint = 0;
    bool reachedEndOfPath = false;



    Seeker seeker;
    Rigidbody2D rb;



    [SerializeField] float timeBetweenUpdatePath = 0.5f;



    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();



        InvokeRepeating("UpdatePath", 0f, timeBetweenUpdatePath);
    }



    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }



    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }



    private void FixedUpdate()
    {
        if(path == null)
        {
            return;
        }



        if(currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath=true;
            return;
        }
        else
        {
            reachedEndOfPath=false;
        }



        Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized;
        Vector2 force = direction * flyingEnemySpeed * Time.deltaTime;//ta bort time.deltaTime???



        rb.AddForce(force);



        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);



        if (distance < nextWayPointDistance)
        {
            currentWayPoint++;
        }



        if (rb.linearVelocity.x >= 0.01f)
        {
            enemyGFX.localScale = new Vector3(1, 1, 1);
        }
        else if (rb.linearVelocity.x <= -0.01f)
        {
            enemyGFX.localScale = new Vector3(-1, 1, 1);
        }
    }
}
