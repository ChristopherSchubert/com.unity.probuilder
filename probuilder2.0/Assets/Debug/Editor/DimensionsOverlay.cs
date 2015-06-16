﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;

public class DimensionsOverlay : ISceneEditor
{
	[MenuItem("Window/Hide Dimensions Overlay", true, 1)]
	public static bool HideVerify()
	{
		return DimensionsOverlay.instance != null;
	}

	[MenuItem("Window/Hide Dimensions Overlay", false, 1)]
	public static void Hide()
	{
		DimensionsOverlay.instance.Close();
	}

	[MenuItem("Window/Show Dimensions Overlay", true, 0)]
	public static bool InitVerify()
	{
		return DimensionsOverlay.instance == null;
	}


	[MenuItem("Window/Show Dimensions Overlay")]
	public static void Init()
	{
		ISceneEditor.Create<DimensionsOverlay>();
	}

	public override void OnInitialize()
	{
		mesh = new Mesh();
		material = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
		mesh.hideFlags = HideFlags.DontSave;
		material.hideFlags = HideFlags.DontSave;
	}

	public override void OnDestroy()
	{
		GameObject.DestroyImmediate(mesh);
		GameObject.DestroyImmediate(material);
	}

	public override void OnSceneGUI(SceneView scnview)
	{
		if( Selection.activeTransform != null && Selection.activeTransform.GetComponent<MeshFilter>() != null)
			RenderBounds(Selection.activeTransform.GetComponent<MeshFilter>());
	}

	Mesh mesh;
	Material material;
	// readonly Color wirecolor = new Color(.9f, .9f, .9f, .6f);
	readonly Color background = new Color(.3f, .3f, .3f, .6f);
	readonly Color LightWhite = new Color(.8f, .8f, .8f, .5f);

	void RenderBounds(MeshFilter mf)
	{
		if(!mesh) return;

		mf.sharedMesh.RecalculateBounds();

		// pb_Handle_Utility.BoundsWireframe(mf.sharedMesh.bounds, wirecolor, ref mesh);
		// material.SetPass(0);
		// Graphics.DrawMeshNow(mesh, mf.transform.localToWorldMatrix);

		// show labels
		Bounds wb = mf.transform.GetComponent<MeshRenderer>().bounds;

		DrawHeight(wb.center, wb.extents);
		DrawWidth(wb.center, wb.extents);
		DrawDepth(wb.center, wb.extents);
		
	}

	const float DISTANCE_LINE_OFFSET = .1f;
	Transform cam { get { return SceneView.lastActiveSceneView.camera.transform; } }

	void DrawHeight(Vector3 cen, Vector3 ext)
	{
		// positibilities
		Vector3[] edges = new Vector3[8]
		{
			// front left
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),

			// front right
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

			// back left
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),

			// back right
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
		};

		// figure leftmost height boundary
		Vector2 pos = Vector2.right * 20000f;
		Vector3 a = Vector3.zero, b = Vector3.zero;

		for(int i = 0; i < edges.Length; i += 2)
		{
			Vector2 screen = HandleUtility.WorldToGUIPoint( (edges[i] + edges[i+1]) * .5f );

			if( screen.x < pos.x )
			{
				pos = screen;
				a = edges[i+0];
				b = edges[i+1];
			}
		}

		Vector3 left = Vector3.Cross(cam.forward, Vector3.up).normalized * DISTANCE_LINE_OFFSET;

		Handles.color = LightWhite;
		Handles.DrawLine(a, a + left);
		Handles.DrawLine(b, b + left);

		a += left;
		b += left;
		
		Handles.color = Color.green;
		Handles.DrawLine(a, b);

		Handles.BeginGUI();
		gc.text = Vector3.Distance(a,b).ToString("F2");
		pos.x -= EditorStyles.label.CalcSize(gc).x * 2f;
		DrawSceneLabel(gc, pos);

		Handles.EndGUI();
	}

	void DrawDepth(Vector3 cen, Vector3 ext)
	{
		// positibilities
		Vector3[] edges = new Vector3[8]
		{
			// bottom right
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),

			// top right
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

			// bottom left
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),

			// top left
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
		};

		// figure leftmost height boundary
		Vector2 pos = Vector2.up * -20000f;
		Vector3 a = Vector3.zero, b = Vector3.zero;

		for(int i = 0; i < edges.Length; i += 2)
		{
			Vector2 screen = HandleUtility.WorldToGUIPoint( (edges[i] + edges[i+1]) * .5f );

			if( screen.y > pos.y )
			{
				pos = screen;
				a = edges[i+0];
				b = edges[i+1];
			}
		}

		float sign = -1f;//a.x - cen.x < 0f ? -1f : 1f;
		Vector3 offset = sign * cam.up * DISTANCE_LINE_OFFSET;

		Handles.color = LightWhite;
		Handles.DrawLine(a, a + offset);
		Handles.DrawLine(b, b + offset);

		a += offset;
		b += offset;
		Handles.color = Color.blue;
		Handles.DrawLine(a, b);

		Handles.BeginGUI();
		gc.text = Vector3.Distance(a,b).ToString("F2");
		// pos.x += EditorStyles.label.CalcSize(gc).x;
		pos.y += EditorStyles.label.CalcHeight(gc, 20000);
		DrawSceneLabel(gc, pos);

		Handles.EndGUI();
	}

	
	void DrawWidth(Vector3 cen, Vector3 extents)
	{
		Vector3 ext = extents;// + extents.normalized * .2f;

		// positibilities
		Vector3[] edges = new Vector3[8]
		{
			// bottom front
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),

			// bottom back
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
			
			// top front
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

			// top back
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
		};

		// figure leftmost height boundary
		Vector2 pos = Vector2.up * -20000f;
		Vector3 a = Vector3.zero, b = Vector3.zero;

		for(int i = 0; i < edges.Length; i += 2)
		{
			Vector2 screen = HandleUtility.WorldToGUIPoint( (edges[i] + edges[i+1]) * .5f );

			if( screen.y > pos.y )
			{
				pos = screen;
				a = edges[i+0];
				b = edges[i+1];
			}
		}

		Vector3 offset = -Vector3.up;
		offset = -Vector3.Cross(Vector3.Cross(cam.forward, Vector3.up), cam.forward).normalized * DISTANCE_LINE_OFFSET;

		Handles.color = LightWhite;
		Handles.DrawLine(a, a + offset);
		Handles.DrawLine(b, b + offset);

		a += offset;
		b += offset;

		Handles.color = Color.red;
		Handles.DrawLine(a, b);


		Handles.BeginGUI();
		DrawSceneLabel(Vector3.Distance(a,b).ToString("F2"), HandleUtility.WorldToGUIPoint((a + b) * .5f));
		Handles.EndGUI();
	}

	GUIContent gc = new GUIContent("", "");
	void DrawSceneLabel(string content, Vector2 position)
	{
		gc.text = content;
		DrawSceneLabel(gc, position);
	}

	void DrawSceneLabel(GUIContent content, Vector2 position)
	{
		float width = EditorStyles.label.CalcSize(content).x;
		float height = EditorStyles.label.CalcHeight(content, width) + 4;

		pb_GUI_Utility.DrawSolidColor( new Rect(position.x-1, position.y, width+2, height-2), background);
		GUI.Label( new Rect(position.x, position.y, width, height), content, EditorStyles.label );
	}
}
