using UnityEngine;

namespace CIFER.Tech.UnitychanToonShaderConverter
{
    public static class UnitychanToonShaderConverter
    {
        public static void UtsConvert(UnitychanToonShaderConverterData converterData)
        {
            var mtoon = Shader.Find("VRM/MToon");

            var length = converterData.UtsMaterials.Length < converterData.MtoonMaterials.Length
                ? converterData.UtsMaterials.Length
                : converterData.MtoonMaterials.Length;

            for (var i = 0; i < length; i++)
            {
                if (converterData.UtsMaterials[i] == null || converterData.MtoonMaterials[i] == null)
                    continue;

                converterData.MtoonMaterials[i].shader = mtoon;

                #region Color

                converterData.MtoonMaterials[i]
                    .SetColor("_Color", converterData.UtsMaterials[i].GetColor("_BaseColor"));

                converterData.MtoonMaterials[i].SetColor("_ShadeColor",
                    converterData.UtsMaterials[i].GetColor("_1st_ShadeColor"));

                converterData.MtoonMaterials[i]
                    .SetTexture("_MainTex", converterData.UtsMaterials[i].GetTexture("_MainTex"));

                converterData.MtoonMaterials[i].SetTexture("_ShadeTexture",
                    converterData.UtsMaterials[i].GetTexture("_1st_ShadeMap"));

                #endregion

                #region Lighting

                converterData.MtoonMaterials[i].SetFloat("_ShadeToony",
                    (1 - converterData.UtsMaterials[i].GetFloat("_BaseShade_Feather")) / 0.9999f);

                converterData.MtoonMaterials[i]
                    .SetTexture("_BumpMap", converterData.UtsMaterials[i].GetTexture("_NormalMap"));

                var baseColorStep = converterData.UtsMaterials[i].GetFloat("_BaseColor_Step") * 2 - 1;
                converterData.MtoonMaterials[i].SetFloat("_ShadeShift", baseColorStep);


                var systemShadowsLevel = converterData.UtsMaterials[i].GetFloat("_Set_SystemShadowsToBase") > 0 &&
                                         baseColorStep >= 0
                    ? converterData.UtsMaterials[i].GetFloat("_Tweak_SystemShadowsLevel") + 0.5f
                    : 0f;
                converterData.MtoonMaterials[i].SetFloat("_ReceiveShadowRate", systemShadowsLevel);

                //lit & shade mixing multiplier
                //UTS側に該当するパラメータ無し

                //LightColor Attenuation
                //UTS側に該当するパラメータ無し

                converterData.MtoonMaterials[i]
                    .SetFloat("_IndirectLightIntensity", converterData.UtsMaterials[i].GetFloat("_GI_Intensity"));

                #endregion

                #region Emission

                //Emission
                converterData.MtoonMaterials[i]
                    .SetTexture("_EmissionMap", converterData.UtsMaterials[i].GetTexture("_Emissive_Tex"));

                converterData.MtoonMaterials[i]
                    .SetColor("_EmissionColor", converterData.UtsMaterials[i].GetColor("_Emissive_Color"));

                if (converterData.UtsMaterials[i].GetFloat("_MatCap") > 0)
                {
                    converterData.MtoonMaterials[i]
                        .SetTexture("_SphereAdd", converterData.UtsMaterials[i].GetTexture("_MatCap_Sampler"));
                }

                #endregion

                #region Rim

                //Rim
                converterData.MtoonMaterials[i]
                    .SetTexture("_RimTexture", converterData.UtsMaterials[i].GetTexture("_Set_RimLightMask"));

                converterData.MtoonMaterials[i]
                    .SetColor("_RimColor", converterData.UtsMaterials[i].GetColor("_RimLightColor"));

                //Lighting Mix
                //UniVRM最新版（v0.55）だと動いてない
                //どのバージョンから動いてないんだろ…？
                //https://github.com/Santarh/MToon/issues/84
                if (converterData.UtsMaterials[i].GetFloat("_LightDirection_MaskOn") > 0)
                {
                    converterData.MtoonMaterials[i]
                        .SetFloat("_RimLightingMix",
                            converterData.UtsMaterials[i].GetFloat("_Tweak_LightDirection_MaskLevel") * 2);
                }

                converterData.MtoonMaterials[i]
                    .SetFloat("_RimFresnelPower",
                        converterData.UtsMaterials[i].GetFloat("_RimLight_FeatherOff") * 92 + 8);

                var rimLightPower = converterData.UtsMaterials[i].GetFloat("_RimLight_Power");
                var rimLightInsideMask = converterData.UtsMaterials[i].GetFloat("_RimLight_InsideMask");
                var rimLift = Mathf.Abs(Mathf.Log10(1 - rimLightInsideMask) * rimLightPower + 0.0001f);
                converterData.MtoonMaterials[i].SetFloat("_RimLift", rimLift);

                #endregion

                #region Outline

                if (converterData.UtsMaterials[i].HasProperty("_OUTLINE"))
                {
                    converterData.MtoonMaterials[i].SetFloat("_OutlineWidthMode", 1f);

                    converterData.MtoonMaterials[i].SetTexture("_OutlineWidthTexture",
                        converterData.UtsMaterials[i].GetTexture("_Outline_Sampler"));

                    converterData.MtoonMaterials[i].SetFloat("_OutlineWidth",
                        Mathf.Abs(converterData.UtsMaterials[i].GetFloat("_Outline_Width")) / 10f);

                    converterData.MtoonMaterials[i].SetFloat("_OutlineColorMode", 1f);

                    converterData.MtoonMaterials[i].SetColor("_OutlineColor",
                        converterData.UtsMaterials[i].GetColor("_Outline_Color"));
                }
                else
                {
                    converterData.MtoonMaterials[i].SetFloat("_OutlineWidthMode", 0f);
                }

                #endregion
            }
        }

        public struct UnitychanToonShaderConverterData
        {
            public Material[] UtsMaterials;
            public Material[] MtoonMaterials;
        }
    }
}