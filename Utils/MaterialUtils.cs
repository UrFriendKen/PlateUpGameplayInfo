using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KitchenGameplayInfo.Utils
{
    internal static class MaterialUtils
    {
        public static void ApplyMaterial(GameObject parent, string childPath, Material[] materials)
        {
            parent.GetChild(childPath)?.ApplyMaterial(materials);
        }

        public static GameObject ApplyMaterial(this GameObject gameObject, params Material[] materials)
        {
            return gameObject.ApplyMaterial<MeshRenderer>(materials);
        }

        public static GameObject ApplyMaterial<T>(this GameObject gameObject, params Material[] materials) where T : Renderer
        {
            T val = (((object)gameObject != null) ? gameObject.GetComponent<T>() : null);
            if (val == null)
            {
                return gameObject;
            }
            val.materials = materials;
            return gameObject;
        }

        public static Dictionary<string, ShaderPropertyType> GetShaderProperties(Shader shader)
        {
            Dictionary<string, ShaderPropertyType> shaderProperties = new Dictionary<string, ShaderPropertyType>();
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string propName = shader.GetPropertyName(i);
                ShaderPropertyType propType = shader.GetPropertyType(i);
                if (shaderProperties.ContainsKey(propName))
                {
                    continue;
                }
                shaderProperties.Add(propName, propType);
            }
            return shaderProperties;
        }

        public static Material CreateMaterial(string shaderName,
            string name = null,
            Dictionary<string, object> values = null,
            Dictionary<string, bool> keywords = null)
        {
            Shader shader = null;
            if (!shaderName.IsNullOrEmpty())
                shader = Shader.Find(shaderName);
            if (shader == null)
                return null;
            Material material = new Material(shader);

            if (!name.IsNullOrEmpty())
                material.name = name;

            Dictionary<string, ShaderPropertyType> shaderProperties = GetShaderProperties(shader);

            if (values != null)
            {
                foreach (KeyValuePair<string, object> item in values)
                {
                    string key = item.Key;
                    if (!shaderProperties.TryGetValue(key, out ShaderPropertyType propertyType))
                        continue;
                    object value = item.Value;
                    switch (propertyType)
                    {
                        case ShaderPropertyType.Color:
                        case ShaderPropertyType.Vector:
                            bool isVector = false;
                            Vector4 vectorVal = default;
                            if (value is Vector4)
                            {
                                isVector = true;
                                vectorVal = (Vector4)value;
                            }
                            if (value is Color colorVal)
                            {
                                isVector = true;
                                vectorVal = colorVal;
                            }
                            if (isVector)
                                material.SetVector(key, vectorVal);
                            break;
                        case ShaderPropertyType.Texture:
                            if (value is Texture textureVal)
                                material.SetTexture(key, textureVal);
                            break;
                        case ShaderPropertyType.Range:
                        case ShaderPropertyType.Float:
                            if (value is float floatVal)
                                material.SetFloat(key, floatVal);
                            break;
                    }
                }
            }

            if (keywords != null)
            {
                foreach (KeyValuePair<string, bool> item in keywords)
                {
                    string key = item.Key;
                    bool isEnable = item.Value;
                    if (isEnable)
                        material.EnableKeyword(key);
                    else
                        material.DisableKeyword(key);
                }
            }
            return material;
        }

        public static Material CreateBlockOutBackground(
            string name = null,
            Color? colour = null)
        {
            return CreateMaterial("Block Out Background", name, values: new Dictionary<string, object>()
            {
                { "_Colour", colour ?? new Color(0.2075472f, 0.2063724f, 0.2063724f, 0f) }
            });
        }

        public static Material CreateBlueprintLight(
            string name = null,
            Color? color = null,
            Color? color0 = null,
            bool hasColour = false,
            bool isCopy = false)
        {
            return CreateMaterial("Blueprint Light", name, values: new Dictionary<string, object>()
            {
                { "_Color", color ?? new Color(8,0,0,0) },
                { "_Color0", color0 ?? new Color(0.137985f, 0.2685301f, 0.4811321f, 0f) },
                { "_HasColour", hasColour? 1f : 0f },
                { "_IsCopy", isCopy ? 1f : 0f }
            });
        }

        public static Material CreateFairyLight(
            string name = null,
            Color? color0 = null,
            float float0 = 0f)
        {
            return CreateMaterial("Fairy Light", name, values: new Dictionary<string, object>()
            {
                { "_Color0", color0 ?? new Color(8f, 0f, 0f, 0f) },
                { "_Float0", float0 }
            });
        }

        public static Material CreateFlatImage(
            string name = null,
            Texture2D image = null,
            float alpha = 1f,
            float blowoutScale = 1f,
            float blowoutScaleOffset = 0f,
            bool isBlowout = false)
        {
            return CreateMaterial("Flat Image", name, values: new Dictionary<string, object>()
            {
                { "_Image", image },
                { "_Alpha", Mathf.Clamp01(alpha) },
                { "_BlowoutScale1", Mathf.Clamp01(blowoutScale) },
                { "_BlowoutOffset1", Mathf.Clamp01(blowoutScaleOffset) },
                { "_isBlowout", isBlowout ? 1f : 0f }
            });
        }

        public static Material CreateFlat(
            string name = null,
            Color? color0 = null)
        {
            return CreateMaterial("Flat", name, values: new Dictionary<string, object>()
            {
                { "_Color0", color0 ?? Color.black }
            });
        }

        public static Material CreateFog(
            string name = null)
        {
            return CreateMaterial("Fog", name);
        }

        public static Material CreateFoliage(
            string name = null,
            Color? color0 = null,
            Color? color1 = null)
        {
            return CreateMaterial("Foliage", name, values: new Dictionary<string, object>()
            {
                { "_Color0", color0 ?? new Color(0.4181768f, 0.4627451f, 0.1450981f, 1f) },
                { "_Color1", color1 ?? new Color(0.3211674f, 0.4622642f, 0.1456568f, 1f) }
            });
        }

        public static Material CreateGhost(
            string name = null,
            Color? colour = null,
            bool isHatched = false)
        {
            return CreateMaterial("Ghost", name, values: new Dictionary<string, object>()
            {
                { "_Colour", colour ?? new Color(0.2584905f, 0.4306628f, 1f, 1f) },
                { "_Hatched", isHatched ? 1f : 0f }
            },
            new Dictionary<string, bool>()
            {
                { "_HATCHED_ON", isHatched }
            });
        }

        public static Material CreateIndicatorLight(
            string name = null,
            Color? color = null)
        {

            return CreateMaterial("Indicator Light", name, values: new Dictionary<string, object>()
            {
                { "_Color", color ?? new Color(8f, 0f, 0f, 0f) }
            });
        }

        public static Material CreateLakeSurface(
            string name = null,
            Color? color0 = null,
            float timeScale = 1f,
            Color? color1 = null,
            Vector4? daySpec = null,
            float scale = 1f,
            Vector4? nightSpec = null)
        {

            return CreateMaterial("Lake Surface", name, values: new Dictionary<string, object>()
            {
                { "_Color0", color0 ?? new Color(0.5377715f, 0.7173741f, 0.8584906f, 1f) },
                { "_TimeScale", timeScale },
                { "_Color1", color1 ?? new Color(0.7784977f, 0.8151122f, 0.8301887f, 1f) },
                { "_DaySpec", daySpec ?? new Vector4(2.912527f, 3.044249f, 3.102792f, 1f) },
                { "_Scale", scale },
                { "_NightSpec", nightSpec ?? new Vector4(2.912527f, 3.044249f, 3.102792f, 1f) }
            });
        }

        public static Material CreateMirrorBacking(
            string name = null,
            Color? color0 = null)
        {

            return CreateMaterial("Mirror Backing", name, values: new Dictionary<string, object>()
            {
                { "_Color0", color0 ?? Color.black }
            });
        }

        public static Material CreateMirrorSurface(
            string name = null,
            Color? color1 = null,
            bool highlight = false)
        {

            return CreateMaterial("Mirror Backing", name, values: new Dictionary<string, object>()
            {
                { "_Color1", color1 ?? Color.black },
                { "_Highlight", highlight ? 1f : 0f }
            });
        }

        public static Material CreateNewspaper(
            string name = null,
            Texture2D photograph = null,
            float alpha = 1f,
            float blowoutScale = 0f,
            float blowoutOffset = 0f,
            float colour = 0f)
        {

            return CreateMaterial("Newspaper", name, values: new Dictionary<string, object>()
            {
                { "_Photograph", photograph },
                { "_Alpha", Mathf.Clamp01(alpha) },
                { "_BlowoutScale", Mathf.Clamp01(blowoutScale) },
                { "_BlowoutOffset", Mathf.Clamp01(blowoutOffset) },
                { "_Colour", Mathf.Clamp01(colour) }
            });
        }

        public static Material CreatePing(
            string name = null,
            Color? playerColour = null)
        {

            return CreateMaterial("Ping", name, values: new Dictionary<string, object>()
            {
                { "_PlayerColour", playerColour ?? new Color(1f, 0.2348771f, 0f, 0f) }
            });
        }

        public static Material CreatePreviewDoor(
            string name = null)
        {

            return CreateMaterial("Preview Door", name);
        }

        public static Material CreatePreviewFloor(
            string name = null,
            float lineRate = 3f,
            float lineOffset = 1f,
            bool isKitchenFloor = false)
        {

            return CreateMaterial("Preview Floor", name, new Dictionary<string, object>()
            {
                { "_LineRate", Mathf.Clamp(lineRate, 1f, 30f) },
                { "_LineOffset", Mathf.Clamp01(lineOffset) },
                { "_IsKitchenFloor", isKitchenFloor ? 1f : 0f }
            },
            new Dictionary<string, bool>()
            {
                { "_ISKITCHENFLOOR_ON", isKitchenFloor }
            });
        }

        public static Material CreatePreviewWall(
            string name = null)
        {

            return CreateMaterial("Preview Wall", name);
        }

        public static Material CreateSimpleFlatTransparent(
            string name = null,
            Color? colour2 = null,
            Color? color0 = null,
            bool highlight = false,
            Texture2D overlay = null,
            bool textureByUV = false,
            bool varyByUV = false,
            float shininess = 0f,
            float overlayLowerBound = 0.6f,
            float overlayUpperBound = 0.9f,
            float overlayScale = 10f,
            float overlayMin = 0f,
            float overlayMax = 0.3f,
            Vector4? overlayOffset = null,
            Vector4? overlayTextureScale = null,
            Color? overlayColour = null,
            float flatness = 0f)
        {
            bool hasTextureOverlay = overlay != null;
            return CreateMaterial("Simple Flat Transparent", name, new Dictionary<string, object>()
            {
                { "_Colour2", colour2 ?? Color.black },
                { "_Color0", color0 ?? Color.black },
                { "_Highlight", highlight ? 1f : 0f },
                { "_Overlay", overlay },
                { "_TextureByUV", textureByUV ? 1f : 0f },
                { "_VaryByUV", varyByUV ? 1f : 0f },
                { "_HasTextureOverlay", hasTextureOverlay ? 1f : 0f },
                { "_Shininess", Mathf.Clamp01(shininess) },
                { "_OverlayLowerBound", Mathf.Clamp01(overlayLowerBound) },
                { "_OverlayUpperBound", Mathf.Clamp01(overlayUpperBound) },
                { "_OverlayScale", Mathf.Clamp(overlayScale, 0f, 1000f) },
                { "_OverlayMin", Mathf.Clamp01(overlayMin) },
                { "_OverlayMax", Mathf.Clamp01(overlayMax) },
                { "_OverlayOffset", overlayOffset ?? Vector4.zero },
                { "_OverlayTextureScale", overlayTextureScale ?? new Vector4(10f, 10f, 0f, 0f) },
                { "_OverlayColour", overlayColour ?? Color.black },
                { "_Flatness", Mathf.Clamp01(flatness) }
            },
            new Dictionary<string, bool>()
            {
                { "_TEXTUREBYUV_ON", textureByUV },
                { "_VARYBYUV_ON", varyByUV },
                { "_HASTEXTUREOVERLAY_ON", hasTextureOverlay }
            });
        }

        public static Material CreateSimpleFlat(
            string name = null,
            Color? colour2 = null,
            Color? color0 = null,
            bool highlight = false,
            Texture2D overlay = null,
            bool textureByUV = false,
            bool varyByUV = false,
            float shininess = 0f,
            float overlayLowerBound = 0.6f,
            float overlayUpperBound = 0.9f,
            float overlayScale = 10f,
            float overlayMin = 0f,
            float overlayMax = 0.3f,
            Vector4? overlayOffset = null,
            Vector4? overlayTextureScale = null,
            Color? overlayColour = null,
            float flatness = 0f)
        {
            bool hasTextureOverlay = overlay != null;
            return CreateMaterial("Simple Flat", name, new Dictionary<string, object>()
            {
                { "_Colour2", colour2 ?? Color.black },
                { "_Color0", color0 ?? Color.black },
                { "_Highlight", highlight ? 1f : 0f },
                { "_Overlay", overlay },
                { "_TextureByUV", textureByUV ? 1f : 0f },
                { "_VaryByUV", varyByUV ? 1f : 0f },
                { "_HasTextureOverlay", hasTextureOverlay ? 1f : 0f },
                { "_Shininess", Mathf.Clamp01(shininess) },
                { "_OverlayLowerBound", Mathf.Clamp01(overlayLowerBound) },
                { "_OverlayUpperBound", Mathf.Clamp01(overlayUpperBound) },
                { "_OverlayScale", Mathf.Clamp(overlayScale, 0f, 1000f) },
                { "_OverlayMin", Mathf.Clamp01(overlayMin) },
                { "_OverlayMax", Mathf.Clamp01(overlayMax) },
                { "_OverlayOffset", overlayOffset ?? Vector4.zero },
                { "_OverlayTextureScale", overlayTextureScale ?? new Vector4(10f, 10f, 0f, 0f) },
                { "_OverlayColour", overlayColour ?? Color.black },
                { "_Flatness", Mathf.Clamp01(flatness) }
            },
            new Dictionary<string, bool>()
            {
                { "_TEXTUREBYUV_ON", textureByUV },
                { "_VARYBYUV_ON", varyByUV },
                { "_HASTEXTUREOVERLAY_ON", hasTextureOverlay }
            });
        }

        public static Material CreateSimpleTransparent(
            string name = null,
            Color? color = null)
        {

            return CreateMaterial("Simple Transparent", name, values: new Dictionary<string, object>()
            {
                { "_Color", color ?? new Color(0.9245283f, 0.9245283f, 0.9245283f, 0.5450981f) }
            });
        }

        public static Material CreateTapStream(
            string name = null)
        {

            return CreateMaterial("Tap Stream", name);
        }

        public static Material CreateUICard(
            string name = null,
            Color? title = null,
            float borderWidth = 0.01f,
            float borderOffset = 0.6235294f,
            float edgeOffset = 0.075f,
            float titleHeight = 0.075f,
            float titleThickness = 0.075f)
        {

            return CreateMaterial("UI Card", name, values: new Dictionary<string, object>()
            {
                { "_Title", title ?? new Color(0.05882353f, 0.1568628f, 0.2627451f, 1f) },
                { "_BorderWidth", Mathf.Clamp(borderWidth, 0f, 0.05f) },
                { "_BorderOffset", Mathf.Clamp01(borderOffset) },
                { "_EdgeOffset", Mathf.Clamp(edgeOffset, 0f, 0.2f) },
                { "_TitleHeight", Mathf.Clamp(titleHeight, 0f, 1.5f) },
                { "_TitleThickness", Mathf.Clamp(titleThickness, 0f, 0.5f) }
            });
        }

        public static Material CreateUIPanel(
            string name = null,
            Color? highlight = null,
            Texture2D mainTex = null,
            bool shadow = false,
            float transparency = 1f)
        {

            return CreateMaterial("UI Panel", name, values: new Dictionary<string, object>()
            {
                { "_Highlight", highlight ?? new Color(0.05882353f, 0.1568628f, 0.2627451f, 1f) },
                { "_MainTex", mainTex },
                { "_Shadow", shadow ? 1f : 0f },
                { "_Transparency", Mathf.Clamp01(transparency) }
            },
            new Dictionary<string, bool>()
            {
                { "_SHADOW_ON", shadow }
            });
        }

        public static Material CreateWalls(
            string name = null,
            Color? colour2 = null,
            Color? color0 = null,
            bool highlight = false,
            Texture2D overlay = null,
            bool textureByUV = false,
            bool varyByUV = false,
            float shininess = 0f,
            float overlayLowerBound = 0.6f,
            float overlayUpperBound = 0.9f,
            float overlayScale = 10f,
            float overlayMin = 0f,
            float overlayMax = 0.3f,
            Vector4? overlayOffset = null,
            Vector4? overlayTextureScale = null,
            float flatness = 0f)
        {
            bool hasTextureOverlay = overlay != null;
            return CreateMaterial("Walls", name, new Dictionary<string, object>()
            {
                { "_Colour2", colour2 ?? Color.black },
                { "_Color0", color0 ?? Color.black },
                { "_Highlight", highlight ? 1f : 0f },
                { "_Overlay", overlay },
                { "_TextureByUV", textureByUV ? 1f : 0f },
                { "_VaryByUV", varyByUV ? 1f : 0f },
                { "_HasTextureOverlay", hasTextureOverlay ? 1f : 0f },
                { "_Shininess", Mathf.Clamp01(shininess) },
                { "_OverlayLowerBound", Mathf.Clamp01(overlayLowerBound) },
                { "_OverlayUpperBound", Mathf.Clamp01(overlayUpperBound) },
                { "_OverlayScale", Mathf.Clamp(overlayScale, 0f, 1000f) },
                { "_OverlayMin", Mathf.Clamp01(overlayMin) },
                { "_OverlayMax", Mathf.Clamp01(overlayMax) },
                { "_OverlayOffset", overlayOffset ?? Vector4.zero },
                { "_OverlayTextureScale", overlayTextureScale ?? new Vector4(10f, 10f, 0f, 0f) },
                { "_Flatness", Mathf.Clamp01(flatness) }
            },
            new Dictionary<string, bool>()
            {
                { "_TEXTUREBYUV_ON", textureByUV },
                { "_VARYBYUV_ON", varyByUV },
                { "_HASTEXTUREOVERLAY_ON", hasTextureOverlay }
            });
        }

        public static Material CreateXPBadge(
            string name = null,
            Color? color0 = null)
        {
            return CreateMaterial("XP Badge", name, new Dictionary<string, object>()
            {
                { "_Color0", color0 ?? Color.black }
            });
        }
    }
}
