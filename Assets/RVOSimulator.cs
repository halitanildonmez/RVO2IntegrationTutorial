using UnityEngine;
using System.Collections;
using RVO;

public class RVOSimulator : MonoBehaviour {

    public int agents = 100;
    public float ringSize = 100;
    public GameObject prefab;

    // goal vectors for each agent
    Vector3[] goals;
    // agents in the Unity scene
    GameObject[] RVOAgents;

    RVO.Vector2 toRVOVector(Vector3 param)
    {
        return new RVO.Vector2(param.x, param.z);
    }

    Vector3 toUnityVector(RVO.Vector2 param)
    {
        return new Vector3(param.x(), 0, param.y());
    }

    // Use this for initialization
    void Start () {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 1.0f, 1.0f, new RVO.Vector2(0.0f, 0.0f));

        RVOAgents = new GameObject[agents];
        goals = new Vector3[agents];

        Vector3 facingOrigin = new Vector3(0, 0, 0);

        for (int i = 0; i < agents; i++)
        {
            float angle = ((float)i / agents) * (float)System.Math.PI * 2;
            Vector3 pos = new Vector3((float)System.Math.Cos(angle), 0, (float)System.Math.Sin(angle)) * ringSize;
            Vector3 antipodal = -pos;

            GameObject go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.Euler(0, angle + 180, 0)) as GameObject;

            go.transform.parent = transform;
            go.transform.position = pos;

            Vector3 dir = (facingOrigin - go.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            go.transform.rotation = Quaternion.Slerp(go.transform.rotation, lookRotation, 1);

            goals[i] = antipodal;
            RVOAgents[i] = go;

            Simulator.Instance.addAgent(toRVOVector(go.transform.position));
        }
    }
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i < Simulator.Instance.getNumAgents(); i++)
        {
            RVO.Vector2 agentLoc = Simulator.Instance.getAgentPosition(i);
            RVO.Vector2 goalVector = toRVOVector(goals[i]) - agentLoc;

            if (RVOMath.absSq(goalVector) > 1.0f)
            {
                goalVector = RVOMath.normalize(goalVector);
            }

            Simulator.Instance.setAgentPrefVelocity(i, goalVector);

            RVOAgents[i].transform.localPosition = toUnityVector(agentLoc);
        }
        Simulator.Instance.doStep();
    }
}
