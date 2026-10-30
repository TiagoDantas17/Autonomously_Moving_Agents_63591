using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

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
        else
            Debug.LogWarning("Destino de fuga fora do NavMesh");
    }

    void Evade()
    {
        Vector3 targetDir = target.transform.position - transform.position;

        float targetSpeed = 0f;
        Drive drive;
        if (target.TryGetComponent(out drive))
            targetSpeed = Mathf.Abs(drive.currentSpeed);
        else
            Debug.LogWarning("O Target não tem o script Drive");

        float lookAhead = targetDir.magnitude / (agent.speed + targetSpeed + 0.01f);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    void Update()
    {
        if (agent == null || target == null) return;

        Evade();
    }
}