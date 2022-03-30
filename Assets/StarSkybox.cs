using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class StarSkybox : MonoBehaviour
{
	public TextAsset starFile;
    public float magnitude_weight;
	public int cubemapSize;
    public Color nightSkyColor;

	Cubemap tex;
	Material mat;
	List<TexPoint> gizmoPoints = new List<TexPoint>();
	List<Star> gizmoStars = new List<Star>();

    // Start is called before the first frame update
    void Start()
    {
    	DrawStarSkybox();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("RedrawStars"))
        {
        	DrawStarSkybox();
        }
    }

    void DrawStarSkybox()
    {
    	tex = new Cubemap(cubemapSize, TextureFormat.ARGB32, false);
    	mat = new Material(Shader.Find("Skybox/Cubemap"));

    	List<Star> stars = new List<Star>();
    	List<TexPoint> points = new List<TexPoint>();

        ReadStars(starFile, stars);
        CubizeStars(stars);
        UnwrapStars(stars, points);
        WriteToTexture(points, tex);
        UploadAsSkybox(tex);
    }

    // Reads all stars in star file, and places them in the stars array
    void ReadStars(TextAsset f, List<Star> stars)
    {
        string[] lines = f.text.Split('\n');
        // Regex matches something of the format [right ascension] +/-[declination]::::[(r, g, b)]::::[visual magnitude]
        Regex re = new Regex(@"(\d+):(\d+):([\d\.]+) ([+-])(\d+):(\d+):(\d+)::::\((\d+), (\d+), (\d+)\)::::([\d\.]+)", RegexOptions.Compiled);
        System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Number;

        foreach (string line in lines)
        {
            Match m = re.Match(line);

            if(!m.Success) { print("Line skipped."); continue; }

            float RA_archours = Single.Parse(m.Groups[1].ToString(), style);
            float RA_arcminutes = Single.Parse(m.Groups[2].ToString(), style);
            float RA_arcseconds = Single.Parse(m.Groups[3].ToString(), style);
            char sign = char.Parse(m.Groups[4].ToString());
            float DEC_degrees = Single.Parse(m.Groups[5].ToString(), style);
            float DEC_arcminutes = Single.Parse(m.Groups[6].ToString(), style);
            float DEC_arcseconds = Single.Parse(m.Groups[7].ToString(), style);

            // Unity colors are 0...1, so normalizing is necessary.
            float r = Int32.Parse(m.Groups[8].ToString()) / 255f;
            float g = Int32.Parse(m.Groups[9].ToString()) / 255f;
            float b = Int32.Parse(m.Groups[10].ToString()) / 255f;

            float visual_mag = Single.Parse(m.Groups[11].ToString(), style);

            float right_ascension = (RA_archours*15f + RA_arcminutes/4 + RA_arcseconds/240 + 90); 
            float declination = 90 + (sign == '+' ? -1 : 1) * (DEC_degrees + DEC_arcminutes/60f + DEC_degrees/3600f);

            // Convert to cartesian coordinates from celestial (spherical) coordinates.
            float x = Mathf.Sin(Mathf.Deg2Rad * declination) * Mathf.Cos(Mathf.Deg2Rad * right_ascension);
            float y = Mathf.Cos(Mathf.Deg2Rad * declination);
            float z = Mathf.Sin(Mathf.Deg2Rad * declination) * Mathf.Sin(Mathf.Deg2Rad * right_ascension);

            

            Vector3 pos = new Vector3(x, y, z);
            Color c = new Color(r, g, b);
            stars.Add(new Star(pos, c, visual_mag));
        }
}

    // Projects each star to its closest point on a cube circumscribing the celestial sphere
    void CubizeStars(List<Star> stars)
    {
    	for(int i = 0; i < stars.Count; i++)
    	{
    		// Divide each coordinate by the absolute value of the largest coordinate.
    		// This ensures the largest coordinate will be 1, as defines a cube, while 
    		// other coordinates remain proportional, thus getting the nearest
    		// point on the cube.
    		Star s = stars[i];
	        float absX = Mathf.Abs(s.pos.x);
	        float absY = Mathf.Abs(s.pos.y);
	        float absZ = Mathf.Abs(s.pos.z);
	        
	        Vector3 newPos = s.pos / Mathf.Max(absX, absY, absZ);

	        stars[i] = new Star(newPos, s.apparentColor);
    	}

    }

    // Unwraps the 3D cube into a 2D plane, as a net
    void UnwrapStars(List<Star> stars, List<TexPoint> points)
    {
    	for(int i = 0; i < stars.Count; i++)
    	{
    		Star s = stars[i];
    		float absX = Mathf.Abs(s.pos.x);
    		float absY = Mathf.Abs(s.pos.y);
    		float absZ = Mathf.Abs(s.pos.z);

    		CubemapFace face;
    		Vector2 textureCoord;

    		// Determine which coordinates to use after unwrapping to cubemap
	        if(absX >= absY && absX >= absZ)
	        {
	            if(s.pos.x > 0)
	            {
	                face = CubemapFace.PositiveX;
	                textureCoord = new Vector2(-s.pos.z, s.pos.y);
	            }

	            else
	            {
	                face = CubemapFace.NegativeX;
	                textureCoord = new Vector2(s.pos.z, s.pos.y);
	            }
	        }

	        else if(absY > absX && absY >= absZ)
	        {
	            if(s.pos.y > 0)
	            {
	                face = CubemapFace.PositiveY;
	                textureCoord = new Vector2(s.pos.x, -s.pos.z);
	            }

	            else
	            {
	                face = CubemapFace.NegativeY;
	                textureCoord = new Vector2(s.pos.x, s.pos.z);
	            }

	        }

	        else
	        {
	            if(s.pos.z > 0)
	            {
	                face = CubemapFace.PositiveZ;
	                textureCoord = new Vector2(s.pos.x, s.pos.y);
	            }

	            else
	            {
	                face = CubemapFace.NegativeZ;
	                textureCoord = new Vector2(-s.pos.x, s.pos.y);
	            }

	        }

	        // Determine color from linear magnitude, scaled to crudely account for atmospheric and observer effects.
            float linear_magnitude = Mathf.Pow(-2.5f, s.visual_mag);
            Color scaled_color = s.apparentColor * Mathf.Min(1.5f, linear_magnitude * magnitude_weight);
            textureCoord = (textureCoord + Vector2.one) / 2;
    		points.Add(new TexPoint(face, textureCoord, scaled_color));
        }
    }

    // Writes the 2D net into a texture 
    void WriteToTexture(List<TexPoint> points, Cubemap t)
    {
        // Set the whole texture to black first
        Color[] fillArray = new Color[cubemapSize * cubemapSize];

        for(int i = 0; i < cubemapSize * cubemapSize; i++)
        {
            fillArray[i] = nightSkyColor;
        }

        t.SetPixels(fillArray, CubemapFace.PositiveX);
        t.SetPixels(fillArray, CubemapFace.PositiveY);
        t.SetPixels(fillArray, CubemapFace.PositiveZ);
        t.SetPixels(fillArray, CubemapFace.NegativeX);
        t.SetPixels(fillArray, CubemapFace.NegativeY);
        t.SetPixels(fillArray, CubemapFace.NegativeZ);

    	for(int i = 0; i < points.Count; i++)
    	{
    		TexPoint p = points[i];
            // Each texture places (0,0) at the top-left corner, so the Y coordinate must be inverted.
    		t.SetPixel(p.face, Mathf.RoundToInt(cubemapSize * p.pos.x), Mathf.RoundToInt(cubemapSize * ( 1 - p.pos.y)), p.color);
    	}

    	t.Apply();
    }

    //  Uses the texture as a cubemap and sets this as the skybox
    void UploadAsSkybox(Cubemap t)
    {
    	mat.SetTexture("_Tex", t);
    	RenderSettings.skybox = mat;
    }

    void OnDrawGizmos()
    {
    	for(int i = 0; i < gizmoPoints.Count; i++)
    	{
            Vector2 faceOffset;
    		TexPoint p = gizmoPoints[i];

            switch(p.face)
            {
                case(CubemapFace.PositiveX):
                    faceOffset = new Vector2(2, 1);
                    break;

                case(CubemapFace.PositiveY):
                    faceOffset = new Vector2(1, 2);
                    break;

                case(CubemapFace.PositiveZ):
                    faceOffset = new Vector2(1, 1);
                    break;

                case(CubemapFace.NegativeX):
                    faceOffset = new Vector2(0, 1);
                    break;

                case(CubemapFace.NegativeY):
                    faceOffset = new Vector2(1, 0);
                    break;

                case(CubemapFace.NegativeZ):
                    faceOffset = new Vector2(3, 1);
                    break;

                default:
                    faceOffset = new Vector3(0, 0);
                    break;
            }

    		Gizmos.color = p.color;
    		Gizmos.DrawSphere(p.pos + faceOffset, 0.05f);
    	}
    }

    struct Star
{
    public Vector3 pos;
    public Color apparentColor;
    public float visual_mag;

    public Star(Vector3 pos, Color apparentColor, float visual_mag=0f)
    {
        this.pos = pos;
        this.apparentColor = apparentColor;
        this.visual_mag = visual_mag;
    }
}

struct TexPoint
{
    public CubemapFace face;
    public Vector2 pos;
    public Color color;

    public TexPoint(CubemapFace face, Vector2 pos, Color color)
    {
        this.face = face;
        this.pos = pos;
        this.color = color;
    }
}
}