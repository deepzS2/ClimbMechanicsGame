using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public static GameObject FindClosestWithTag(string tag, Vector3 position)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        float closestDistance = Mathf.Infinity;

        GameObject closest = null;

        foreach (GameObject gameObject in objects)
        {
            float currentDistance;
            currentDistance = Vector3.Distance(position, gameObject.transform.position);

            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closest = gameObject;
            }
        }

        return closest;
    }
}
