using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    void Pursue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;

        float targetSpeed = Mathf.Abs(target.GetComponent<Drive>().currentSpeed);

        // Se o alvo está parado, ou está à frente a vir na minha direção, basta fazer Seek
        float relativeHeading = Vector3.Angle(this.transform.forward, target.transform.forward);
        float toTarget = Vector3.Angle(this.transform.forward, targetDir);

        if ((toTarget > 90 && relativeHeading < 20) || targetSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        // Quanto tempo vou demorar a chegar, e onde o alvo vai estar nesse tempo
        float lookAhead = targetDir.magnitude / (agent.speed + targetSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Update()
    {
        Pursue();
    }
}