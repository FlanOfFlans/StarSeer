using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceholderStarGenerator : MonoBehaviour
{
	public GameObject placeholderStarPrefab;
	public int placeholderStarCount;
	public int placeholderStarDistance;

	Transform t;

    // Start is called before the first frame update
    void Start()
    {
    	t = GetComponent<Transform>();
    	GenerateStars();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateStars()
    {
    	foreach(Transform child in t)
    	{
    		Destroy(child);
    	}

		for(int i = 0; i < placeholderStarCount; i++)
        {
        	Vector3 pos = Random.onUnitSphere * placeholderStarDistance;
        	GameObject o = Instantiate(placeholderStarPrefab, pos, Quaternion.identity, t);
        }
    }
}