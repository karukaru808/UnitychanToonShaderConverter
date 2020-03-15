using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CIFER.Tech.UnitychanToonShaderConverter.Editor
{
    public class UnitychanToonShaderConverterWindow : EditorWindow
    {
        public Material[] utsMaterials = new Material[0];
        public Material[] mtoonMaterials = new Material[0];

        private Vector2 _utsScrollPos = Vector2.zero;
        private Vector2 _mtoonScrollPos = Vector2.zero;

        [MenuItem("CIFER.Tech/UnitychanToonShaderConverter/For VRM")]
        private static void Open()
        {
            var window = GetWindow<UnitychanToonShaderConverterWindow>("UTSC For VRM");
            window.minSize = new Vector2(650f, 300f);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                //変換元
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (GUILayout.Button("選択中のマテリアルを登録する"))
                    {
                        var selectedMaterials = GetSelectedMaterials();
                        if (selectedMaterials != null)
                            utsMaterials = selectedMaterials;
                    }

                    EditorGUILayout.Space();

                    _utsScrollPos = EditorGUILayout.BeginScrollView(_utsScrollPos, GUI.skin.box);
                    {
                        ArrayApplyModifiedProperties(nameof(utsMaterials), "変換元");
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                //変換先
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (GUILayout.Button("選択中のマテリアルを登録する"))
                    {
                        var selectedMaterials = GetSelectedMaterials();
                        if (selectedMaterials != null)
                            mtoonMaterials = selectedMaterials;
                    }

                    EditorGUILayout.Space();

                    _mtoonScrollPos = EditorGUILayout.BeginScrollView(_mtoonScrollPos, GUI.skin.box);
                    {
                        ArrayApplyModifiedProperties(nameof(mtoonMaterials), "変換先");
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();


            //エラー、警告判定
            if (utsMaterials.Length <= 0 || mtoonMaterials.Length <= 0)
            {
                EditorGUILayout.HelpBox("リストの状態が不正です。", MessageType.Error);
                return;
            }

            if (utsMaterials.Where(uts => uts != null).Any(uts => !uts.shader.name.StartsWith("UnityChanToonShader")))
            {
                EditorGUILayout.HelpBox("変換元のシェーダがUTS2ではありません。", MessageType.Error);
                return;
            }

            if (utsMaterials.Any(uts => uts == null) || mtoonMaterials.Any(mtoon => mtoon == null))
            {
                EditorGUILayout.HelpBox("Nullが含まれています。\r\n該当要素はスキップされます。", MessageType.Warning);
            }

            if (utsMaterials.Length != mtoonMaterials.Length)
            {
                EditorGUILayout.HelpBox("配列のサイズが違います。\r\n足りない要素分はスキップされます。", MessageType.Warning);
            }

            //コピー！
            if (GUILayout.Button("変換！"))
            {
                var copyData = new UnitychanToonShaderConverter.UnitychanToonShaderConverterData()
                {
                    UtsMaterials = utsMaterials,
                    MtoonMaterials = mtoonMaterials
                };
                UnitychanToonShaderConverter.UtsConvert(copyData);
            }
        }

        private void ArrayApplyModifiedProperties(string varName, string dispName)
        {
            ScriptableObject target = this;
            var so = new SerializedObject(target);
            var sp = so.FindProperty(varName);
            EditorGUILayout.PropertyField(sp, new GUIContent(dispName), true, GUILayout.ExpandWidth(true));
            so.ApplyModifiedProperties();
        }

        private static Material[] GetSelectedMaterials()
        {
            return Selection.GetFiltered(typeof(Material), SelectionMode.Assets).Select(obj => obj as Material)
                .ToArray();
        }
    }
}