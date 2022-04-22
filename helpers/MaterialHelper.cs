using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class MaterialHelper
    {
        public static void RecolorAllMaterials(GameObject obj, Color color, string shaderKey = null, bool emissive = false, string[] shaderKeywords = null, bool forceEnable = false)
        {
            Color halfMain = new Color(color.r, color.g, color.b);
            halfMain.a = 0.5f;

            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (forceEnable)
                    renderer.enabled = true;

                foreach (Material material in renderer.materials)
                {
                    if (!string.IsNullOrEmpty(shaderKey))
                        material.shader = Shader.Find(shaderKey);

                    if (emissive)
                    {
                        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                        material.EnableKeyword("_EMISSION");
                    }

                    if (shaderKeywords != null)
                        material.SetShaderKeywords(shaderKeywords);         

                    if (material.HasProperty("_EmissionColor"))
                        material.SetColor("_EmissionColor", color * 0.5f);

                    if (material.HasProperty("_MainColor"))
                        material.SetColor("_MainColor", color);
                    if (material.HasProperty("_RimColor"))
                        material.SetColor("_RimColor", color);
                    if (material.HasProperty("_Color"))
                        material.SetColor("_Color", halfMain);
                }
            }
        }
    }
}