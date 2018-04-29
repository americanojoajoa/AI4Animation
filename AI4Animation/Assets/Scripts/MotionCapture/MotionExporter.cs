﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEditor.SceneManagement;

public class MotionExporter : EditorWindow {

	public static EditorWindow Window;
	public static Vector2 Scroll;

	public string Directory = string.Empty;
	public int Framerate = 60;
	public int BatchSize = 10;
	public bool[] Export = new bool[0];
	public SceneAsset[] Animations = new SceneAsset[0];

    private bool Exporting = false;

	private static string Separator = " ";
	private static string Accuracy = "F5";

	[MenuItem ("Addons/Motion Exporter")]
	static void Init() {
		Window = EditorWindow.GetWindow(typeof(MotionExporter));
		Scroll = Vector3.zero;
	}
	
	void OnGUI() {
		Scroll = EditorGUILayout.BeginScrollView(Scroll);

		Utility.SetGUIColor(UltiDraw.Black);
		using(new EditorGUILayout.VerticalScope ("Box")) {
			Utility.ResetGUIColor();

			Utility.SetGUIColor(UltiDraw.Grey);
			using(new EditorGUILayout.VerticalScope ("Box")) {
				Utility.ResetGUIColor();

				Utility.SetGUIColor(UltiDraw.Orange);
				using(new EditorGUILayout.VerticalScope ("Box")) {
					Utility.ResetGUIColor();
					EditorGUILayout.LabelField("Exporter");
				}

                if(!Exporting) {
                    if(Utility.GUIButton("Export Labels", UltiDraw.DarkGrey, UltiDraw.White)) {
                        this.StartCoroutine(ExportLabels());
                    }
                    if(Utility.GUIButton("Export Data", UltiDraw.DarkGrey, UltiDraw.White)) {
                        this.StartCoroutine(ExportData());
                    }

                    EditorGUILayout.BeginHorizontal();
                    if(Utility.GUIButton("Enable All", UltiDraw.DarkGrey, UltiDraw.White)) {
                        for(int i=0; i<Export.Length; i++) {
                            Export[i] = true;
                        }
                    }
                    if(Utility.GUIButton("Disable All", UltiDraw.DarkGrey, UltiDraw.White)) {
                        for(int i=0; i<Export.Length; i++) {
                            Export[i] = false;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                } else {
                    if(Utility.GUIButton("Stop", UltiDraw.DarkRed, UltiDraw.White)) {
                        this.StopAllCoroutines();
                        Exporting = false;
                    }
                }
				
				Framerate = EditorGUILayout.IntField("Framerate", Framerate);
				BatchSize = Mathf.Max(1, EditorGUILayout.IntField("Batch Size", BatchSize));	

				using(new EditorGUILayout.VerticalScope ("Box")) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Assets/", GUILayout.Width(45f));
					LoadDirectory(EditorGUILayout.TextField(Directory));
					EditorGUILayout.EndHorizontal();

					for(int i=0; i<Animations.Length; i++) {

						if(Exporting && Animations[i].name == EditorSceneManager.GetActiveScene().name) {
							Utility.SetGUIColor(UltiDraw.Mustard);
						} else {
							if(Export[i]) {
								Utility.SetGUIColor(UltiDraw.DarkGreen);
							} else {
								Utility.SetGUIColor(UltiDraw.DarkRed);
							}
						}

						using(new EditorGUILayout.VerticalScope ("Box")) {
							Utility.ResetGUIColor();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField((i+1).ToString(), GUILayout.Width(20f));
							Export[i] = EditorGUILayout.Toggle(Export[i], GUILayout.Width(20f));
							Animations[i] = (SceneAsset)EditorGUILayout.ObjectField(Animations[i], typeof(SceneAsset), true);
							EditorGUILayout.EndHorizontal();
						}
					}
					
				}
			}
		}

		EditorGUILayout.EndScrollView();
	}

	private void LoadDirectory(string directory) {
		if(Directory != directory) {
			Directory = directory;
			Animations = new SceneAsset[0];
			Export = new bool[0];
			string path = "Assets/"+Directory;
			if(AssetDatabase.IsValidFolder(path)) {
				string[] files = AssetDatabase.FindAssets("t:SceneAsset", new string[1]{path});
				Animations = new SceneAsset[files.Length];
				Export = new bool[files.Length];
				for(int i=0; i<files.Length; i++) {
					Animations[i] = (SceneAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(files[i]), typeof(SceneAsset));
					Export[i] = true;
				}
			}
		}
	}

	private IEnumerator ExportLabels() {
        Exporting = true;

		string name = "Labels";
		string filename = string.Empty;
		string folder = Application.dataPath + "/../../../Export/";
		if(!File.Exists(folder+name+".txt")) {
			filename = folder+name;
		} else {
			int i = 1;
			while(File.Exists(folder+name+" ("+i+").txt")) {
				i += 1;
			}
			filename = folder+name+" ("+i+")";
		}
		StreamWriter file = File.CreateText(filename+".txt");

		EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Animations[0]));
		MotionEditor editor = FindObjectOfType<MotionEditor>();
		if(editor == null) {
			Debug.Log("No motion editor found in scene " + Animations[0].name + ".");
		} else {
			int index = 0;
			file.WriteLine(index + " " + "Sequence"); index += 1;
			file.WriteLine(index + " " + "Frame"); index += 1;
			file.WriteLine(index + " " + "Timestamp"); index += 1;
			for(int i=0; i<editor.GetActor().Bones.Length; i++) {
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "PositionX"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "PositionY"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "PositionZ"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "ForwardX"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "ForwardY"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "ForwardZ"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "UpX"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "UpY"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "UpZ"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "VelocityX"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "VelocityY"+(i+1)); index += 1;
				file.WriteLine(index + " " + editor.GetActor().Bones[i].GetName() + "VelocityZ"+(i+1)); index += 1;
			}
			for(int i=1; i<=12; i++) {
				file.WriteLine(index + " " + "TrajectoryPositionX"+i); index += 1;
				file.WriteLine(index + " " + "TrajectoryPositionZ"+i); index += 1;
				file.WriteLine(index + " " + "TrajectoryDirectionX"+i); index += 1;
				file.WriteLine(index + " " + "TrajectoryDirectionZ"+i); index += 1;
				file.WriteLine(index + " " + "TrajectoryVelocityX"+i); index += 1;
				file.WriteLine(index + " " + "TrajectoryVelocityZ"+i); index += 1;
				for(int j=1; j<=editor.Data.Styles.Length; j++) {
					file.WriteLine(index + " " + editor.Data.Styles[j-1] + i); index += 1;
				}
			}
			file.WriteLine(index + " " + "RootMotionX"); index += 1;
			file.WriteLine(index + " " + "RootMotionY"); index += 1;
			file.WriteLine(index + " " + "RootMotionZ"); index += 1;
		}

        yield return new WaitForSeconds(0f);

		file.Close();

        Exporting = false;
	}

	private IEnumerator ExportData() {
        Exporting = true;

		string name = "Data";
		string filename = string.Empty;
		string folder = Application.dataPath + "/../../../Export/";
		if(!File.Exists(folder+name+".txt")) {
			filename = folder+name;
		} else {
			int i = 1;
			while(File.Exists(folder+name+" ("+i+").txt")) {
				i += 1;
			}
			filename = folder+name+" ("+i+")";
		}
		StreamWriter file = File.CreateText(filename+".txt");

		int sequence = 0;
		int items = 0;

        for(int i=0; i<Animations.Length; i++) {
            if(Export[i]) {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Animations[i]));
                MotionEditor editor = FindObjectOfType<MotionEditor>();
                if(editor == null) {
                    Debug.Log("No motion editor found in scene " + Animations[i].name + ".");
                } else {
					for(int m=1; m<=2; m++) {
						for(int s=0; s<editor.Data.Sequences.Length; s++) {
							sequence += 1;
							float start = editor.Data.GetFrame(editor.Data.Sequences[s].Start).Timestamp;
							float end = editor.Data.GetFrame(editor.Data.Sequences[s].End).Timestamp;
							for(float t=start; t<=end; t+=1f/Framerate) {
								string line = string.Empty;

								if(m==1) {
									editor.SetMirror(false);
								} else {
									editor.SetMirror(true);
								}
								editor.LoadFrame(t);
								MotionEditor.FrameState state = editor.GetState();

								line += sequence + Separator;
								line += state.Index + Separator;
								line += state.Timestamp + Separator;

								//Bone data
								for(int k=0; k<state.BoneTransformations.Length; k++) {
									//Position
									line += FormatVector3(state.BoneTransformations[k].GetPosition().GetRelativePositionTo(state.Root));

									//Rotation
									line += FormatVector3(state.BoneTransformations[k].GetForward().GetRelativeDirectionTo(state.Root));
									line += FormatVector3(state.BoneTransformations[k].GetUp().GetRelativeDirectionTo(state.Root));

									//Bone Velocity
									line += FormatVector3(state.BoneVelocities[k].GetRelativeDirectionTo(state.Root));
								}
								
								//Trajectory data
								for(int k=0; k<12; k++) {
									Vector3 position = state.Trajectory.Points[k].GetPosition().GetRelativePositionTo(state.Root);
									Vector3 direction = state.Trajectory.Points[k].GetDirection().GetRelativeDirectionTo(state.Root);
									Vector3 velocity = state.Trajectory.Points[k].GetVelocity().GetRelativeDirectionTo(state.Root);
									line += FormatValue(position.x);
									line += FormatValue(position.z);
									line += FormatValue(direction.x);
									line += FormatValue(direction.z);
									line += FormatValue(velocity.x);
									line += FormatValue(velocity.z);
									line += FormatArray(state.Trajectory.Points[k].Styles);
								}

								//Height map
								//for(int k=0; k<state.HeightMap.Points.Length; k++) {
								//	float distance = Vector3.Distance(state.HeightMap.Points[k], state.HeightMap.Pivot.GetPosition());
								//	line += FormatValue(distance);
								//}

								//Depth map
								//for(int k=0; k<state.DepthMap.Points.Length; k++) {
								//	float distance = Vector3.Distance(state.DepthMap.Points[k], state.DepthMap.Pivot.GetPosition());
								//	line += FormatValue(distance);
								//}

								//Root motion
								line += FormatVector3(state.RootMotion);

								//Finish
								line = line.Remove(line.Length-1);
								line = line.Replace(",",".");
								file.WriteLine(line);

								items += 1;
								if(items == BatchSize) {
									items = 0;
									yield return new WaitForSeconds(0f);
								}
							}
						}
					}
                }
            }
        }

        yield return new WaitForSeconds(0f);
        
		file.Close();

        Exporting = false;
	}

	private string FormatString(string value) {
		return value + Separator;
	}

	private string FormatValue(float value) {
		return value.ToString(Accuracy) + Separator;
	}

	private string FormatArray(float[] array) {
		string format = string.Empty;
		for(int i=0; i<array.Length; i++) {
			format += array[i].ToString(Accuracy) + Separator;
		}
		return format;
	}

	private string FormatArray(bool[] array) {
		string format = string.Empty;
		for(int i=0; i<array.Length; i++) {
			float value = array[i] ? 1f : 0f;
			format += value.ToString(Accuracy) + Separator;
		}
		return format;
	}

	private string FormatVector3(Vector3 vector) {
		return vector.x.ToString(Accuracy) + Separator + vector.y.ToString(Accuracy) + Separator + vector.z.ToString(Accuracy) + Separator;
	}

	private string FormatQuaternion(Quaternion quaternion, bool imaginary, bool real) {
		string output = string.Empty;
		if(imaginary) {
			output += quaternion.x + Separator + quaternion.y + Separator + quaternion.z + Separator;
		}
		if(real) {
			output += quaternion.w + Separator;
		}
		return output;
	}

}
#endif