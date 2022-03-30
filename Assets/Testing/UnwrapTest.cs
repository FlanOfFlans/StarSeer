using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnwrapTest : MonoBehaviour
{
	const int POINTARRAYSIZE = 20000;

    [SerializeField]
    public int pointCount;
	[SerializeField]
	public float pointSize;
	[SerializeField]
	UnwrapTestMode mode = UnwrapTestMode.Arrows;

    UnwrapTestStar[] stars = new UnwrapTestStar[POINTARRAYSIZE];
	UnwrapTestPoint[] points = new UnwrapTestPoint[POINTARRAYSIZE];
    bool unwrapped = false;

    // Start is called before the first frame update
    void Start()
    {
    	if(mode == UnwrapTestMode.Arrows)
    	{
            Vector3[] positions = {
                new Vector3(0f, 1f, 1f), // Z+ face
                new Vector3(0f, 0.6f, 1f),
                new Vector3(0f, 0.2f, 1f),
                new Vector3(0f, -0.2f, 1f),
                new Vector3(0f, -0.6f, 1f),
                new Vector3(0f, -1f, 1f),
                new Vector3(-0.33f, -0.6f, 1f),
                new Vector3(-0.66f, -0.2f, 1f),
                new Vector3(-0.33f, -0.2f, 1f),

                new Vector3(0f, 1f, -1f), // Z- face
                new Vector3(0f, 0.6f, -1f),
                new Vector3(0f, 0.2f, -1f),
                new Vector3(0f, -0.2f, -1f),
                new Vector3(0f, -0.6f, -1f),
                new Vector3(0f, -1f, -1f),
                new Vector3(0.33f, -0.6f, -1f),
                new Vector3(0.66f, -0.2f, -1f),
                new Vector3(0.33f, -0.2f, -1f),

                new Vector3(1f, 1f, 0f), // X+ face
                new Vector3(1f, 0.6f, 0f),
                new Vector3(1f, 0.2f, 0f),
                new Vector3(1f, -0.2f, 0f),
                new Vector3(1f, -0.6f, 0f),
                new Vector3(1f, -1f, 0f),
                new Vector3(1f, -0.6f, 0.33f),
                new Vector3(1f, -0.2f, 0.66f),
                new Vector3(1f, -0.2f, 0.33f),

                new Vector3(-1f, 1f, 0f), // X- face
                new Vector3(-1f, 0.6f, 0f),
                new Vector3(-1f, 0.2f, 0f),
                new Vector3(-1f, -0.2f, 0f),
                new Vector3(-1f, -0.6f, 0f),
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f, -0.6f, -0.33f),
                new Vector3(-1f, -0.2f, -0.66f),
                new Vector3(-1f, -0.2f, -0.33f),

                new Vector3(0f, 1f, -1f), // Y+ face
                new Vector3(0f, 1f, -0.6f),
                new Vector3(0f, 1f, -0.2f),
                new Vector3(0f, 1f, 0.2f),
                new Vector3(0f, 1f, 0.6f),
                new Vector3(0f, 1f, 1f),
                new Vector3(-0.33f, 1f, 0.6f),
                new Vector3(-0.66f, 1f, 0.2f),
                new Vector3(-0.33f, 1f, 0.2f),

                new Vector3(0f, -1f, 1f), // Y- face
                new Vector3(0f, -1f, 0.6f),
                new Vector3(0f, -1f, 0.2f),
                new Vector3(0f, -1f, -0.2f),
                new Vector3(0f, -1f, -0.6f),
                new Vector3(0f, -1f, -1f),
                new Vector3(-0.33f, -1f, -0.6f),
                new Vector3(-0.66f, -1f, -0.2f),
                new Vector3(-0.33f, -1f, -0.2f),
            };

            Color[] colors = {
                new Color(0, 0, 255),
                new Color(0, 0, 128),
                new Color(255, 0, 0),
                new Color(128, 0, 0),
                new Color(0, 255, 0),
                new Color(0, 128, 0)
            };

	    	for(int i = 0; i < 6; i++)
	    	{
                for(int j = 0; j < 9; j++)
                {
                    stars[i*9 + j] = new UnwrapTestStar(positions[i*9 + j], colors[i]);
                }
	    	}

            pointCount = 54;
	    }
    }

    // Update is called once per frame
    void Update()
    {
    	if(Input.GetKeyDown(KeyCode.Space))
    	{
    		for(int i = 0; i < Mathf.Min(pointCount, POINTARRAYSIZE); i++)
    		{
    			points[i] = unwrapStar(stars[i]);
    		}
            unwrapped = true;
    	}
    }

    UnwrapTestPoint unwrapStar(UnwrapTestStar s)
    {
        float absX = Mathf.Abs(s.pos.x);
        float absY = Mathf.Abs(s.pos.y);
        float absZ = Mathf.Abs(s.pos.z);

        Vector2 textureCoord;
        Vector2 faceOffset;

        if(absX >= absY && absX >= absZ)
        {
            if(s.pos.x > 0)
            {
                faceOffset = new Vector2(4, 2);
                textureCoord = new Vector2(-s.pos.z, s.pos.y);
            }

            else
            {
                faceOffset = new Vector2(0, 2);
                textureCoord = new Vector2(s.pos.z, s.pos.y);
            }
        }

        else if(absY > absX && absY >= absZ)
        {
            if(s.pos.y > 0)
            {
                faceOffset = new Vector2(2, 4);
                textureCoord = new Vector2(s.pos.x, -s.pos.z);
            }

            else
            {
                faceOffset = new Vector2(2, 0);
                textureCoord = new Vector2(s.pos.x, s.pos.z);
            }

        }

        else
        {
            if(s.pos.z > 0)
            {
                faceOffset = new Vector2(2, 2);
                textureCoord = new Vector2(s.pos.x, s.pos.y);
            }

            else
            {
                faceOffset = new Vector2(6, 2);
                textureCoord = new Vector2(-s.pos.x, s.pos.y);
            }

        }

        return new UnwrapTestPoint(faceOffset + textureCoord, s.color);
    }

    void OnDrawGizmos()
    {
    	for(int i = 0; i < Mathf.Min(pointCount, POINTARRAYSIZE); i++)
    	{
            if(unwrapped)
            {
                Gizmos.color = points[i].color;
                Gizmos.DrawSphere(points[i].pos, pointSize);
            }

            else
            {
                Gizmos.color = stars[i].color;
                Gizmos.DrawSphere(stars[i].pos, pointSize);                
            }
    	}
    }
}

struct UnwrapTestPoint
{
    public Vector2 pos;
    public Color color;

    public UnwrapTestPoint(Vector2 pos, Color color)
    {
        this.pos = pos;
        this.color = color;
    }

    public UnwrapTestPoint(Vector2 pos)
    {
        this.pos = pos;
        this.color = Color.black;
    }
}    

struct UnwrapTestStar
{
	public Vector3 pos;
	public Color color;

	public UnwrapTestStar(Vector3 pos, Color color)
	{
		this.pos = pos;
		this.color = color;
	}

	public UnwrapTestStar(Vector3 pos)
	{
		this.pos = pos;
		this.color = Color.black;
	}
}

enum UnwrapTestMode {Arrows};