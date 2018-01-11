using System;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;
using UnityEngine.UI;

public class CSGOperations : MonoBehaviour
{

	[Header("Objects for CSG")] private GameObject main_object;
	private GameObject operating_object;
	public GameObject[] stored_parts_prefab;
	private int current_step_num, max_step_num;
	private GameObject generated_object, composite;
	public Material[] materials;
	public List<String> CSG_operation_history;
	public Button[] bool_operation_buttons;
	public Image[] transformation_buttons;
	private TransfOp current_operation;


	enum TransfOp
	{
		Spectate,
		Move,
		Rotate,
		Scale
	};
	enum BoolOp
	{
		Union,
		Subtract,
		Intersect
	};


	void Start()
	{
		CSG_operation_history = new List<string>();
		current_step_num = 0;
		max_step_num = 0;
		current_operation = TransfOp.Spectate;

		enableDisableTransfButtons(false);
		enableDisableBoolButtons(false);
	}

	/// File handle operations
	public void LoadFile()
	{
		//ReconstructOperationHistory(GetComponent<FileHandler>().ReadFile());
//		CSG_operation_history = GetComponent<FileHandler>().ReadFile();
	}
	public void SaveFile()
	{
		//GetComponent<FileHandler>().WriteFile(CSG_operation_history);
	}

	/// Object handle operations 
	public void AddObject(int part)
	{
		GameObject new_obj = Instantiate(stored_parts_prefab[part], Vector3.zero, Quaternion.identity);
		new_obj.GetComponent<Renderer>().material = materials[material_ID];
		InitializeMainAndConstructObj(new_obj);
	}
	
	private void UpdateTransformationButtons()
	{
		// Reset all buttons
		foreach (Image button in transformation_buttons)
		{
			button.color = Color.white;
		}
		
		int button_index = 0;
		switch (current_operation)
		{
			case TransfOp.Spectate: return;
			case TransfOp.Move: button_index = 0; break;
			case TransfOp.Rotate: button_index = 1; break;
			case TransfOp.Scale: button_index = 2; break;
			default: return;
		}
		
		transformation_buttons[button_index].color = Color.cyan;
	}
	private void enableDisableTransfButtons(bool interactable)
	{
		foreach (Image button in transformation_buttons)
		{
			button.GetComponent<Button>().interactable = interactable;
		}
	}

	
	private void InitializeMainAndConstructObj(GameObject obj)
	{
		if (main_object == null)
		{
			main_object = obj;
			enableDisableTransfButtons(false);
			enableDisableBoolButtons(false);
		}		
		else
		{
			if (operating_object != null)
				Destroy(operating_object);
			operating_object = obj;
			
			enableDisableTransfButtons(true);
			enableDisableBoolButtons(true);
		}
	}
		
	
	/// Boolean operations
	public void Union()
	{
		GameObject comp = Boolean(BoolOp.Union);
		InitializeMainAndConstructObj(comp);
	}
	public void Subtraction()
	{
		GameObject comp = Boolean(BoolOp.Subtract);
		InitializeMainAndConstructObj(comp);
		
	}
	public void Intersection()
	{	
		
		GameObject comp = Boolean(BoolOp.Intersect);
		InitializeMainAndConstructObj(comp);
	}
	private GameObject Boolean(BoolOp operation)
	{
		Reset(main_object, operating_object);
		current_operation = TransfOp.Spectate;
		UpdateTransformationButtons();
		
		Mesh m;

		if(CSG_operation_history.Count == 0)
			StoreCurrentState("start", main_object);
		
		switch (operation)
		{
			case BoolOp.Union:
					StoreCurrentState("transformation", main_object);
					StoreCurrentState("union", operating_object);
					m = CSG.Union(main_object, operating_object);
				break;

			case BoolOp.Subtract:
					StoreCurrentState("transformation", main_object);
					StoreCurrentState("subtract", operating_object);
					m = CSG.Subtract(main_object, operating_object);
				break;

			case BoolOp.Intersect:
			default:
					StoreCurrentState("transformation", main_object);
					StoreCurrentState("intersect", operating_object);
					m = CSG.Intersect(main_object, operating_object);
				break;
		}

		composite = new GameObject();
		composite.name = "Constructed CSG";
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = materials[material_ID];

		// Generate new mesh
		Mesh mesh = ConstructMesh(composite);

		// Generate new meshcollider
		composite.AddComponent<MeshCollider>();	
		composite.GetComponent<MeshCollider>().sharedMesh = mesh;
		
		// Change tag
		composite.tag = "csg";
		
		Destroy(main_object);
		Destroy(operating_object);

		main_object = null;
		operating_object = null;
		
		
		// Switch new composited object to main object
		return composite;
	}
	private void Reset(GameObject A, GameObject B)
	{
		main_object = A;
		operating_object = B;

		ConstructMesh(main_object);
		ConstructMesh(operating_object);
	}
	private void enableDisableBoolButtons(bool interactable)
	{
		foreach (Button button in bool_operation_buttons)
		{
			button.interactable = interactable;
		}
	}


	/// Change objects material
	private int material_ID = 0;
	public void ChangeMaterial()
	{
		if (material_ID++ > materials.Length-2)
			material_ID = 0;
		
		if (main_object != null)
		{
			var mat = main_object.GetComponent<Renderer>().material;
			mat = materials[material_ID];
			main_object.GetComponent<Renderer>().material = mat;
		}
		if (operating_object != null)
		{
			var mat = operating_object.GetComponent<Renderer>().material;
			mat = materials[material_ID];
			operating_object.GetComponent<Renderer>().material = mat;
		}
	}
	
	///  Store CSG operation flow stepss
	private void StoreCurrentState(string operation, GameObject obj)
	{		
		// Split data in position, rotation and scale
		Vector3 position = obj.transform.position;
		Quaternion rotatation = obj.transform.rotation;
		Vector3 scale = obj.transform.localScale;

		string tag = obj.tag;

		if (operation == "transformation")
			tag = "csg";
		
		String pos = String.Format("{0}#{1}#{2}", position.x, position.y, position.z);
		String rot = String.Format("{0}#{1}#{2}#{3}", rotatation.x, rotatation.y, rotatation.z, rotatation.w);
		String sca = String.Format("{0}#{1}#{2}", scale.x, scale.y, scale.z);

		this.CSG_operation_history.Add(String.Format("{0},{1},{2},{3},{4}", tag, pos, rot, sca, operation));
		current_step_num++;
		max_step_num = this.CSG_operation_history.Count;
	}


	/// Rebuild mesh with individual triangles, adding barycentric coordinates
	Mesh ConstructMesh(GameObject go)
	{
		Mesh m = go.GetComponent<MeshFilter>().sharedMesh;

		if (m == null) return null;

		int[] tris = m.triangles;
		int triangleCount = tris.Length;

		Vector3[] mesh_vertices = m.vertices;
		Vector3[] mesh_normals = m.normals;
		Vector2[] mesh_uv = m.uv;

		Vector3[] vertices = new Vector3[triangleCount];
		Vector3[] normals = new Vector3[triangleCount];
		Vector2[] uv = new Vector2[triangleCount];
		Color[] colors = new Color[triangleCount];

		for (int i = 0; i < triangleCount; i++)
		{
			vertices[i] = mesh_vertices[tris[i]];
			normals[i] = mesh_normals[tris[i]];
			uv[i] = mesh_uv[tris[i]];

			colors[i] = i % 3 == 0 ? new Color(1, 0, 0, 0) :
				(i % 3) == 1 ? new Color(0, 1, 0, 0) : new Color(0, 0, 1, 0);

			tris[i] = i;
		}

		Mesh wireframeMesh = new Mesh();

		wireframeMesh.Clear();
		wireframeMesh.vertices = vertices;
		wireframeMesh.triangles = tris;
		wireframeMesh.normals = normals;
		wireframeMesh.colors = colors;
		wireframeMesh.uv = uv;

		go.GetComponent<MeshFilter>().sharedMesh = wireframeMesh;

		return wireframeMesh;
	}

	void ReconstructOperationHistory(List<string> operation_history)
	{
		CSG_operation_history = new List<string>();
		foreach (string step in operation_history)
		{
			string[] data = step.Split(',');
			
			// Object
			GameObject obj = null;

			int obj_index;

			if (int.TryParse(data[0], out obj_index))
			{
				obj = Instantiate(stored_parts_prefab[obj_index], Vector3.zero, Quaternion.identity);
			}
			
			// Position
			string[] pos_data = data[1].Split('#');
			Vector3 pos = new Vector3(float.Parse(pos_data[0]),float.Parse(pos_data[1]),float.Parse(pos_data[2]));
			
			// Rotation
			string[] rot_data = data[2].Split('#');
			Quaternion rot = new Quaternion(float.Parse(rot_data[0]),float.Parse(rot_data[1]),float.Parse(rot_data[2]), float.Parse(rot_data[3]));
			
			// Scale
			string[] sca_data = data[3].Split('#');
			Vector3 sca = new Vector3(float.Parse(sca_data[0]),float.Parse(sca_data[1]),float.Parse(sca_data[2]));

			// Apply transformation
			if (obj != null)
			{
				obj.transform.position = pos;
				obj.transform.rotation = rot;
				obj.transform.localScale = sca;
			}

			// Operation
			switch (data[4])
			{
					case "start":
							main_object = obj;
						break;
					case "transformation":
							main_object.transform.position = pos;
							main_object.transform.rotation = rot;
							main_object.transform.localScale = sca;
						break;
					case "union":
							operating_object = obj;
							Union();
						break;
					case "intersect":
							operating_object = obj;
							Intersection();
						break;
					case "subtract":
							operating_object = obj;
							Subtraction();
						break;		
			}
		}
	}
	void ReconstructStepByStepOperationHistory(string step)
	{
			string[] data = step.Split(',');
			
			// Object
			GameObject obj = null;
		
			int obj_index;
	
			if (int.TryParse(data[0], out obj_index))
			{
				obj = Instantiate(stored_parts_prefab[obj_index], Vector3.zero, Quaternion.identity);
			}
		
			// Position
			string[] pos_data = data[1].Split('#');
			Vector3 pos = new Vector3(float.Parse(pos_data[0]),float.Parse(pos_data[1]),float.Parse(pos_data[2]));
			
			// Rotation
			string[] rot_data = data[2].Split('#');
			Quaternion rot = new Quaternion(float.Parse(rot_data[0]),float.Parse(rot_data[1]),float.Parse(rot_data[2]), float.Parse(rot_data[3]));
			
			// Scale
			string[] sca_data = data[3].Split('#');
			Vector3 sca = new Vector3(float.Parse(sca_data[0]),float.Parse(sca_data[1]),float.Parse(sca_data[2]));

			// Apply transformation
			if (obj != null)
			{
				obj.transform.position = pos;
				obj.transform.rotation = rot;
				obj.transform.localScale = sca;
			}

			// Operation
			switch (data[4])
			{
					case "start":
							main_object = obj;
						break;
					case "transformation":
							main_object.transform.position = pos;
							main_object.transform.rotation = rot;
							main_object.transform.localScale = sca;
						break;
					case "union":
							operating_object = obj;
							Union();
						break;
					case "intersect":
							operating_object = obj;
							Intersection();
						break;
					case "subtract":
							operating_object = obj;
							Subtraction();
						break;		
			}
	}
}
