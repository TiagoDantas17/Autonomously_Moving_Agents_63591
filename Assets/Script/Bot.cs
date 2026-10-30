using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;

    // Distâncias de deteção
    public float detectDistance = 15f;  // a partir daqui reage ao polícia
    public float loseDistance = 25f;    // a partir daqui "perde-o de vista" e volta a vaguear

    bool reacting = false;

    // Wander
    Vector3 wanderTarget = Vector3.zero;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTarget = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));

        if (agent == null)
            Debug.LogError("O ladrão não tem NavMeshAgent!");
        if (target == null)
            Debug.LogError("O campo Target está vazio!");
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeDirection = (transform.position - location).normalized;
        Vector3 destination = transform.position + fleeDirection * 10f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, 10f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void Pursue()
    {
        Vector3 targetDir = target.transform.position - transform.position;

        float targetSpeed = 0f;
        Drive drive;
        if (target.TryGetComponent(out drive))
            targetSpeed = Mathf.Abs(drive.currentSpeed);

        float relativeHeading = Vector3.Angle(transform.forward, target.transform.forward);
        float toTarget = Vector3.Angle(transform.forward, targetDir);

        if ((toTarget > 90 && relativeHeading < 20) || targetSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + targetSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade()
    {
        Vector3 targetDir = target.transform.position - transform.position;

        float targetSpeed = 0f;
        Drive drive;
        if (target.TryGetComponent(out drive))
            targetSpeed = Mathf.Abs(drive.currentSpeed);

        float lookAhead = targetDir.magnitude / (agent.speed + targetSpeed + 0.01f);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    void Wander()
    {
        float wanderRadius = 5;
        float wanderDistance = 15;
        float wanderJitter = 0.5f;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                    0,
                                    Random.Range(-1.0f, 1.0f) * wanderJitter);

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = transform.TransformPoint(targetLocal);

        Seek(targetWorld);
    }

    void Update()
    {
        if (agent == null || target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        // Histerese: começa a reagir a 15 e só desiste a 25, para não ficar a mudar de comportamento
        if (distance < detectDistance)
            reacting = true;
        else if (distance > loseDistance)
            reacting = false;

        if (reacting)
            Evade();   // troca por Pursue(); se quiseres que ele persiga
        else
            Wander();
    }
}