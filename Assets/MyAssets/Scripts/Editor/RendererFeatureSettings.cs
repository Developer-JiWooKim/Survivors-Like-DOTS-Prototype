using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.MyAssets.Scripts.Editor
{
    /// <summary>
    /// URP 렌더러의 렌더러 기능(Renderer Feature) 변경을 코드로 남긴다 (CLAUDE.md 3.4 — 에셋은 코드로).
    ///
    /// SSAO 를 없애는 이유 (ISSUE-005):
    /// URP 템플릿 기본값으로 켜져 있던 SSAO 는 노멀이 필요해 **DepthNormal 프리패스**를 강제한다.
    /// 그 프리패스가 적 1 만 마리를 한 번 더 그려 지오메트리 제출량이 정확히 2 배가 됐다 (Frame Debugger 로 확인).
    /// 단색 2D 탑다운에서는 SSAO 의 시각적 이득이 거의 없다.
    ///
    /// 처음(2026-09-17)에는 **끄기만** 했다 — 되돌릴 때 설정값이 남아 있게.
    /// 그런데 그게 **플레이어 빌드를 검은 화면으로 만들었다 (ISSUE-018, 2026-10-15)**:
    /// URP 는 켜진 SSAO 가 없으면 빌드에서 SSAO 셰이더·리소스를 스트리핑하는데,
    /// 꺼진 기능도 렌더러 생성 때 <c>Create()</c> 가 불려 리소스를 찾다가 NRE → 파이프라인 생성 실패.
    /// 그래서 끄기가 아니라 **삭제**한다. 설정값은 템플릿 기본값이라 다시 추가해도 잃는 것이 없다.
    /// </summary>
    public static class RendererFeatureSettings
    {
        private const string PcRendererPath = "Assets/Settings/PC_Renderer.asset";

        [MenuItem("Tools/렌더러/SSAO 삭제")]
        public static void RemoveSsao()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(PcRendererPath);
            if (renderer == null)
            {
                Debug.LogError($"[RendererFeatureSettings] 렌더러를 찾지 못했습니다: {PcRendererPath}");
                return;
            }

            // rendererFeatures 리스트에서만 빼면 짝을 이루는 m_RendererFeatureMap(로컬 ID 목록)과 어긋난다.
            // URP 인스펙터가 기능을 지울 때처럼 SerializedObject 로 두 배열을 같은 인덱스에서 함께 지운다.
            var serialized = new SerializedObject(renderer);
            SerializedProperty features = serialized.FindProperty("m_RendererFeatures");
            SerializedProperty featureMap = serialized.FindProperty("m_RendererFeatureMap");

            int removed = 0;
            for (int i = features.arraySize - 1; i >= 0; i--)
            {
                var feature = features.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableRendererFeature;
                if (feature is not ScreenSpaceAmbientOcclusion)
                {
                    continue;
                }

                // 오브젝트 참조 원소는 한 번 지우면 null 로 남고, 두 번째에 칸이 사라진다.
                features.GetArrayElementAtIndex(i).objectReferenceValue = null;
                features.DeleteArrayElementAtIndex(i);
                if (featureMap != null && i < featureMap.arraySize)
                {
                    featureMap.DeleteArrayElementAtIndex(i);
                }

                serialized.ApplyModifiedProperties();

                // 기능은 렌더러 에셋의 서브 에셋이다. 목록에서만 빼면 파일 안에 고아로 남는다.
                AssetDatabase.RemoveObjectFromAsset(feature);
                Object.DestroyImmediate(feature, true);
                removed++;
            }

            EditorUtility.SetDirty(renderer);
            AssetDatabase.SaveAssets();

            Debug.Log($"[RendererFeatureSettings] {PcRendererPath} SSAO 삭제 — {removed} 개, 남은 기능 {renderer.rendererFeatures.Count} 개");
        }
    }
}
