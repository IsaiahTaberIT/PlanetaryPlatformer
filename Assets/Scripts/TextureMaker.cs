using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static TextureMaker.LayerManager;
using static TextureMaker.LayerManager.TextureLayer;
using static UnityEngine.Mathf;
using System.IO;

[System.Serializable]
[ExecuteInEditMode]

public class TextureMaker : MonoBehaviour
{
    public string Name;
    public const int MaxRecursionDepth = 30;
    
    public const int BaseSize = 256;
    public Vector2Int Dimensions = new Vector2Int(BaseSize, BaseSize);
    [Min(0.01f)] public float Ratio = 1;
    
    private float _LastRatio;
    [Tooltip("Prevents aspect ratio from being changed when modifying dimensions, should be disabled when is being used as a child")]
    public bool LockRatio = false;


    private Vector2Int _OldDimensions = new Vector2Int(BaseSize, BaseSize);
    public Texture2D OutputTexture;
    public LayerManager Manager = new();
    public bool Regenerate = true;
    public int MaxResolution = 4096 * 4096;

    public TextureCompression CompressionLevel;
    public enum TextureCompression
    {
        Uncompressed = 0,
        Low = 1,
        High = 2,
    }




    [ContextMenu("Save As Asset")]
    void Save()
    {
        if (Name == null || Name == "")
        {
            Debug.LogWarning("Name Is required for saving");
            return;
        }

        byte[] bytes = OutputTexture.EncodeToPNG();
        string folder = "Assets/TextureMakerSprites";
        string fileName = Name + ".png";
        Directory.CreateDirectory(folder);
        string filePath = System.IO.Path.Combine(folder, fileName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Saved PNG at {filePath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }





    [ContextMenu("delete")]

    void DeleteTextures()
    {
        OutputTexture = null;
        Manager.texture = null;

        if (TryGetComponent(out SpriteRenderer renderer))
        {
            renderer.sprite = null;
        }

        for (int i = 0; i < Manager.TextureLayers.Count; i++)
        {
            Manager.TextureLayers[i].tex = null;
        }
    }
   
    void OnEnable()
    {
        if (Dimensions.ComponentProducts() * Manager.TextureLayers.Count < 20_000_000f)
        {
            GenerateAndApply();
        }
        else
        {
            Debug.Log("Object Is Too Large to Reasonably Initialize, Edit The Object To Regenerate It");
        }
       
    }

    public static Color RandomColor()
    {
        System.Random rng = new();
        return new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), 1);
    }

    [ContextMenu("add new layer")]
    void AddNewLayer()
    {
        Manager.TextureLayers.Add(new SimpleGradientGpu());
    }

    /// <summary>
    /// Generates the Output image using all TextureLayers in the LayerManager,
    /// and assigns a sprite with that image to the TextureMaker.
    /// For use when doing recursive calls where a TextureLayer contains a reference
    /// to another TextureMaker, prevents stack overflow in event of a reference loop
    /// </summary>
    public void GenerateAndApplySubCall(int depth)
    {
        if (depth > MaxRecursionDepth)
        {
            Debug.LogWarning($"Max Recursion Depth Reached! you either have an infinite loop, or a depth of {MaxRecursionDepth}", gameObject);
            return;
        }
        Manager._Depth = depth;
        GenerateAndApply();
    }

    /// <summary>
    /// Generates the Output image using all TextureLayers in the LayerManager,
    /// and assigns a sprite with that image to the TextureMaker.
    /// Not for internal use For SubMakers For that use GenerateAndApplySubCall() instead.
    /// This will prevent a stack overflow in certain cases.
    /// </summary>
    [ContextMenu("Generate and Apply")]
 
    public void GenerateAndApply()
    {
        if (!Regenerate)
        {
            return;
        }

        if (Min(Dimensions.x, Dimensions.y) < 4)
        {
            return;
        }

        Manager.Parent = gameObject;

        if (TryGetComponent(out SpriteRenderer renderer))
        {
            Ratio = Ratio > 100 ? 100 : Ratio;

            if (LockRatio)
            {
                if (_OldDimensions.x != Dimensions.x || Ratio != _LastRatio)
                {
                    Dimensions.y = (int)(Ratio * Dimensions.x);
                }
                else if (_OldDimensions.y != Dimensions.y)
                {
                    Dimensions.x = (int)(1.0f / Ratio * Dimensions.y);
                }
            }
            else
            {
                Ratio = (float)Dimensions.y / (float)Dimensions.x;
            }

            if (Dimensions.x < 4)
            {
                Dimensions.x = 4;
            }

            if (Dimensions.y < 4)
            {
                Dimensions.y = 4;
            }

            if (_OldDimensions.x * _OldDimensions.y > MaxResolution)
            {
                _OldDimensions = _OldDimensions / 2;
            }

            if (Dimensions.x * Dimensions.y > MaxResolution)
            {
                Dimensions = _OldDimensions;
            }

            int hardwareLimit = SystemInfo.maxTextureSize;
            Dimensions.x = Dimensions.x > hardwareLimit ? hardwareLimit : Dimensions.x;
            Dimensions.y = Dimensions.y > hardwareLimit ? hardwareLimit : Dimensions.y;


            _LastRatio = Ratio;
            _OldDimensions = Dimensions;

            Vector2Int roundedDims = new Vector2Int((int)Floor(Dimensions.x / 4f), (int)Floor(Dimensions.y / 4f)) * 4;

            Manager.Dimentions = roundedDims;
            Manager.GenerateTexture(true);
            OutputTexture = new(roundedDims.x, roundedDims.y);
            OutputTexture.hideFlags = HideFlags.DontSave;
            OutputTexture.filterMode = FilterMode.Point;
            RenderTexture.active = Manager.texture;
            OutputTexture.ReadPixels(new Rect(0, 0, OutputTexture.width, OutputTexture.height), 0, 0);
            OutputTexture.Apply();
            RenderTexture.active = null;
            // removing this may have caused a memory leak... if performace issues do occur, re-add and refactor
            //Manager.texture.Release();
            OutputTexture.Apply();

            if ((int)CompressionLevel != 0)
            {
                OutputTexture.Compress(Logic.IntToBool((int)CompressionLevel-1));
            }
       
            Rect rect = new Rect(new Vector2(0, 0), new Vector2(Manager.texture.width, Manager.texture.height));
            Sprite sprite = Sprite.Create(OutputTexture, rect, new Vector2(1, 1) / 2);
            sprite.name = "generated sprite";
            sprite.hideFlags = HideFlags.DontSave;
            renderer.sprite = sprite;
        }

        // this is a little wiggly because the children end up getting updated first resulting in them lagging behind the parent
        float ScaleRatio = transform.localScale.magnitude / transform.lossyScale.magnitude;
        transform.localScale = Vector3.one / new Vector2(Manager.texture.width, Manager.texture.height).magnitude * (181.02f) * ScaleRatio;




    }
    [System.Serializable]
    public class LayerManager
    {
        public int _Depth;
        public Vector2Int Dimentions = new Vector2Int(BaseSize, BaseSize);
        [SerializeReference] public List<TextureLayer> TextureLayers = new();
        public RenderTexture texture;
        [HideInInspector] public GameObject Parent;
        public void CreateNewLayer()
        {
            //updating to create semi random layer

           
            System.Random r = new System.Random();
            int rand = r.Next(4);

            TextureLayer layer;

            switch (rand)
            {
                case 0:
                    layer = new SimpleGradientGpu();
                    break;
                case 1:
                    layer = new StripesGpu(Parent);
                    break;
                case 2:
                    layer = new PolygonGpu();
                    break;
                case 3:
                    layer = new NoiseGpu();
                    break;
                case 4:
                    layer = new RadialGradientGpu();
                    break;
                default:
                    layer = new SimpleGradientGpu();
                    break;

            }

            TextureLayers.Add(layer);
        }

        [ContextMenu("create Texture")]

        public void GenerateTexture(bool Reset)
        {
            if (Min(Dimentions.x, Dimentions.y) < 2)
            {
                return;
            }

            if (texture == null || Reset)
            {
                if (Reset && texture != null)
                {
                    texture.Release();
                }


                texture = new RenderTexture(Dimentions.x, Dimentions.y, 0, RenderTextureFormat.ARGBFloat);
                texture.hideFlags = HideFlags.DontSave;
                texture.enableRandomWrite = true;
                texture.Create();

            }

            for (int i = 0; i < TextureLayers.Count; i++)
            {

                if (TextureLayers[i] == null)
                {
                    TextureLayers.RemoveAt(i);
                }

                TextureLayers[i].depth = _Depth;


                if (TextureLayers[i] is ISubmaker sub)
                {
                    if (sub.ParentSM == null)
                    {
                        sub.ParentSM = Parent;
                    }


                    if (sub is IDistortionSubMaker subdis)
                    {
                        if (subdis.DistortionSM.Parent == null)
                        {
                            (subdis).DistortionSM.Parent = Parent;
                        }

                    }

                    if (sub is IReplaceColorSubMaker subreplace)
                    {
                        if (subreplace.ReplaceColorSM.Parent == null)
                        {
                            (subreplace).ReplaceColorSM.Parent = Parent;
                        }

                    }

                    if (sub is ISecondarySubMaker subsec)
                    {
                        if (subsec.SecondarySM.Parent == null)
                        {
                            (subsec).SecondarySM.Parent = Parent;
                        }

                    }

                    if (sub is IMaskSubMaker submask)
                    {
                        if (submask.MaskSM.Parent == null)
                        {
                            (submask).MaskSM.Parent = Parent;
                        }

                    }

                }

                if (TextureLayers[i].Type != TextureLayers[i].LastType)
                {
                    if (TextureLayers[i].LastType == TextureLayerType.composite)
                    {
                        if (TextureLayers[i] is CompositeGpu && (TextureLayers[i] as CompositeGpu).MakerObject.TryGetComponent(out TextureMaker _))
                        {
                            GameObject.DestroyImmediate((TextureLayers[i] as CompositeGpu).MakerObject);
                        }
                    }
                    TextureLayers[i] = ReplaceLayer(TextureLayers[i].Type);
                }

                if (TextureLayers[i] is Filter)
                {
                    Filter filter = TextureLayers[i] as Filter;

                    if (filter.FilterType != filter.LastFilterType)
                    {
                        TextureLayers[i] = ReplaceFilter(filter.FilterType);
                    }

                }

                if (TextureLayers[i].Enabled)
                {
                    TextureLayers[i].tex = texture;
                    texture = TextureLayers[i].OutTex;
                }
                else
                {
                    continue;
                }

            }
        }

        public class ContrastFilterGpu : Filter
        {
            [Range(-1f, 1f)] public float Contrast = 0;

            public ContrastFilterGpu()
            {
                FilterType = FilterTypes.contrast;
                LastFilterType = FilterType;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);
                computeshader.SetFloat("Contrast", Pow((Pow(Contrast * 5f, 2)) + 1f, Sign(Contrast)));

            }
            public override RenderTexture Generate()
            {
                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("ContrastShader");
                }

                RenderTexture rt = tex;


                ApplyShaderToRT(rt);

                return tex;
            }
        }
        public class QuantizationFilterGpu : Filter
        {
            public QuantizationModes QuantizationMode = 0;

            [Min(1)] public float QuantizeFactor = 5;
            public enum QuantizationModes
            {
                ByColor = 0,
                ByValue = 1,
                ByPosition = 2,
            }
            public QuantizationFilterGpu()
            {
                FilterType = FilterTypes.quantization;
                LastFilterType = FilterType;
            }
            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                float floorscale = Floor(QuantizeFactor);


                base.PassValuesToShader(rt, kernel);

                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetFloat("Scale", floorscale);

                computeshader.SetInt("QuantizationMode", (int)QuantizationMode);

            }
            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("QuantizationShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;
            }


        }

        public class TransparencyFilterGpu : Filter
        {
            public BlendModes BlendMode = BlendModes.RotLerp;
            public bool Additive;
            [Range(0, 1f)] public float Alpha = 0;

            public TransparencyFilterGpu()
            {
                FilterType = FilterTypes.transparency;
                LastFilterType = FilterType;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);
                computeshader.SetInt("Additive", Logic.BoolToInt(Additive));
                computeshader.SetFloat("Alpha", Alpha);
                computeshader.SetInt("BlendMode", (int)BlendMode);
            }
            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("TransparencyShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;
            }


        }
        public class HueShiftFilterGpu : Filter
        {

            public float Shift = 0;
            public BlendModes BlendMode = BlendModes.RotLerp;

            public HueShiftFilterGpu()
            {
                FilterType = FilterTypes.hueshift;
                LastFilterType = FilterType;
            }
            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);

                computeshader.SetFloat("Shift", Abs(Shift) / 10f + 1);
                computeshader.SetInt("BlendMode", (int)BlendMode);
            }
            public override RenderTexture Generate()
            {
                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("HueShiftShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;

            }
        }
        public class SaturationFilterGpu : Filter
        {
            [Tooltip("So like... you can change this, it will Respond... but I'm only letting you do it because I have to reason to prevent you from doing it")]
            public BlendModes BlendMode = BlendModes.RotLerp;

            [Range(0, 1f)] public float Saturation = 0;

            public SaturationFilterGpu()
            {
                FilterType = FilterTypes.saturation;
                LastFilterType = FilterType;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);

                computeshader.SetFloat("Saturation", Saturation);
                computeshader.SetInt("BlendMode", (int)BlendMode);
            }
            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("SaturationShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;
            }

        }

        [System.Serializable]
        public class Filter : TextureLayer
        {

            public FilterTypes FilterType;
            [HideInInspector] public FilterTypes LastFilterType;

            public enum FilterTypes
            {
                saturation = 0,
                contrast = 1,
                transparency = 2,
                hueshift = 3,
                rotatecolor = 4,
                quantization = 5,

            }
            public Filter()
            {
                Type = TextureLayerType.filter;
                LastType = Type;
            }
        }

        [System.Serializable]
        public class CustomMaskGPU : TextureLayer, ISecondarySubMaker, IMaskSubMaker
        {
            public MaskModes MaskMode;
            public bool Invert;

            [Range(0f, 1f)] public float Threshold = 0.5f;
            [HideInInspector] public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

            public SubMaker mask = new("Mask Maker");
            public SubMaker MaskSM { get => mask; set => mask = value; }

            public SubMaker secondary = new("Secondary Maker");
            public SubMaker SecondarySM { get => secondary; set => secondary = value; }

            public enum MaskModes
            {

                byValue = 0,
                byValueThreshold = 1,
                byColor = 2,
                byHue = 3,
            }

            public BlendModes BlendMode = BlendModes.RotLerp;
            public Color ComparisonColor = RandomColor();

            public CustomMaskGPU()
            {
                Type = TextureLayerType.customMask;
                LastType = Type;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);
                computeshader.SetFloat("Threshold", Threshold);
                computeshader.SetInt("Invert", Logic.BoolToInt(Invert));
                computeshader.SetInt("MaskMode", (int)MaskMode);
                computeshader.SetVector("ComparisonColor", ComparisonColor);
                mask.Dims = new(tex.width, tex.height);
                computeshader.SetTexture(kernel, "MaskTex", mask.Generate(depth));
                secondary.Dims = new(tex.width, tex.height);
                computeshader.SetTexture(kernel, "SecondaryInputTexture", secondary.Generate(depth));
                computeshader.SetInt("BlendMode", (int)BlendMode);

            }

            public override RenderTexture Generate()
            {
                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("CustomMaskShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;

            }
        }

        [System.Serializable]
        public class SolidGpu : TextureLayer
        {
            public BlendModes BlendMode = BlendModes.RotLerp;
            public Color Color = RandomColor();
            public SolidGpu()
            {
                Type = TextureLayerType.solid;
                LastType = Type;

            }
            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("Color", Color);

            }

            // i have no idea btw if this is a good way of doing things it does make it really nice to add stuff though
            // im refering to the roundabout logic with the parent versions of stuff to isolate only the middle 
            //portion to be changed for every shader that being this method:  PassValuesToShader(RenderTexture rt, int kernel)
            // i haven't yet read any books on "clean code" or stuff like that... which is most likely obvious
            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("SolidShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;

            }

        }

        [System.Serializable]
        public class PolygonGpu : TextureLayer, IDistortionSubMaker, IReplaceColorSubMaker
        {
            [HideInInspector] public GameObject Parent;
            public BlendDirections BlendDirection = BlendDirections.None;
       
            [Min(0.1f)] public float BlendDistance = 0.1f;
            public float Power = 1f;

            public GameObject ParentSM { get => Parent; set => Parent = value; }
            public DistortionMaker distortion = new("Distortion Maker");
            public DistortionMaker DistortionSM { get => distortion; set => distortion = value; }

            public ReplaceColorSubMaker secondary = new("Secondary Maker");
            public ReplaceColorSubMaker ReplaceColorSM { get => secondary; set => secondary = value; }

            public ShapeGenerator.Shapes shape = new();
            public Vector2 Offset = new Vector2(0.5f, 0.5f);
            ComputeBuffer VertsBuffer;
            public Vector2[] Verts;
            public BlendModes BlendMode = BlendModes.RotLerp;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();

            public enum BlendDirections
            {
                None = 0,
                In = 1,
                Out = 2,
                Both = 3,
            }

            public PolygonGpu()
            {
                Type = TextureLayerType.polygon;
                LastType = Type;

            }
            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                distortion.Parent = Parent;
                secondary.Parent = Parent;

                distortion.Dims = new(tex.width, tex.height);
                computeshader.SetTexture(kernel, "DistortionTex", distortion.Generate(depth));

                secondary.Dims = new(tex.width, tex.height);
                computeshader.SetTexture(kernel, "SecondaryInputTexture", secondary.Generate(depth));


                computeshader.SetInt("OverrideWithMask", Logic.BoolToInt(secondary.OverrideWithMask));
                computeshader.SetInt("OverridePrimaryColor", Logic.BoolToInt(secondary.OverridePrimaryColor));

                Vector2 dims = new Vector2(tex.width, tex.height);
                float rootmagnitude = Sqrt(dims.magnitude);
                distortion.Influence = distortion.Maker == null ? 0 : distortion.Influence;
                computeshader.SetFloat("DistortionFactor", distortion.Influence * rootmagnitude / BaseSize);

                computeshader.SetFloat("Power", Power);

                computeshader.SetFloat("BlendDistance",32f / BlendDistance);

                computeshader.SetInt("BlendDirection", (int)BlendDirection);

                shape.GenerateCirclePoints();
                float ratioY = (float)tex.height / (float)tex.width;
                float ratioX = 1 / ratioY;

                if (ratioY >= 1)
                {
                    ratioY = 1;
                }

                if (ratioX >= 1)
                {
                    ratioX = 1;
                }

                Vector2 screenSpaceOffset = new Vector2(Offset.x * ratioX, ratioY * Offset.y);

                Verts = shape.Verts.ToVector2(screenSpaceOffset);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                VertsBuffer = new ComputeBuffer(Verts.Length, sizeof(float) * 2);
                VertsBuffer.SetData(Verts);
                computeshader.SetBuffer(kernel, "Verts", VertsBuffer);
                computeshader.SetInt("VertCount", Verts.Length);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetVector("SecondaryColor", SecondaryColor);
                base.PassValuesToShader(rt, kernel);
            }
            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("PolygonShader");
                }
                RenderTexture rt = tex;
                ApplyShaderToRT(rt);
                VertsBuffer.Dispose();

                return tex;

            }
        }

        [System.Serializable]
        public class StripesGpu : TextureLayer, IDistortionSubMaker, IReplaceColorSubMaker
        {
          
            public bool Fuzzy;
            [Min(0.01f)] public float BlendPower = 1;
            [HideInInspector] public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

            public ReplaceColorSubMaker secondary = new("Secondary Maker");
            public DistortionMaker DistortionSM { get => distortion; set => distortion = value; }
            public DistortionMaker distortion = new("Distortion Maker");
            public ReplaceColorSubMaker ReplaceColorSM { get => secondary; set => secondary = value; }
            public BlendModes BlendMode = BlendModes.RotLerp;
            public BlendModes GradientBlendMode = BlendModes.RotLerp;
            public float Angle;
            public float Spacing = 40;
            [Min(0f)] public float Offset;
            private Vector2 Slope => Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * Vector2.up;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();

          
            public StripesGpu(GameObject parent)
            {
                distortion.Parent = parent;
                Type = TextureLayerType.stripes;
                LastType = Type;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                distortion.Parent = Parent;
                distortion.Dims = new(tex.width, tex.height);
                RenderTexture temp1 = distortion.Generate(depth);

                secondary.Parent = Parent;
                secondary.Dims = new(tex.width, tex.height);
                RenderTexture temp2 = secondary.Generate(depth);

                computeshader.SetTexture(kernel, "DistortionTex", temp1);
                computeshader.SetTexture(kernel, "SecondaryInputTexture", temp2);

                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("Fuzzy", Logic.BoolToInt(Fuzzy));
                computeshader.SetInt("OverrideWithMask", Logic.BoolToInt(secondary.OverrideWithMask));
                computeshader.SetInt("OverridePrimaryColor", Logic.BoolToInt(secondary.OverridePrimaryColor));
                computeshader.SetFloat("BlendPower", BlendPower);

                base.PassValuesToShader(rt, kernel);
                Vector2 dims = new Vector2(tex.width, tex.height);
                float rootmagnitude = Mathf.Sqrt(dims.magnitude);
                distortion.Influence = distortion.Maker == null ? 0 : distortion.Influence;
                computeshader.SetFloat("DistortionFactor", distortion.Influence * rootmagnitude / BaseSize);
                computeshader.SetFloat("Offset", Offset * dims.magnitude / (BaseSize / 2));
                computeshader.SetVector("Dims", dims);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("Slope", Slope);
                computeshader.SetFloat("Spacing", Spacing * Max(tex.width, tex.height) / BaseSize);
                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetVector("SecondaryColor", SecondaryColor);
            }

            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("StripesShader");
                }

                RenderTexture rt = tex;
                ApplyShaderToRT(rt);

                return tex;
            }
        }

        [System.Serializable]
        public class ColorRotateGpu : Filter
        {

            public float Degrees;
            public Vector3 Axis = new Vector3(0, 0, 1);
            public Vector3 Pivot = Vector3.zero;
            public ColorRotateGpu()
            {
                FilterType = FilterTypes.rotatecolor;
                LastFilterType = FilterType;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);

                computeshader.SetVector("Pivot", Pivot);
                computeshader.SetVector("Axis", Axis);
                computeshader.SetFloat("Shift", Degrees * Deg2Rad);

            }

            public override RenderTexture Generate()
            {

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("ColorRotateShader");
                }

                RenderTexture rt = tex;

                ApplyShaderToRT(rt);

                return tex;
            }
        }

        [System.Serializable]
        public class NoiseGpu : TextureLayer
        {
            public bool FeedInSeedNoise = false;
            public bool CoherentNoise = false;
            public BlendModes BlendMode = (BlendModes)1;
            public BlendModes NoiseBlendMode = (BlendModes)1;

            [Range(0f, 1f)] public float Alpha = 1f;
            [Min(0.01f)] public float Scale = 1;
            [Range(1, 3)] public int NoiseDimensions = 1;
            public Color PrimaryColor = Color.black;
            public Color SecondaryColor = Color.white;

            [Min(1)] public uint Seed = 100;
            [Min(0.01f)] public float SmoothingPower = 3f;

            public NoiseGpu(uint seed)
            {
                Color BaseColor = Color.red;
                CoherentNoise = false;
                NoiseDimensions = 2;
                Seed = seed;
                Scale = 1;
                Alpha = 1f;
                BlendMode = (BlendModes)1;
                Type = TextureLayerType.noise;
                LastType = Type;
                SmoothingPower = 3;

            }
            public NoiseGpu()
            {
                BlendMode = (BlendModes)1;
                Type = TextureLayerType.noise;
                LastType = Type;
            }

            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {
                // i spent a full day on an issue with these sub-layers/makers that was caused by
                // wrtiting to a computeshader, then calling a function that overwites your changes to that compute shader
                // because it used the same shader to generate a texture, which results in a partially filled shader 
                // all i had to do was reorder a base.PassValuesToShader() call to be later, 
                // that was a good example of shat-gpt not being able to solve a problem for you
                // it was mystified
                //Scale = (CoherentNoise && Scale <= 2) ? 2 : Scale;

                float floorscale = 0;

                if (CoherentNoise)
                {
                    floorscale = Scale * Max(tex.width, tex.height) / 100;
                    floorscale = Floor(floorscale);

                    if (floorscale < 2f)
                    {
                        floorscale = 2;
                    }
                }
                else
                {
                    Scale = (Scale < 1) ? 1 : Scale;
                    floorscale = Floor(Scale);

                }

                RenderTexture inputTexture;

                if (FeedInSeedNoise)
                {
                    inputTexture = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat);

                }
                else
                {
                    Vector2Int SeednoiseDims = new Vector2Int((int)Floor(tex.width / floorscale), (int)Floor(tex.height / floorscale));
                    SeednoiseDims += Vector2Int.one;
                    inputTexture = new RenderTexture(SeednoiseDims.x, SeednoiseDims.y, 0, RenderTextureFormat.ARGBFloat);

                }

                inputTexture.enableRandomWrite = true;
                inputTexture.Create();

                if (CoherentNoise && !FeedInSeedNoise)
                {
                    NoiseGpu SeedNoise = new(Seed);
                    SeedNoise.tex = inputTexture;
                    SeedNoise.Generate();
                }
                else
                {
                    Graphics.Blit(tex, inputTexture);
                }

                computeshader.SetTexture(kernel, "Noise", inputTexture);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetVector("SecondaryColor", SecondaryColor);
                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetFloat("NoiseDimensions", NoiseDimensions);
                computeshader.SetFloat("Power", SmoothingPower);
                computeshader.SetInt("UseCoherentNoise", Logic.BoolToInt(CoherentNoise));
                computeshader.SetFloat("Scale", floorscale);
                computeshader.SetFloat("Alpha", Alpha);
                computeshader.SetFloat("Seed", Seed);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetInt("NoiseBlendMode", (int)NoiseBlendMode);
                base.PassValuesToShader(Mainrt, kernel);
            }
            public override RenderTexture Generate()
            {
                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("NoiseShader");
                }
                RenderTexture rt = tex;
                rt.Create();
                ApplyShaderToRT(rt);
                return tex;
            }

        }
        [System.Serializable]
        public class CompositeGpu : TextureLayer
        {
            public bool PassInBackgound = false;
            public BlendModes BlendMode = (BlendModes)1;
            [HideInInspector] public GameObject Parent;
            public GameObject MakerObject;
            private TextureMaker Maker;
            [Range(0f, 1f)] public float Opacity;

            public CompositeGpu(GameObject parent, string name)
            {
                Parent = parent;
                MakerObject = new(name, new System.Type[] { typeof(SpriteRenderer), typeof(TextureMaker) });
                MakerObject.transform.SetParent(parent.transform);
                Type = TextureLayerType.composite;
                LastType = Type;
                Maker = MakerObject.GetComponent<TextureMaker>();
            }

            public override RenderTexture Generate()
            {

                RenderTexture inputTexture = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat);
                inputTexture.enableRandomWrite = true;
                Graphics.Blit(tex, inputTexture);
                inputTexture.Create();

                if (Maker == null)
                {
                    Maker = MakerObject.GetComponent<TextureMaker>();
                }

                if (Maker.Manager.Parent == null)
                {
                    Maker.Manager.Parent = MakerObject;
                }

                Maker.Manager.Dimentions = new(tex.width, tex.height);
                Maker.Dimensions = Maker.Manager.Dimentions;

                Maker.Manager.texture = tex;
                Maker.Manager.GenerateTexture(!PassInBackgound);
                tex = Maker.Manager.texture;

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("SimpleBlendShader");
                }

                int width = tex.width;
                int height = tex.height;

                int kernel = computeshader.FindKernel("CSMain");

                computeshader.SetTexture(kernel, "InputTex", inputTexture);
                computeshader.SetTexture(kernel, "Result", tex);

                int threadGroupsX = CeilToInt(width / 8f);
                int threadGroupsY = CeilToInt(height / 8f);

                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetFloat("Opacity", Opacity);

                computeshader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

                return tex;
            }
        }

        [System.Serializable]
        public class RadialGradientGpu : TextureLayer, IDistortionSubMaker
        {
            [HideInInspector] public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }
            public DistortionMaker distortion = new("Distortion Maker");
            public DistortionMaker DistortionSM { get => distortion; set => distortion = value; }

            public BlendModes BlendMode = BlendModes.RotLerp;
            public BlendModes GradientBlendMode = BlendModes.RotLerp;
            [Range(1f, 256f)] public float Blend;
            public Vector2 Center = Vector2.one / 2f;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();
            public float Radius = 10;

            public RadialGradientGpu()
            {
                Type = TextureLayerType.radialGradient;
                LastType = Type;
                Blend = 1;
            }

            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {

                distortion.Parent = Parent;

                distortion.Dims = new(tex.width, tex.height);
                distortion.Generate(depth);
                base.PassValuesToShader(Mainrt, kernel);
                computeshader.SetTexture(kernel, "DistortionTex", distortion.SubTex);
                Vector2 dims = new Vector2(tex.width, tex.height);
                float rootmagnitude = Sqrt(dims.magnitude);
                distortion.Influence = distortion.Maker == null ? 0 : distortion.Influence;
                computeshader.SetFloat("DistortionFactor", distortion.Influence * rootmagnitude / BaseSize);
                computeshader.SetFloat("Radius", Radius * dims.magnitude / BaseSize);
                computeshader.SetVector("Dims", dims);
                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetFloat("BlendPower", Sqrt(Blend));
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetVector("Center", Center);
                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetVector("SecondaryColor", SecondaryColor);
            }
            public override RenderTexture Generate()
            {
                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("RadialGradientShader");
                }
                RenderTexture rt = tex;
                rt.Create();
                ApplyShaderToRT(rt);
                return tex;
            }

        }

        [System.Serializable]
        public class SimpleGradientGpu : TextureLayer, IDistortionSubMaker
        {

            [HideInInspector] public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

            public DistortionMaker distortion = new("Distortion Maker");
            public DistortionMaker DistortionSM { get => distortion; set => distortion = value; }

            public BlendModes BlendMode = BlendModes.RotLerp;
            public BlendModes GradientBlendMode = BlendModes.RotLerp;
            public float Angle;
            private Vector2 Slope => Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * Vector2.up;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();

            public SimpleGradientGpu()
            {
                Type = TextureLayerType.gradient;
                LastType = Type;
            }

            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {
                distortion.Parent = Parent;
                distortion.Dims = new(tex.width, tex.height);
                distortion.Generate(depth);
                base.PassValuesToShader(Mainrt, kernel);

                Vector2 dims = new Vector2(tex.width, tex.height);
                float rootmagnitude = Sqrt(dims.magnitude);
                distortion.Influence = distortion.Maker == null ? 0 : distortion.Influence;
                computeshader.SetFloat("DistortionFactor", distortion.Influence * rootmagnitude / BaseSize);
                computeshader.SetTexture(kernel, "DistortionTex", distortion.SubTex);

                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("Slope", Slope);
                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetVector("SecondaryColor", SecondaryColor);
            }
            public override RenderTexture Generate()
            {
                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("SimpleGradientShader");
                }
                RenderTexture rt = tex;
                rt.Create();
                ApplyShaderToRT(rt);
                return tex;

            }

        }

        [System.Serializable]
        public class BumpyCircleGpu : TextureLayer, IDistortionSubMaker
        {

            [HideInInspector] public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

            public DistortionMaker distortion = new("Distortion Maker");
            public DistortionMaker DistortionSM { get => distortion; set => distortion = value; }

            public BlendModes BlendMode = BlendModes.RotLerp;

            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();

            public List<Octave> Octaves = new();
            public Vector2 Center = Vector2.one / 2f;
            public float Radius = 0.5f;

            static Octave Default = new(false, 0, 0, 10, 1, 6);
            public BumpyCircleGpu()
            {

                Type = TextureLayerType.bumpyCircle;
                LastType = Type;
            }

            [System.Serializable]
            public struct Octave
            {
                [HideInInspector] public bool Initialized;
                public bool Abs;
                public float offset;
                [Range(-1f, 1f)] public float constant;
                public float magnitude;
                public float power;
                public float spacing;

                public Octave(bool _abs, float _offset, float _constant, float _magnitude, float _power, float _spacing)
                {
                    Initialized = true;
                    Abs = _abs;
                    offset = _offset;
                    constant = _constant;
                    magnitude = _magnitude;
                    power = _power;
                    spacing = _spacing;
                }
            }

            void ApplyOctavesAndGenerate(int depth)
            {


                distortion.Parent = Parent;
                distortion.Dims = new(tex.width, tex.height);
                distortion.Generate(depth);

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("BumpyCircleShader");
                }

                Vector2 dims = new Vector2(tex.width, tex.height);
                RenderTexture rt = tex;
                int kernel = computeshader.FindKernel("CSMain");
                computeshader.SetTexture(kernel, "Result", rt);

                int threadGroupsX = CeilToInt(dims.x / 8f);
                int threadGroupsY = CeilToInt(dims.y / 8f);

                int[] abs = new int[20];
                float[] offset = new float[20];
                float[] constant = new float[20];
                float[] magnitude = new float[20];
                float[] power = new float[20];
                float[] spacings = new float[20];

                for (int i = 0; i < Octaves.Count; i++)
                {
                    if (!Octaves[i].Initialized)
                    {
                        Octaves[i] = Default;
                    }
                    abs[i] = Logic.BoolToInt(Octaves[i].Abs);
                    offset[i] = Octaves[i].offset;
                    constant[i] = Octaves[i].constant;
                    magnitude[i] = Octaves[i].magnitude * dims.magnitude / BaseSize;
                    power[i] = Octaves[i].power;
                    spacings[i] = Octaves[i].spacing;
                }

                float rootmagnitude = Sqrt(dims.magnitude);
                distortion.Influence = distortion.Maker == null ? 0 : distortion.Influence;
                computeshader.SetFloat("DistortionFactor", distortion.Influence * rootmagnitude / BaseSize);
                computeshader.SetTexture(kernel, "DistortionTex", distortion.SubTex);

                // this is another example of asking chat-gpt how to do something and the reusing it once you know how
                // still really annoys me that setFloats doesnt actually set an array of floats...

                // Create and set ComputeBuffers
                ComputeBuffer offsetBuffer = new ComputeBuffer(offset.Length, sizeof(float));
                offsetBuffer.SetData(offset);
                computeshader.SetBuffer(kernel, "Offsets", offsetBuffer);

                ComputeBuffer powerBuffer = new ComputeBuffer(power.Length, sizeof(float));
                powerBuffer.SetData(power);
                computeshader.SetBuffer(kernel, "Powers", powerBuffer);

                ComputeBuffer magnitudeBuffer = new ComputeBuffer(magnitude.Length, sizeof(float));
                magnitudeBuffer.SetData(magnitude);
                computeshader.SetBuffer(kernel, "Amplitudes", magnitudeBuffer);

                ComputeBuffer spacingBuffer = new ComputeBuffer(spacings.Length, sizeof(float));
                spacingBuffer.SetData(spacings);
                computeshader.SetBuffer(kernel, "Rates", spacingBuffer);

                ComputeBuffer constantBuffer = new ComputeBuffer(constant.Length, sizeof(float));
                constantBuffer.SetData(constant);
                computeshader.SetBuffer(kernel, "Constants", constantBuffer);

                // For UseAbsbools, use uints instead of bools for compatibility
                uint[] absUInt = abs.Select(x => (uint)x).ToArray();
                ComputeBuffer absBuffer = new ComputeBuffer(absUInt.Length, sizeof(uint));
                absBuffer.SetData(absUInt);
                computeshader.SetBuffer(kernel, "UseAbsbools", absBuffer);

                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("Center", Center * dims);
                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetVector("SecondaryColor", SecondaryColor);
                computeshader.SetInt("ArrayLength", Octaves.Count);
                computeshader.SetFloat("Radius", Radius * dims.magnitude / BaseSize);

                computeshader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

                offsetBuffer.Dispose();
                powerBuffer.Dispose();
                magnitudeBuffer.Dispose();
                absBuffer.Dispose();
                constantBuffer.Dispose();
                spacingBuffer.Dispose();
            }
            public override RenderTexture Generate()
            {
                ApplyOctavesAndGenerate(depth);

                return tex;
            }
        }
        public interface ISubmaker
        {
            GameObject ParentSM { get; set; }

        }
        public interface IDistortionSubMaker : ISubmaker
        {
            DistortionMaker DistortionSM { get; set; }
        }
        public interface IMaskSubMaker : ISubmaker
        {
            SubMaker MaskSM { get; set; }
        }
        public interface IReplaceColorSubMaker : ISubmaker
        {
            ReplaceColorSubMaker ReplaceColorSM { get; set; }
        }
        public interface ISecondarySubMaker : ISubmaker
        {
            SubMaker SecondarySM { get; set; }
        }

        [System.Serializable]
        public class ReplaceColorSubMaker : SubMaker
        {

            public bool OverrideWithMask;
            public bool OverridePrimaryColor;

            public ReplaceColorSubMaker(string name)
            {
                Name = name;
            }

        }

        [System.Serializable]
        public class DistortionMaker : SubMaker
        {
            public float Influence;

            public DistortionMaker(string name)
            {
                Name = name;
            }

        }

        [System.Serializable]
        public class SubMaker
        {
            public string Name;


            [HideInInspector] public Vector2Int Dims = Vector2Int.one;
            [HideInInspector] public RenderTexture SubTex;
            public GameObject SubTextureMaker;
            [HideInInspector] public GameObject Parent;
            [HideInInspector] public TextureMaker Maker;

            public SubMaker()
            {

            }
            public SubMaker(string name)
            {
                Name = name;
            }

            public RenderTexture Generate(int depth)
            {
                if (SubTextureMaker == null)
                {
                    Maker = null;
                }

                if (SubTex != null)
                {
                    SubTex.Release();
                }

                SubTex = new RenderTexture(Dims.x, Dims.y, 0, RenderTextureFormat.ARGBFloat);
                SubTex.enableRandomWrite = true;
                SubTex.Create();

                if (SubTextureMaker != null)
                {
                    SubTextureMaker.TryGetComponent(out Maker);

                }

                if (Maker != null)
                {
                    if (Maker.Manager.Parent == null)
                    {
                        Maker.Manager.Parent = Parent;
                    }

                    Maker.Dimensions = new Vector2Int(SubTex.width, SubTex.height);
                    Maker.GenerateAndApplySubCall(depth + 1);

                    if (Maker.Manager != null)
                    {
                        SubTex = Maker.Manager.texture;
                    }
                }

                return SubTex;
            }
            public void CreateTextureMaker()
            {
                if (SubTextureMaker == null)
                {
                    if (Parent == null)
                    {
                        Debug.Log("Parent");


                        return;
                    }

                    SubTextureMaker = new(Name, new System.Type[] { typeof(SpriteRenderer), typeof(TextureMaker) });

                    if (SubTextureMaker.TryGetComponent(out SpriteRenderer s))
                    {
                        Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

                        if (unlitShader != null)
                        {
                            s.material = new(unlitShader);
                        }
                    }

                    SubTextureMaker.transform.SetParent(Parent.transform);
                    SubTextureMaker.transform.localPosition = new Vector3(1, 0, 0);
                    Maker = SubTextureMaker.GetComponent<TextureMaker>();
                }
            }
        }

        [System.Serializable]
        public class TextureLayer
        {
            [HideInInspector] public int depth;
            public bool Enabled = true;
            [HideInInspector] public LayerManager ManagerRef;
            public RenderTexture OutTex => Generate();
            [HideInInspector] public TextureLayerType LastType;
            public TextureLayerType Type;
            public ComputeShader computeshader;
            public RenderTexture tex;
            public enum BlendModes
            {
                Lerp = 0,
                RotLerp = 1,
                Multiply = 2,
                Darken = 3,
                Lighten = 4,
                Addition = 5,
                Subtraction = 6,
                Overwrite = 7,
            }
            public enum TextureLayerType
            {
                solid = 0,
                gradient = 1,
                stripes = 2,
                composite = 3,
                bumpyCircle = 4,
                filter = 5,
                radialGradient = 6,
                noise = 7,
                polygon = 8,
                customMask = 9,
            }
            public virtual void PassValuesToShader(RenderTexture rt, int kernel)
            {
                computeshader.SetTexture(kernel, "Result", rt);
            }
            public void ApplyShaderToRT(RenderTexture rt)
            {
                int threadGroupsX = CeilToInt(tex.width / 8f);
                int threadGroupsY = CeilToInt(tex.height / 8f);

                int kernel = computeshader.FindKernel("CSMain");

                PassValuesToShader(rt, kernel);

                computeshader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            }
            public RenderTexture CreateRTfromTexture(Texture tex)
            {
                RenderTexture rt = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat);
                rt.enableRandomWrite = true;
                Graphics.Blit(tex, rt);
                rt.Create();
                return rt;
            }

     
            public virtual RenderTexture Generate()
            {
                return tex;
            }
        }
        public TextureLayer ReplaceFilter(Filter.FilterTypes type)
        {
            switch (type)
            {
                case Filter.FilterTypes.saturation:
                    return new SaturationFilterGpu();
                case Filter.FilterTypes.contrast:
                    return new ContrastFilterGpu();
                case Filter.FilterTypes.hueshift:
                    return new HueShiftFilterGpu();
                case Filter.FilterTypes.transparency:
                    return new TransparencyFilterGpu();
                case Filter.FilterTypes.rotatecolor:
                    return new ColorRotateGpu();
                case Filter.FilterTypes.quantization:
                    return new QuantizationFilterGpu();
                default:
                    return new SaturationFilterGpu();
            }
        }
        public TextureLayer ReplaceLayer(TextureLayer.TextureLayerType type)
        {
            switch (type)
            {
                case TextureLayer.TextureLayerType.bumpyCircle:
                    return new BumpyCircleGpu();
                case TextureLayer.TextureLayerType.gradient:
                    return new SimpleGradientGpu();
                case TextureLayer.TextureLayerType.stripes:
                    return new StripesGpu(Parent);
                case TextureLayer.TextureLayerType.composite:
                    CompositeGpu composite = new CompositeGpu(Parent, "SubMaker");
                    return composite;
                case TextureLayer.TextureLayerType.solid:
                    return new SolidGpu();
                case TextureLayer.TextureLayerType.filter:
                    return new SaturationFilterGpu();
                case TextureLayer.TextureLayerType.radialGradient:
                    return new RadialGradientGpu();
                case TextureLayer.TextureLayerType.noise:
                    return new NoiseGpu();
                case TextureLayer.TextureLayerType.polygon:
                    return new PolygonGpu();
                case TextureLayer.TextureLayerType.customMask:
                    return new CustomMaskGPU();
                default:
                    return new SimpleGradientGpu();
            }
        }

     
    }
}
