using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CubizationTest : MonoBehaviour
{
	const int POINTARRAYSIZE = 20000;

	[SerializeField]
	public bool recolor;
	[SerializeField]
	public float pointSize;
	[SerializeField]
	public float pointDistance;
	[SerializeField]
	public float pointCount;
	[SerializeField]
	CubizationTestModes mode = CubizationTestModes.Random;

	CubizationTestPoint[] points = new CubizationTestPoint[POINTARRAYSIZE];

    // Start is called before the first frame update
    void Start()
    {
    	if(mode == CubizationTestModes.Random)
    	{
	    	for(int i = 0; i < Mathf.Min(pointCount, POINTARRAYSIZE); i++)
	    	{
	    		CubizationTestPoint newPoint = new CubizationTestPoint(Random.onUnitSphere * pointDistance);
	    		points[i] = newPoint;
	    	}   
	    }

	    else if(mode == CubizationTestModes.GreatCircles)
	    {
	    	float step = 360f / (Mathf.Floor(pointCount / 3f));
	    	float theta = 0f;
	    	int i = 0;
	    	for(; theta <= 360f; theta += step, i += 3)
	    	{
	    		float sinTheta = Mathf.Sin(theta);
	    		float cosTheta = Mathf.Cos(theta);

	    		points[i] = new CubizationTestPoint(new Vector3(0, cosTheta, sinTheta), Color.red);
	    		points[i+1] = new CubizationTestPoint(new Vector3(sinTheta, cosTheta, 0), Color.blue);
	    		points[i+2] = new CubizationTestPoint(new Vector3(cosTheta, 0, sinTheta), Color.green);
	    	}
	    }
    }

    // Update is called once per frame
    void Update()
    {
    	if(Input.GetKeyDown(KeyCode.Space))
    	{
    		for(int i = 0; i < Mathf.Min(pointCount, POINTARRAYSIZE); i++)
    		{
    			points[i] = cubizePoint(points[i], recolor);
    		}
    	}
    }

    CubizationTestPoint cubizePoint(CubizationTestPoint spherePoint, bool recolor)
    {
        float absX = Mathf.Abs(spherePoint.pos.normalized.x);
        float absY = Mathf.Abs(spherePoint.pos.normalized.y);
        float absZ = Mathf.Abs(spherePoint.pos.normalized.z);
        
        Vector3 newPos = spherePoint.pos / Mathf.Max(absX, absY, absZ);

        Color newColor;
        if(recolor)
        {
            if(Mathf.Approximately(newPos.x, -pointDistance) || Mathf.Approximately(newPos.x, pointDistance)) { newColor = Color.red; }
            else if(Mathf.Approximately(newPos.y, -pointDistance) || Mathf.Approximately(newPos.y, pointDistance)) { newColor = Color.green; }
            else if(Mathf.Approximately(newPos.z, -pointDistance) || Mathf.Approximately(newPos.z, pointDistance)) { newColor = Color.blue; }
            else 
            {
                newColor = Color.black;
                print("Non-planar point found. " + newPos.ToString());
            }
        }

        else { newColor = spherePoint.color; }

        // Scale between 0 and 1, for texture mapping convenience.
        // This is done AFTER checking colors, for the test, because
        // otherwise things will register as non-planar in error.
        newPos = (newPos + Vector3.one) * 0.5f;

        return new CubizationTestPoint(newPos, newColor);
    }

    void OnDrawGizmos()
    {
    	for(int i = 0; i < Mathf.Min(pointCount, POINTARRAYSIZE); i++)
    	{
    		Gizmos.color = points[i].color;
    		Gizmos.DrawSphere(points[i].pos, pointSize);
    	}
    }
}

struct CubizationTestPoint
{
	public Vector3 pos;
	public Color color;

	public CubizationTestPoint(Vector3 pos, Color color)
	{
		this.pos = pos;
		this.color = color;
	}

	public CubizationTestPoint(Vector3 pos)
	{
		this.pos = pos;
		this.color = Color.black;
	}
}

enum CubizationTestModes { Random, GreatCircles };