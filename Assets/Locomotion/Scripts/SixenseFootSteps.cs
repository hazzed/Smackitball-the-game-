using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SixenseFootSteps : MonoBehaviour 
{
	public float		MinThreshold = 0.1f;
	public Transform	LeftFoot;
	public Transform	RightFoot;
	
	public Texture[]	GrassTextures;
	public Texture[]	TileTextures;
	public Texture[]	WoodTextures;
	
	public AudioClip	DefaultSound;
	public AudioClip	GrassSound;
	public AudioClip	TileSound;
	public AudioClip	WoodSound;
	
	private Animator 	m_animator;
	private float		m_lastFootStepVal;
//	private int			m_terrainTextureIndex = -1;
	
	
	private enum WhichFoot
	{
		RIGHT,
		LEFT
	};
	
	private WhichFoot	m_nextStep = WhichFoot.LEFT;
	
	
	// Use this for initialization
	void Start () 
	{
		m_animator = GetComponentInChildren<Animator>();
	}
	
	
	void FixedUpdate () 
	{
		if ( m_animator )
		{
			// The FootStep AnimationController param is tied to the Mecanim FootStep animation curve
			float footStepVal = m_animator.GetFloat("FootStep");
			
			if ( Mathf.Abs(footStepVal) > MinThreshold && 
				 ( ( m_nextStep == WhichFoot.LEFT && footStepVal < m_lastFootStepVal ) || 
				 ( m_nextStep == WhichFoot.RIGHT && footStepVal > m_lastFootStepVal ) ) )
			{
				DoFootStep( m_nextStep ); 
			}	
			
			m_lastFootStepVal = footStepVal;
		}
	}
	
	
	void DoFootStep( WhichFoot foot )
	{
		m_nextStep = m_nextStep == WhichFoot.LEFT ? WhichFoot.RIGHT : WhichFoot.LEFT;
		
		Vector3 start = ( foot == WhichFoot.LEFT ? LeftFoot.position : RightFoot.position ) + new Vector3( 0, 0.05f, 0 );
				
		Ray groundRay = new Ray( start, Vector3.down );
		RaycastHit hitInfo;
			
		//m_terrainTextureIndex = -1;
		
		Texture stepOnTexture = null;
		
		//Debug.DrawLine( start, start + Vector3.down * 1.0f, Color.cyan, 5 );
		
		if ( Physics.Raycast (groundRay, out hitInfo, 1.0f, 1 << 0 )) 
		{
			TerrainCollider terrainCollider = hitInfo.collider as TerrainCollider;
					
			if ( terrainCollider != null )
			{
				// Terrain
				
				int terrainTextureIndex = GetMainTexture(start);
				
				SplatPrototype[] splats =	Terrain.activeTerrain.terrainData.splatPrototypes;
				
				stepOnTexture = splats[terrainTextureIndex].texture;	
				
				//m_terrainTextureIndex = terrainTextureIndex;
			}
			
			else
			{
				// Mesh CAN'T BE STATIC!!!
				if ( !hitInfo.collider.gameObject.isStatic )
				{	
					MeshFilter meshFilter = hitInfo.collider.gameObject.GetComponent<MeshFilter>();
					if ( meshFilter != null )
					{
						// find the submesh the foot is standing on
						
						Mesh mesh = meshFilter.sharedMesh;
						
						int[] hittedTriangle = new int[3] 
	                    {
	                        mesh.triangles[hitInfo.triangleIndex * 3], 
	                        mesh.triangles[hitInfo.triangleIndex * 3 + 1], 
	                        mesh.triangles[hitInfo.triangleIndex * 3 + 2] 
	                    };
	
						for (int i = 0; i < mesh.subMeshCount; i++)
	                    {
	                        int[] subMeshTris = mesh.GetTriangles(i);
	                        for (int j = 0; j < subMeshTris.Length; j += 3)
	                        {
	                            if (subMeshTris[j] == hittedTriangle[0] &&
	                                subMeshTris[j + 1] == hittedTriangle[1] &&
	                                subMeshTris[j + 2] == hittedTriangle[2])
	                            {
	                                //Debug.Log(string.Format("triangle index:{0} submesh index:{1} submesh triangle index:{2}", hitInfo.triangleIndex, i, j / 3));
									
									stepOnTexture = hitInfo.collider.gameObject.GetComponent<Renderer>().materials[i].mainTexture;
									break;
									
	                            }
	                        }
	                    }
					}
				}
				
			}
		}
		
		PlayFootStepAudio( stepOnTexture );
	}
	
	
	private void PlayFootStepAudio( Texture stepOnTexture )
	{
		// play the audio clip
		
		AudioClip stepSound = null;
		
		if ( stepOnTexture != null )
		{
			foreach ( Texture texture in GrassTextures )
			{
				if ( stepOnTexture == texture )
				{
					stepSound = GrassSound;
					break;
				}
			}
			
			if ( stepSound == null )
			{
				foreach ( Texture texture in TileTextures )
				{
					if ( stepOnTexture == texture )
					{
						stepSound = TileSound;
						break;
					}
				}
			}
			
			if ( stepSound == null )
			{
				foreach ( Texture texture in WoodTextures )
				{
					if ( stepOnTexture == texture )
					{
						stepSound = WoodSound;
						break;
					}
				}
			}
		}
		
		if ( stepSound == null )
			stepSound = DefaultSound;
		
		
		if ( stepSound != null )
		{
			GetComponent<AudioSource>().clip = stepSound;
			GetComponent<AudioSource>().pitch	= 1 + ( m_nextStep == WhichFoot.LEFT ? 0.2f + Random.Range(-0.1f, 0.1f) : 0 );
			
			CharacterController c = GetComponent<CharacterController>();
			GetComponent<AudioSource>().volume = c != null ? 1.0f * c.velocity.magnitude : 1.0f;
			GetComponent<AudioSource>().Play();
		}
		
	}
	
	
	private float[] GetTextureMix(Vector3 WorldPos)
	{
		// returns an array containing the relative mix of textures
		// on the main terrain at this world position.
		
		// The number of values in the array will equal the number
		// of textures added to the terrain.
		
		Terrain terrain = Terrain.activeTerrain;
		TerrainData terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;
		
		// calculate which splat map cell the worldPos falls within (ignoring y)
		int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		
		// get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
		float[,,] splatmapData = terrainData.GetAlphamaps( mapX, mapZ, 1, 1 );
		
		// extract the 3D array data to a 1D array:
		float[] cellMix = new float[ splatmapData.GetUpperBound(2) + 1 ];
		
		for(int n=0; n<cellMix.Length; n++)
		{
		 	cellMix[n] = splatmapData[ 0, 0, n ];
		}
		
		return cellMix;
    }
 
	
    private int GetMainTexture(Vector3 WorldPos)
	{
		// returns the zero-based index of the most dominant texture
		// on the main terrain at this world position.
		float[] mix = GetTextureMix(WorldPos);
		
		float maxMix = 0;
		int maxIndex = 0;
		
		// loop through each mix value and find the maximum
		for(int n=0; n<mix.Length; n++)
		{
			if ( mix[n] > maxMix )
			{
				maxIndex = n;
				maxMix = mix[n];
			}
		}
		
		return maxIndex;
    }

	
	void OnGUI()
	{
		//GUI.Label( new Rect( Screen.width / 2 - 100, 20, 200, 25 ), "FootStep: " + m_animator.GetFloat("FootStep") );
		
		//GUI.Label( new Rect( Screen.width / 2 - 100, 50, 200, 25 ), "TextureIndex: " + m_terrainTextureIndex );
	}
}
