using UnityEditor;
using UnityEngine;

namespace Assets.NastyWizard_IK.NastyIK.Scripts.CustomEditor
{
/*
    [UnityEditor.CustomEditor(typeof(CustomIKSolver))]
    [CanEditMultipleObjects]
    public class EditorIKSolver : Editor
    {

        SerializedProperty JointsProp;
        SerializedProperty AnkleProp;
        SerializedProperty TargetProp;
        SerializedProperty ItterationsProp;

        private GUISkin Skin;

        private Texture IKTexture;
        private Texture PlusTexture;
        private Texture MinusTexture;
        private bool IKActive;

        // Use this for initialization
        void OnEnable()
        {
            JointsProp = serializedObject.FindProperty("Joints");
            AnkleProp = serializedObject.FindProperty("Ankle");
            TargetProp = serializedObject.FindProperty("Target");
            ItterationsProp = serializedObject.FindProperty("Itterations");
            IKTexture = Resources.Load("IKMenu") as Texture;
            PlusTexture = Resources.Load("PlusImg") as Texture;
            MinusTexture = Resources.Load("MinusImg") as Texture;
            Skin = Resources.Load("NastySkin") as GUISkin;

            //EditorApplication.update -= OnUpdate();
            //EditorApplication.update += OnUpdate();
        }

        // Update is called once per frame
        void Update() {
            GUI.skin = Skin;
        }

        public override void OnInspectorGUI ()
        {
            GUI.skin = Skin;
            CustomIKSolver t = target as CustomIKSolver;
            serializedObject.Update();

            EditorGUILayout.PrefixLabel(new GUIContent(IKTexture));
            GUI.color = new Color(61.0f / 255.0f, 147.0f / 255.0f, 160.0f / 255.0f);

            EditorGUILayout.BeginHorizontal("box");
            t.label = EditorGUILayout.TextField(t.label); // draw name box
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box"); // 0
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(new GUIContent(IKTexture),GUILayout.Width(16));
            if (GUILayout.Button("Joints")) // draw joints 
            {
                IKActive = !IKActive;
            }

            EditorGUILayout.EndHorizontal();

            if (IKActive)
            {
                EditorGUI.indentLevel++;

                // Enter the array

                if (t && t.Joints != null && t.Joints.Count > 0)
                {
                    JointsProp = serializedObject.FindProperty("Joints");
                    JointsProp.Next(true);
                    JointsProp.Next(true);
                    JointsProp.Next(true);
                }
                //

                // loop through array
                if (t.Joints != null)
                    for (int i = 0; i < t.Joints.Count; i++)
                    {
                        GUI.color = new Color(1, 1, 1);

                        EditorGUILayout.BeginVertical("box"); // 1
                        EditorGUILayout.BeginHorizontal(); // 1

                        if (t.Joints[i] != null)
                            EditorGUILayout.LabelField(new GUIContent(t.Joints[i].name), GUILayout.Width(100));
                        else
                            EditorGUILayout.LabelField(new GUIContent("!Empty!"), GUILayout.Width(100));

                        EditorGUILayout.PropertyField(JointsProp);
                        if (GUILayout.Button("X")) // remove
                        {
                            t.Joints.RemoveAt(i);
                        }


                        EditorGUILayout.EndHorizontal(); // 1

                        if (t.Joints[i] != null)
                        {
                            EditorGUILayout.BeginHorizontal(); // 2
                            if (!t.Joints[i].detailToggle)
                                GUILayout.Label(new GUIContent(PlusTexture), GUILayout.Width(16));
                            else
                                GUILayout.Label(new GUIContent(MinusTexture), GUILayout.Width(16));

                            if (GUILayout.Button("Details", GUILayout.Width(100 - 16))) // 
                            {
                                t.Joints[i].detailToggle = !t.Joints[i].detailToggle;
                            }

                            EditorGUILayout.EndHorizontal(); // 2

                            if (t.Joints[i].detailToggle)
                            {
                                GUILayout.Label(new GUIContent("Start Offset: " + t.Joints[i].transform.localPosition));
                                GUILayout.Label(new GUIContent("Position: " + t.Joints[i].transform.position));
                            }
                        }

                        EditorGUILayout.EndVertical(); // 1

                        GUI.color = Color.white;
                        JointsProp.Next(false);
                    }


                // add 
                EditorGUILayout.BeginHorizontal("box");

                GUILayout.Label(new GUIContent(PlusTexture), GUILayout.Width(16));
                if (GUILayout.Button("ADD"))
                {
                    t.Joints?.Add(null);
                }
                GUILayout.Label(new GUIContent(PlusTexture), GUILayout.Width(16));

                EditorGUILayout.EndHorizontal();



                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical(); // 0


            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(AnkleProp);
            EditorGUILayout.PropertyField(TargetProp);
            EditorGUILayout.PropertyField(ItterationsProp);
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(20);

            t.Visualise = EditorGUILayout.Toggle("Visualise:", t.Visualise);

            EditorGUILayout.EndVertical();

            GUILayout.Space(20);
            //base.OnInspectorGUI();
        }



        void OnSceneGUI() // visualise the joint location
        {

            CustomIKSolver t = (target as CustomIKSolver);

            if (t.Visualise)
            {

                //Handles.BeginGUI();

                Handles.color = new Color(1.0f,.6f,0.0f);
                for (int i = 0; i < t.Joints.Count; i++)
                    Handles.DrawWireDisc(t.Joints[i].transform.position, t.Joints[i].transform.up,1.0f);
                //Handles.EndGUI();

            }
        }
    }
*/
}
