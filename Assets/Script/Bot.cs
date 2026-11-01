using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    Drive ds;

    public float detectRange = 20f;   // distância a partir da qual reage ao polícia
    public float viewAngle = 60f;     // ângulo em que o polícia "vê" o ladrão
    public float hideTime = 5f;       // segundos que fica escondido

    bool hiding = false;
    Vector3 wanderTarget = Vector3.zero;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTarget = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        if (agent == null) Debug.LogError("O ladrão não tem NavMeshAgent!");
        if (target == null) { Debug.LogError("O campo Target está vazio!"); return; }

        ds = target.GetComponent<Drive>();
        if (ds == null) Debug.LogError("O Target não tem o script Drive!");
        if (World.Instance.GetHidingSpots().Length == 0)
            Debug.LogError("Não há objetos com a tag 'hide' na cena!");
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    float TargetSpeed()
    {
        return ds != null ? Mathf.Abs(ds.currentSpeed) : 0f;
    }

    // SEGUIR: vai para onde o polícia vai estar
    void Pursue()
    {
        Vector3 targetDir = target.transform.position - transform.position;

        float relativeHeading = Vector3.Angle(transform.forward, target.transform.forward);
        float toTarget = Vector3.Angle(transform.forward, targetDir);

        if ((toTarget > 90 && relativeHeading < 20) || TargetSpeed() < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude / (agent.speed + TargetSpeed());
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    // VAGUEAR: anda sem destino
    void Wander()
    {
        float wanderRadius = 5;
        float wanderDistance = 15;
        float wanderJitter = 0.5f;

        wanderTarget += new Vector3(Random.Range(-1f, 1f) * wanderJitter, 0,
                                    Random.Range(-1f, 1f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Seek(transform.TransformPoint(targetLocal));
    }

    // FUGIR: esconde-se atrás do objeto mais próximo
    void CleverHide()
    {
        GameObject[] spots = World.Instance.GetHidingSpots();
        if (spots.Length == 0) return;

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = null;

        for (int i = 0; i < spots.Length; i++)
        {
            Vector3 hideDir = spots[i].transform.position - target.transform.position;
            Vector3 hidePos = spots[i].transform.position + hideDir.normalized * 100;

            float d = Vector3.Distance(transform.position, hidePos);
            if (d < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = spots[i];
                dist = d;
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;

        if (hideCol != null && hideCol.Raycast(backRay, out info, 100f))
            Seek(info.point + chosenDir.normalized * 5);
    }

    // O polícia está virado para o ladrão e sem obstáculos pelo meio?
    bool PoliceSeesMe()
    {
        Vector3 toMe = transform.position - target.transform.position;

        if (Vector3.Angle(target.transform.forward, toMe) > viewAngle)
            return false;

        Vector3 origin = target.transform.position + Vector3.up;
        Vector3 dir = (transform.position + Vector3.up) - origin;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit))
            return hit.transform == transform || hit.transform.IsChildOf(transform);

        return false;
    }

    void StopHiding()
    {
        hiding = false;
    }

    void Update()
    {
        if (agent == null || target == null || hiding) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > detectRange)
        {
            Wander();                                  // polícia longe
        }
        else if (PoliceSeesMe())
        {
            CleverHide();                              // polícia a ver -> foge e esconde-se
            hiding = true;
            Invoke("StopHiding", hideTime);
        }
        else
        {
            Pursue();                                  // polícia de costas -> segue
        }
    }
}