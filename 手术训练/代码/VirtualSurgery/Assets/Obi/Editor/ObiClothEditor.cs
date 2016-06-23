using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

/**
 * Custom inspector for ObiCloth components.
 * Allows particle selection and constraint edition. 
 * 
 * Selection:
 * 
 * - To select a particle, left-click on it. 
 * - You can select multiple particles by holding shift while clicking.
 * - To deselect all particles, click anywhere on the object except a particle.
 * 
 * Constraints:
 * 
 * - To edit particle constraints, select the particles you wish to edit.
 * - Constraints affecting any of the selected particles will appear in the inspector.
 * - To add a new pin constraint to the selected particle(s), click on "Add Pin Constraint".
 * 
 */
[CustomEditor(typeof(ObiCloth)), CanEditMultipleObjects] 
public class ObiClothEditor : Editor
{

	[MenuItem("Component/Physics/Obi/Obi Cloth",false,0)]
    static void AddObiCloth()
    {
		foreach(Transform t in Selection.transforms)
			Undo.AddComponent<ObiCloth>(t.gameObject);
    }

	public enum EditionTool{
		SELECT,
		SELECTBRUSH,
		PAINT
	}

	public enum PaintMode{
		PAINT,
		SMOOTH
	}

	public enum ParticleProperty{
		MASS,
		SKIN_RADIUS,
		SKIN_BACKSTOP
	}

	ObiCloth cloth;
	EditorCoroutine routine;

	EditionTool tool = EditionTool.SELECT;
	PaintMode paintMode = PaintMode.PAINT;
	ParticleProperty currentProperty = ParticleProperty.MASS;

	Gradient valueGradient = new Gradient();

	float vertexSize = 0.02f;
	bool previewVirtualParticles = false;
	bool previewSpatialGrid = false;
	bool previewSkin = false;
	bool previewTethers = false;
	bool backfaces = false;
	bool constraintsFolded = true;
	Rect uirect;

	//Mass edition related:
	float selectionMass = 0;
	float newMass = 0;

	float maxValue = Single.MinValue;
	float minValue = Single.MaxValue;

	//Brush related:
	float brushRadius = 50;
	float brushOpacity = 0.01f;
	float minBrushValue = 0;
	float maxBrushValue = 10;
	bool selectionMask = false;

	//Selection related:
	int selectedCount = 0;

	//Editor playback related:
	bool isPlaying = false;
	float lastFrameTime = 0.0f;
	float accumulatedTime = 0.0f;

	Vector3 camup;
	Vector3 camright;

	//Additional GUI styles:
	GUIStyle separatorLine;

	//Additional status info for all particles:
	bool[] selectionStatus = new bool[0];
	bool[] facingCamera = new bool[0];
	Vector3[] wsPositions = new Vector3[0];
	
	public void OnEnable(){

		cloth = (ObiCloth)target;

		cloth.ResizeArrays();

		SetupValuesGradient();

		separatorLine = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).box);
		separatorLine.normal.background = EditorGUIUtility.Load("SeparatorLine.psd") as Texture2D;
		separatorLine.border = new RectOffset(3,3,0,0);
		separatorLine.fixedHeight = 3;
		separatorLine.stretchWidth = true;

		// In case the cloth has not been initialized yet, start the initialization routine.
		if (!cloth.Initialized && !cloth.Initializing){
			CoroutineJob job = new CoroutineJob();
			routine = EditorCoroutine.StartCoroutine(job.Start(cloth.GeneratePhysicRepresentationForMesh()));
		}

		EditorApplication.update += Update;
		EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
	}

	public void OnDisable(){
		EditorApplication.update -= Update;
		EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
		EditorUtility.ClearProgressBar();
	}

	private void SetupValuesGradient(){

		GradientColorKey[] gck = new GradientColorKey[2];
		gck[0].color = Color.blue;
		gck[0].time = 0.0f;
		gck[1].color = new Color(1,0.7f,0,1);
		gck[1].time = 1.0f;

		GradientAlphaKey[] gak = new GradientAlphaKey[2];
		gak[0].alpha = 1.0f;
		gak[0].time = 0.0f;
		gak[1].alpha = 1.0f;
		gak[1].time = 1.0f;

		valueGradient.SetKeys(gck,gak);
	}
	
	private void ResizeParticleArrays(){

		if (cloth.particles != null){

			// Reinitialize particle property min/max values if needed:
			if (selectionStatus.Length != cloth.particles.Count){
				ParticlePropertyChanged();
			}

			Array.Resize(ref selectionStatus,cloth.particles.Count);
			Array.Resize(ref facingCamera,cloth.particles.Count);
			Array.Resize(ref wsPositions,cloth.particles.Count);

		}

	}
	
	public override void OnInspectorGUI() {
		
		serializedObject.Update();

		ResizeParticleArrays();

		Editor.DrawPropertiesExcluding(serializedObject,"m_Script","pressureConstraintsGroup","skinConstraintsGroup");

		// Inform about current skin constraint availability, and show it if available.
		if (cloth.IsSkinned)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("skinConstraintsGroup"),true);
		else
			EditorGUILayout.HelpBox("Mesh is not skinned, so no skin constraints available.",MessageType.Info);

		// Inform about current pressure constraint availability, and show it if available.
		if (cloth.edgeStructure.IsClosed)
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pressureConstraintsGroup"),true);
		else
			EditorGUILayout.HelpBox("Mesh is not closed, so no pressure constraint available.",MessageType.Info);

		// Draw thethers ui:
		DrawTetherConstraintsUI();

		// Draw rigidbody pin constraints.
		DrawPinConstraintsUI();

		// Progress bar:
		EditorCoroutine.ShowCoroutineProgressBar("Obi is thinking...",routine);
		
		// Apply changes to the serializedProperty
		if (GUI.changed)
			serializedObject.ApplyModifiedProperties();

	}

	private void DrawTetherConstraintsUI(){
		
		// Draw tether constraints:
		constraintsFolded =	EditorGUILayout.Foldout(constraintsFolded,"Tether Constraints");
		if (constraintsFolded){
			
				int[] containsTethers = new int[selectionStatus.Length];
				
				int selectedConstraintCount = 0;
				foreach(DistanceConstraint tether in cloth.tethers){
					containsTethers[tether.p1]++;
					containsTethers[tether.p2]++;
					if (selectionStatus[tether.p1]) selectedConstraintCount++;
					if (selectionStatus[tether.p2]) selectedConstraintCount++;
				}

				if (selectedConstraintCount > 0){
	
					GUILayout.BeginHorizontal("box");
					
					GUILayout.Label("Tethers: "+selectedConstraintCount);

					Color oldColor = GUI.color;
					GUI.color = Color.red;

					if (GUILayout.Button("X",GUILayout.Width(30))){
						for(int i = 0; i < selectionStatus.Length; i++){
							if (selectionStatus[i] && containsTethers[i] > 0){
								cloth.tethers.RemoveWhere(x => x.p1 == i || x.p2 == i);
								continue;
							}
						}
					}

					GUI.color = oldColor;
					
					GUILayout.EndHorizontal();

				}else{
					EditorGUILayout.HelpBox("No constraints for selected particles.",MessageType.Info);
				}

			// Add new constraint button:
			if (GUILayout.Button("Add Tether Constraints")){
				for(int i = 0; i < selectionStatus.Length; i++){
					if (selectionStatus[i] && containsTethers[i] == 0){
						// Add tethers between this fixed particle and all free particles:
						if (cloth.particles[i].w == 0){
							foreach(ObiClothParticle p in cloth.particles){
								if (p.w > 0){
									float distance = (p.position - cloth.particles[i].position).FLength();
									cloth.tethers.Add(new DistanceConstraint(cloth.transform,i,p.index,distance,1,0,0));
								}	
							}	
						}
						// Add tethers between this particle and all fixed particles:
						else{
							foreach(ObiClothParticle p in cloth.particles){
								if (p.w == 0){
									float distance = (p.position - cloth.particles[i].position).FLength();
									cloth.tethers.Add(new DistanceConstraint(cloth.transform,i,p.index,distance,1,0,0));
								}
							}
						}
						
					}
				}
				EditorUtility.SetDirty(cloth);
			}
		}

	}

	private void DrawPinConstraintsUI(){

		PinConstraint removedConstraint = null;
		
		// Draw pin constraints:
		constraintsFolded =	EditorGUILayout.Foldout(constraintsFolded,"Pin Constraints");
		if (constraintsFolded){

			List<PinConstraint> selectedConstraints = new List<PinConstraint>();

			foreach(PinConstraint pin in cloth.pins){
				if (selectionStatus[pin.pIndex]){
					selectedConstraints.Add(pin);
				}
			}

			if (selectedConstraints.Count > 0){
			
				foreach(PinConstraint c in selectedConstraints){
					
					GUILayout.BeginVertical("box");

						GUILayout.BeginHorizontal();
							
							EditorGUI.BeginChangeCheck();
							bool allowSceneObjects = !EditorUtility.IsPersistent(target);
							c.rigidbody = EditorGUILayout.ObjectField("Pinned to:",c.rigidbody,typeof(Rigidbody),allowSceneObjects) as Rigidbody;
							
							// initialize offset after changing the rigidbody.
							if (EditorGUI.EndChangeCheck() && c.rigidbody != null){
								c.offset = c.rigidbody.transform.InverseTransformVector(cloth.transform.TransformPoint(cloth.particles[c.pIndex].position) - c.rigidbody.transform.position);
							}

							Color oldColor = GUI.color;
							GUI.color = Color.red;
							if (GUILayout.Button("X",GUILayout.Width(30))){
								removedConstraint = c;
								continue;
							}
							GUI.color = oldColor;
						
						GUILayout.EndHorizontal();

						c.offset = EditorGUILayout.Vector3Field("Offset:",c.offset);

					GUILayout.EndVertical();
					
				}
				
				if (removedConstraint != null){
					cloth.pins.RemoveWhere(x => x == removedConstraint);
					EditorUtility.SetDirty(cloth);
				}
			}else{
				EditorGUILayout.HelpBox("No constraints for selected particles.",MessageType.Info);
			}
			
			// Add new constraint button:
			if (GUILayout.Button("Add Pin Constraint")){
				for(int i = 0; i < selectionStatus.Length; i++){
					if (selectionStatus[i]){
						PinConstraint pin = new PinConstraint(cloth.transform,i,null,Vector3.zero);
						cloth.pins.Add(pin);
					}
				}
				EditorUtility.SetDirty(cloth);
			}
		}
	}

	public static Material particleMaterial;
	static void CreateParticleMaterial() {
		if (!particleMaterial) { 
			particleMaterial = EditorGUIUtility.LoadRequired("Particles.mat") as Material;
			particleMaterial.hideFlags = HideFlags.HideAndDontSave;
			particleMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void OnSceneGUI(){

		CreateParticleMaterial();
		particleMaterial.SetPass(0);

		ResizeParticleArrays();

		if (cloth.mesh == null || cloth.particles == null) return;

		// get mesh vertices and normals:
		Vector3[] vertices = cloth.mesh.vertices;
		Vector3[] normals = cloth.mesh.normals;

		if (Camera.current != null){
				
			camup = Camera.current.transform.up * vertexSize;
			camright = Camera.current.transform.right * vertexSize;

		}


		if (Event.current.type == EventType.Repaint){

			// Update camera facing status and world space positions array:
			for(int i = 0; i < cloth.particles.Count; i++)
			{
				wsPositions[i] = cloth.transform.TransformPoint(cloth.particles[i].position);		
				facingCamera[i] = backfaces ? true : IsClothParticleFacingCamera(cloth.transform,cloth.RootBone,Camera.current,normals,i,wsPositions[i]);
			}

			// Draw 3D stuff: particles, constraints, grid, etc.
			DrawParticles();
	
			if (previewVirtualParticles && cloth.collisionConstraintsGroup.virtualParticleCoordinates != null)
				DrawVirtualParticles(vertices);

			if (previewSpatialGrid)
				DrawGrid();
				  	
			if (previewTethers)
				DrawTetherConstraints();
			DrawPinConstraints();			

			if (previewSkin)	
				DrawSkinPreview();

		}

		// Draw tool handles:
		if (Camera.current != null){
			
			switch(tool){
			case EditionTool.SELECT: 
				if (ObiClothParticleHandles.ParticleSelector(wsPositions,selectionStatus,facingCamera)){
					SelectionChanged();
				}
				break;
			case EditionTool.SELECTBRUSH: 
				if (ObiClothParticleHandles.ParticleBrush(wsPositions,facingCamera,brushRadius,
				                                          (List<ParticleStampInfo> stampInfo,bool modified)=>{
																foreach(ParticleStampInfo info in stampInfo)
																	selectionStatus[info.index] = !modified;
														  },
														  EditorGUIUtility.Load("BrushHandle.psd") as Texture2D)){
					SelectionChanged();
				}
				break;
			case EditionTool.PAINT: //TODO: select mask (paint on selected)
				if (ObiClothParticleHandles.ParticleBrush(wsPositions,facingCamera,brushRadius,
					                                  PaintbrushStampCallback,
													  EditorGUIUtility.Load("BrushHandle.psd") as Texture2D)){
					ParticlePropertyChanged();
				}
				break;
			}
		}

		// Sceneview GUI:
		GUI.changed = false;

		Handles.BeginGUI();

			GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

			if (Event.current.type == EventType.Repaint){
				uirect = GUILayout.Window(0,uirect,DrawUIWindow,"Obi Cloth");
				uirect.x = Screen.width - uirect.width - 10; //10 and 28 are magic values, since Screen size is not exactly right.
				uirect.y = Screen.height - uirect.height - 28;
			}
			GUILayout.Window(0,uirect,DrawUIWindow,"Obi Cloth");

		Handles.EndGUI();

		if (GUI.changed){
			EditorUtility.SetDirty(cloth);
		}
		
	}

	private void ForceWindowRelayout(){
		uirect.Set(0,0,0,0);
	}

	private void DrawParticles(){

		//Draw all cloth vertices:
		GL.Begin(GL.TRIANGLES);
		for(int i = 0; i < cloth.particles.Count; i++)
		{
			// skip particles not facing the camera:
			if (!facingCamera[i]) continue;
			
			// get particle size in screen space:
			float size = HandleUtility.GetHandleSize(wsPositions[i])*2;

			// get particle color:
			Color color;
			if (cloth.particles[i].w == 0){
				color = Color.red;
			}else{
				if (cloth.particles[i].asleep)
					color = Color.gray;
				else
					color = GetPropertyValueGradient(GetPropertyValue(i));
			}
			
			// highlight the particle if its selected:
			if (selectionStatus[i]){
				GL.Color(color);
				DrawParticle(wsPositions[i],camup*2*size,camright*2*size);
				GL.Color(Color.white);
				DrawParticle(wsPositions[i],camup*1.5f*size,camright*1.5f*size);
			}
			
			// draw the regular particle:
			GL.Color(color);
			DrawParticle(wsPositions[i],camup*size,camright*size);
		}
		GL.End();
		
	}

	private void DrawVirtualParticles(Vector3[] vertices){
		//preview virtual particles:
		GL.Begin(GL.TRIANGLES);	
		GL.Color(Color.yellow);
		for(int i = 0; i < selectionStatus.Length; i++){
			if (selectionStatus[i])
			foreach(HalfEdge.HEFace face in cloth.edgeStructure.GetNeighbourFacesEnumerator(cloth.edgeStructure.heVertices[i])){
				
				Vector3 v1 = vertices[cloth.edgeStructure.heVertices[cloth.edgeStructure.heEdges[face.edges[0]].startVertex].physicalIDs[0]];
				Vector3 v2 = vertices[cloth.edgeStructure.heVertices[cloth.edgeStructure.heEdges[face.edges[1]].startVertex].physicalIDs[0]];
				Vector3 v3 = vertices[cloth.edgeStructure.heVertices[cloth.edgeStructure.heEdges[face.edges[2]].startVertex].physicalIDs[0]];
				
				foreach(Vector3 vpCoord in cloth.collisionConstraintsGroup.virtualParticleCoordinates){
					
					Vector3 virtualPosition = cloth.transform.TransformPoint(ObiUtils.BarycentricInterpolation(v1,v2,v3,vpCoord));
					float size = HandleUtility.GetHandleSize(virtualPosition);
					
					DrawParticle(virtualPosition,camup*size,camright*size);
				}
				
			}
		}
		GL.End();
	}

	private void DrawTetherConstraints(){
		if (cloth.tethers != null){
			List<DistanceConstraint> selectedConstraints = cloth.tethers.FindAll(x => selectionStatus[x.p1] || selectionStatus[x.p2]);
			Handles.color = Color.yellow;
			Handles.matrix = cloth.transform.localToWorldMatrix;
			foreach(DistanceConstraint c in selectedConstraints){
				Handles.DrawDottedLine(cloth.particles[c.p1].position,cloth.particles[c.p2].position,5);
			}
		}
	}

	private void DrawPinConstraints(){
		if (cloth.pins != null){
			List<PinConstraint> selectedConstraints = cloth.pins.FindAll(x => selectionStatus[x.pIndex]);
			Handles.color = Color.red;
			foreach(PinConstraint c in selectedConstraints){
				if (c.rigidbody != null){
					
					Vector3 pinPosition = c.rigidbody.transform.TransformPoint(c.offset);
					
					Handles.DrawDottedLine(wsPositions[c.pIndex],
					                       c.rigidbody.transform.TransformPoint(c.offset),5);
					
					Handles.SphereCap(0,pinPosition,Quaternion.identity,HandleUtility.GetHandleSize(pinPosition)*0.1f);
					
				}
			}
		}
	}

	private void DrawSkinPreview(){

		Color main = Color.red;
		Color secondary = new Color(1,0,0,0.1f);

		Handles.matrix = cloth.transform.localToWorldMatrix;
		for(int i = 0; i < selectionStatus.Length; i++){

			if (selectionStatus[i]){

				SkinConstraint sc = cloth.skinConstraints[i];
				float radius = cloth.skinConstraintsGroup.pp_radius[i];
				float backstop = cloth.skinConstraintsGroup.pp_backstop[i];

				if (radius > 0){
					float discRadius = Mathf.Sqrt(1 - Mathf.Pow(backstop / radius,2)) * radius;
					Handles.color = main;
					Handles.DrawWireDisc(sc.point + sc.normal * backstop,sc.normal,discRadius);
					Handles.DrawLine(sc.point,sc.point + sc.normal * backstop);	
					Handles.color = secondary;
					Handles.DrawSolidDisc(sc.point + sc.normal * backstop,sc.normal,discRadius);	
				}

			}

		}

	}

	/**
	 * Return whether all physical vertices at the particle should are culled or not. If at least one is not culled, the particle is visible.
	 */
	private bool IsClothParticleFacingCamera(Transform clothTransform, Transform skinTransform, Camera cam, Vector3[] meshNormals, int particleIndex, Vector3 particleWorldPosition){

		if (cam == null || clothTransform == null) return false;

		if (particleIndex < cloth.edgeStructure.heVertices.Count){

			HalfEdge.HEVertex vertex = cloth.edgeStructure.heVertices[particleIndex];

			if (skinTransform == null){
				foreach(int index in vertex.physicalIDs){
					if (Vector3.Dot(clothTransform.TransformVector(meshNormals[index]),cam.transform.position - particleWorldPosition) > 0)
						return true;
				}
			}else{		
				foreach(int index in vertex.physicalIDs){
					if (Vector3.Dot(clothTransform.InverseTransformVector(skinTransform.TransformVector(meshNormals[index])),cam.transform.position - particleWorldPosition) > 0)
						return true;
				}
			}

		}

		return false;

	}

	private void SelectionChanged(){

		// Find out how many selected particles we have:
		selectedCount = 0;
		for(int i = 0; i < selectionStatus.Length; i++){
			if (selectionStatus[i]) selectedCount++;
		}

		// Set the initial mass value:
		for(int i = 0; i < selectionStatus.Length; i++){
			if (selectionStatus[i]){
				newMass = selectionMass = cloth.particles[i].mass; 
				break;
			}
		}	

		Repaint();	

	}

	/**
	 * Called when the currenty edited property of any particle as changed.
	 */
	private void ParticlePropertyChanged(){
		
		maxValue = Single.MinValue;
		minValue = Single.MaxValue;

		for(int i = 0; i < cloth.particles.Count; i++){

			//Skip fixed particles:
			if (cloth.particles[i].w == 0) continue;

			float value = GetPropertyValue(i); 
			maxValue = Mathf.Max(maxValue,value);
			minValue = Mathf.Min(minValue,value);

		}		
		
	}

	private void SetPropertyValue(int index, float value){

		switch(currentProperty){
			case ParticleProperty.MASS: 
				cloth.particles[index].mass = value;
			break; 
			case ParticleProperty.SKIN_BACKSTOP: 
				cloth.skinConstraintsGroup.pp_backstop[index] = value; 
			break;
			case ParticleProperty.SKIN_RADIUS: 
				cloth.skinConstraintsGroup.pp_radius[index] = value; 
			break;
		}

	}

	private float GetPropertyValue(int index){
		switch(currentProperty){
			case ParticleProperty.MASS: 
				return cloth.particles[index].mass;
			case ParticleProperty.SKIN_BACKSTOP: 
				return cloth.skinConstraintsGroup.pp_backstop[index];
			case ParticleProperty.SKIN_RADIUS: 
				return cloth.skinConstraintsGroup.pp_radius[index];
		}
		return 0;
	}

	private Color GetPropertyValueGradient(float value){
		return valueGradient.Evaluate(Mathf.InverseLerp(minValue,maxValue,value));
	}

	private string GetPropertyName(){
		switch(currentProperty){
			case ParticleProperty.MASS: return "mass";
			case ParticleProperty.SKIN_RADIUS: return "radius";
			case ParticleProperty.SKIN_BACKSTOP: return "backstop";
		}
		return "";
	}

	/**
	 * Callback called for each paintbrush stamp (each time the user drags the mouse, and when he first clicks down).
	 */ 
	private void PaintbrushStampCallback(List<ParticleStampInfo> stampInfo, bool modified){

		// Average and particle count for SMOOTH mode.
		float averageValue = 0;	
		int numParticles = 0;

		foreach(ParticleStampInfo info in stampInfo){

			// Dont do anything with fixed particles, just skip them.
			if (cloth.particles[info.index].w == 0) continue;

			// Also skip unselected particles, if selection mask is on.
			if (selectionMask && !selectionStatus[info.index]) continue;

			switch(paintMode){
				case PaintMode.PAINT: 
					float currentValue = GetPropertyValue(info.index);
					if (modified){
						SetPropertyValue(info.index,Mathf.Max(currentValue - (this.brushRadius - Mathf.Sqrt(info.sqrDistanceToMouse)) * brushOpacity,minBrushValue));
					}else{
						SetPropertyValue(info.index,Mathf.Min(currentValue + (this.brushRadius - Mathf.Sqrt(info.sqrDistanceToMouse)) * brushOpacity,maxBrushValue));
					}
				break;
				case PaintMode.SMOOTH:
					averageValue += GetPropertyValue(info.index);
					numParticles++;
				break;
			}

		}

		if (paintMode == PaintMode.SMOOTH){
			averageValue /= numParticles;
			foreach(ParticleStampInfo info in stampInfo){

				// Again, dont do anything with fixed particles, just skip them.
				if (cloth.particles[info.index].w == 0) continue;
				
				// Also skip unselected particles, if selection mask is on.
				if (selectionMask && !selectionStatus[info.index]) continue;

				float currentValue = GetPropertyValue(info.index);
				if (modified){ //Sharpen
					SetPropertyValue(info.index,Mathf.Clamp(currentValue + (this.brushRadius - Mathf.Sqrt(info.sqrDistanceToMouse)) * (currentValue - averageValue) * brushOpacity,minBrushValue,maxBrushValue));
				}else{	//Smooth
					SetPropertyValue(info.index,currentValue - (this.brushRadius - Mathf.Sqrt(info.sqrDistanceToMouse)) * (currentValue - averageValue) * brushOpacity);
				}
			}
		}

	}

	/**
	 * Draws a window with cloth tools:
	 */
	void DrawUIWindow(int windowID) {
	
		//-------------------------------
		// Visualization options
		//-------------------------------
		GUILayout.BeginHorizontal();
		previewVirtualParticles = GUILayout.Toggle(previewVirtualParticles,"virtual particles");
		previewSpatialGrid = GUILayout.Toggle(previewSpatialGrid,"grid");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		backfaces = GUILayout.Toggle(backfaces,"backfaces");
		previewTethers = GUILayout.Toggle(previewTethers,"tethers");
		GUILayout.EndHorizontal();

		if (cloth.IsSkinned){
			GUILayout.BeginHorizontal();
				previewSkin = GUILayout.Toggle(previewSkin,"skin");
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Box("",separatorLine);

		//-------------------------------
		// Tools
		//-------------------------------
		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(tool == EditionTool.SELECT,"Select",GUI.skin.FindStyle("ButtonLeft")) && tool != EditionTool.SELECT){
			tool = EditionTool.SELECT;
			ForceWindowRelayout();
		}
		if (GUILayout.Toggle(tool == EditionTool.SELECTBRUSH,"Brush",GUI.skin.FindStyle("ButtonMid")) && tool != EditionTool.SELECTBRUSH){
			tool = EditionTool.SELECTBRUSH;
			ForceWindowRelayout();
		}
		if (GUILayout.Toggle(tool == EditionTool.PAINT,"Paint",GUI.skin.FindStyle("ButtonRight")) && tool != EditionTool.PAINT){
			tool = EditionTool.PAINT;
			ForceWindowRelayout();
		}
		GUILayout.EndHorizontal();

		currentProperty = (ParticleProperty) EditorGUILayout.EnumPopup(currentProperty,GUI.skin.FindStyle("DropDown"));

		switch(tool){
			case EditionTool.SELECT:
				DrawSelectionToolUI();
			break;
			case EditionTool.SELECTBRUSH:
				GUILayout.BeginHorizontal();
					GUILayout.Label("Radius");
					brushRadius = EditorGUILayout.Slider(brushRadius,5,200);
				GUILayout.EndHorizontal();
				DrawSelectionToolUI();
			break;
			case EditionTool.PAINT:
				DrawPaintToolUI();
			break;
		}
		
		//-------------------------------
		// Cloth tools
		//-------------------------------
		GUILayout.Box("",separatorLine);

		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Reset Cloth",GUILayout.Width(88))){
			accumulatedTime = 0;
			CoroutineJob job = new CoroutineJob();
			job.asyncThreshold = 1000;
			routine = EditorCoroutine.StartCoroutine(job.Start(cloth.ResetAll()));
		}
		
		if (GUILayout.Button("Optimize",GUILayout.Width(88))){
			if (EditorUtility.DisplayDialog("Cloth optimization","About to remove fixed particles that do not contribute to the simulation. The only way to undo this is reset or recreate the cloth. Do you want to continue?","Ok","Cancel"))
				cloth.Optimize();
		}
		
		GUILayout.EndHorizontal();
	
		//-------------------------------
		//Playback functions
		//-------------------------------
		GUILayout.BeginHorizontal();

		GUI.enabled = !EditorApplication.isPlaying;

			if (GUILayout.Button(EditorGUIUtility.Load("RewindButton.psd") as Texture2D,GUILayout.MaxHeight(24),GUILayout.Width(42))){
				cloth.ResetGeometry();
				accumulatedTime = 0;
			}

			if (GUILayout.Button(EditorGUIUtility.Load("StopButton.psd") as Texture2D,GUILayout.MaxHeight(24),GUILayout.Width(42))){
				isPlaying = false;
			}

			if (GUILayout.Button(EditorGUIUtility.Load("PlayButton.psd") as Texture2D,GUILayout.MaxHeight(24),GUILayout.Width(42))){
				lastFrameTime = Time.realtimeSinceStartup;
				isPlaying = true;
			}

			if (GUILayout.Button(EditorGUIUtility.Load("StepButton.psd") as Texture2D,GUILayout.MaxHeight(24),GUILayout.Width(42))){
				isPlaying = false;
				cloth.SimulateStep(Time.fixedDeltaTime);
				cloth.CommitResultsToMesh();
			}

		GUI.enabled = true;

		GUILayout.EndHorizontal();

	}

	void DrawSelectionToolUI(){

		GUILayout.Label(selectedCount+" particle(s) selected");
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Invert",GUILayout.Width(88))){
			for(int i = 0; i < selectionStatus.Length; i++)
				selectionStatus[i] = !selectionStatus[i];
			SelectionChanged();
		}
		GUI.enabled = selectedCount > 0;
		if (GUILayout.Button("Clear",GUILayout.Width(88))){
			for(int i = 0; i < selectionStatus.Length; i++)
				selectionStatus[i] = false;
			SelectionChanged();
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Select fixed",GUILayout.Width(88))){
			for(int i = 0; i < cloth.particles.Count; i++){
				if (cloth.particles[i].w == 0)
					selectionStatus[i] = true;
			}
			SelectionChanged();
		}
		GUI.enabled = selectedCount > 0;
		if (GUILayout.Button("Unselect fixed",GUILayout.Width(88))){
			for(int i = 0; i < cloth.particles.Count; i++){
				if (cloth.particles[i].w == 0)
					selectionStatus[i] = false;
			}
			SelectionChanged();
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		
		GUI.enabled = selectedCount > 0;		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("Fix",EditorGUIUtility.Load("PinIcon.psd") as Texture2D),GUILayout.MaxHeight(18),GUILayout.Width(88))){
			for(int i = 0; i < selectionStatus.Length; i++){
				if (selectionStatus[i]){
					ObiClothParticle particle = cloth.particles[i];
					if (particle.w != 0){	
						newMass = particle.mass = float.PositiveInfinity;
						particle.velocity = Vector3.zero;
					}
				}
			}
		}
		if (GUILayout.Button(new GUIContent("Unfix",EditorGUIUtility.Load("UnpinIcon.psd") as Texture2D),GUILayout.MaxHeight(18),GUILayout.Width(88))){
			for(int i = 0; i < selectionStatus.Length; i++){
				if (selectionStatus[i]){
					ObiClothParticle particle = cloth.particles[i];
					if (particle.w == 0){	
						particle.mass = 1;
					}
				}
			}
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();		
		
		EditorGUI.showMixedValue = false;
		for(int i = 0; i < selectionStatus.Length; i++){
			if (selectionStatus[i] && cloth.particles[i].mass != selectionMass){
				EditorGUI.showMixedValue = true;
			}	
		}
		
		newMass = EditorGUILayout.FloatField(newMass,GUILayout.Width(88));
		EditorGUI.showMixedValue = false;
		
		if (GUILayout.Button("Set "+GetPropertyName(),GUILayout.Width(88))){
			selectionMass = newMass;
			for(int i = 0; i < selectionStatus.Length; i++){
				if (selectionStatus[i]){
					SetPropertyValue(i,selectionMass);
				}
			}
			ParticlePropertyChanged();
		}
		
		GUILayout.EndHorizontal();
		GUI.enabled = true;
	}

	void DrawPaintToolUI(){

		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(paintMode == PaintMode.PAINT,EditorGUIUtility.Load("Paint_brush_icon.psd") as Texture2D,GUI.skin.FindStyle("ButtonLeft"),GUILayout.MaxHeight(28)))
			paintMode = PaintMode.PAINT;
		if (GUILayout.Toggle(paintMode == PaintMode.SMOOTH,EditorGUIUtility.Load("Smooth_brush_icon.psd") as Texture2D,GUI.skin.FindStyle("ButtonRight"),GUILayout.MaxHeight(28)))
			paintMode = PaintMode.SMOOTH;
		GUILayout.EndHorizontal();

		selectionMask = GUILayout.Toggle(selectionMask,"Selection mask");

		GUILayout.BeginHorizontal();
			GUILayout.Label("Radius");
			brushRadius = EditorGUILayout.Slider(brushRadius,5,200);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
			GUILayout.Label("Opacity");
			brushOpacity = EditorGUILayout.Slider(brushOpacity*20,0,1)/20f;
		GUILayout.EndHorizontal();

		GUI.enabled = paintMode == PaintMode.PAINT;
			GUILayout.BeginHorizontal();
				GUILayout.Label("Min value");
				GUILayout.FlexibleSpace();
				minBrushValue = EditorGUILayout.FloatField(minBrushValue,GUILayout.Width(EditorGUIUtility.fieldWidth));
			GUILayout.EndHorizontal();
	
			GUILayout.BeginHorizontal();
				GUILayout.Label("Max value");
				GUILayout.FlexibleSpace();
				maxBrushValue = EditorGUILayout.FloatField(maxBrushValue,GUILayout.Width(EditorGUIUtility.fieldWidth));
			GUILayout.EndHorizontal();
		GUI.enabled = true;

	}

	void OnPlayModeStateChanged()
	{
		//Prevent the user from going into play mode while we are doing stuff:
		if (routine != null && !routine.IsDone && EditorApplication.isPlayingOrWillChangePlaymode)
		{
			EditorApplication.isPlaying = false;
		}
	}

	void Update () {
		if (isPlaying){

			accumulatedTime += Mathf.Min(Time.realtimeSinceStartup - lastFrameTime, Time.maximumDeltaTime);

			while (accumulatedTime >= Time.fixedDeltaTime){

				foreach (ObiClothParticle p in cloth.particles)
					cloth.pp_previousPosition[p.index] = p.position;
			
				cloth.SimulateStep(Time.fixedDeltaTime);
				accumulatedTime -= Time.fixedDeltaTime;
			}		

			float alpha = accumulatedTime / Time.fixedDeltaTime;
			foreach (ObiClothParticle p in cloth.particles)
				cloth.pp_drawPosition[p.index] = Vector3.Lerp(cloth.pp_previousPosition[p.index],p.position,alpha);

			cloth.CommitResultsToMesh();

			lastFrameTime = Time.realtimeSinceStartup;
		}
	}

	private void DrawParticle(Vector3 p, Vector3 up, Vector3 r){

		GL.Vertex(p+up);
		GL.Vertex(p-r);
		GL.Vertex(p+r);

		GL.Vertex(p-up);
		GL.Vertex(p+r);
		GL.Vertex(p-r);

	}

	private void DrawGrid(){
		GL.Begin(GL.LINES);
		GL.Color(new Color(0,1,0,0.25f));
		if (cloth.grid != null){
			//Draw adaptive grid:
			Gizmos.color = new Color(1,1,0,0.25f);
			foreach (KeyValuePair<int,AdaptiveGrid.Cell> pair in cloth.grid.cells){
				DrawGridCell(pair.Value.Index*cloth.grid.CellSize + Vector3.one*cloth.grid.CellSize*0.5f,Vector3.one*cloth.grid.CellSize*0.5f);
			}
		}
		GL.End();
	}

	private void DrawGridCell(Vector3 center, Vector3 size){
		
		//Bottom face:
		GL.Vertex(center + new Vector3(-size.x,-size.y,-size.z));
		GL.Vertex(center + new Vector3(size.x,-size.y,-size.z));

		GL.Vertex(center + new Vector3(size.x,-size.y,-size.z));
		GL.Vertex(center + new Vector3(size.x,-size.y,size.z));

		GL.Vertex(center + new Vector3(size.x,-size.y,size.z));
		GL.Vertex(center + new Vector3(-size.x,-size.y,size.z));

		GL.Vertex(center + new Vector3(-size.x,-size.y,size.z));
		GL.Vertex(center + new Vector3(-size.x,-size.y,-size.z));

		//Top face:
		GL.Vertex(center + new Vector3(-size.x,size.y,-size.z));
		GL.Vertex(center + new Vector3(size.x,size.y,-size.z));
		
		GL.Vertex(center + new Vector3(size.x,size.y,-size.z));
		GL.Vertex(center + new Vector3(size.x,size.y,size.z));
		
		GL.Vertex(center + new Vector3(size.x,size.y,size.z));
		GL.Vertex(center + new Vector3(-size.x,size.y,size.z));
		
		GL.Vertex(center + new Vector3(-size.x,size.y,size.z));
		GL.Vertex(center + new Vector3(-size.x,size.y,-size.z));

		//Remaining edges:
		GL.Vertex(center + new Vector3(-size.x,size.y,-size.z));
		GL.Vertex(center + new Vector3(-size.x,-size.y,-size.z));
		
		GL.Vertex(center + new Vector3(size.x,size.y,-size.z));
		GL.Vertex(center + new Vector3(size.x,-size.y,-size.z));
		
		GL.Vertex(center + new Vector3(size.x,size.y,size.z));
		GL.Vertex(center + new Vector3(size.x,-size.y,size.z));
		
		GL.Vertex(center + new Vector3(-size.x,size.y,size.z));
		GL.Vertex(center + new Vector3(-size.x,-size.y,size.z));
		
	}

}
}

