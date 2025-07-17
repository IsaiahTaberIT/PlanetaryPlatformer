using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static TextureMaker.LayerManager;

using static TextureMaker.LayerManager.TextureLayer;
using static UnityEngine.Mathf;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;




[System.Serializable]

public class TextureMaker : MonoBehaviour
{
    public Vector2Int Dimentions = new Vector2Int(256,256);
    public Texture2D OutputTexture;


    public LayerManager Manager = new();


    public static Color RandomColor()
    {
        System.Random rng = new ();
        return new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(),1);
    }



   
    [ContextMenu("add new layer")]
    void AddNewLayer()
    {
        Manager.TextureLayers.Add(new SimpleGradientGpu());
    }


    [ContextMenu("Generate and Apply")]
    public void GenerateAndApply()
    {
        if (Min(Dimentions.x, Dimentions.y) < 2)
        {
            return;
        }

        Manager.Parent = gameObject;
        //Debug.Log("applied");
     

        if (TryGetComponent(out SpriteRenderer renderer))
        {
            Manager.Dimentions = Dimentions;
            Manager.GenerateTexture(true);

            OutputTexture = new(Dimentions.x,Dimentions.y);

            RenderTexture.active = Manager.texture;
            OutputTexture.ReadPixels(new Rect(0, 0, OutputTexture.width, OutputTexture.height), 0, 0);
            OutputTexture.Apply();
            RenderTexture.active = null;
            Manager.texture.Release();
            OutputTexture.Apply();

            Rect rect = new Rect(new Vector2(0, 0), new Vector2(Manager.texture.width, Manager.texture.height));
            Sprite sprite = Sprite.Create(OutputTexture, rect, new Vector2(1, 1) / 2);
            sprite.name = "generated sprite";
            renderer.sprite = sprite;
        }

    }


    // public List<TextureLayer> layers = new List<TextureLayer>();


    [System.Serializable]
    public class LayerManager
    {
        
        public Vector2Int Dimentions = new Vector2Int(256, 256);

        [SerializeReference] public List<TextureLayer> TextureLayers = new();
        public RenderTexture texture;
        public GameObject Parent;
        public void CreateNewLayer()
        {
            
            TextureLayers.Add(new SimpleGradientGpu());
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

            [Min(1)] public float QuantizeFactor = 256;

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
        public class PolygonGpu : TextureLayer , IDistorionSubMaker, IMaskSubMaker
        {
            public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }


       

            public SubMaker distortion = new("Distortion Maker");
            public SubMaker DistortionSM { get => distortion; set => distortion = value; }

            public MaskMaker secondary = new("Secondary Maker");
            public MaskMaker MaskSM { get => secondary; set => secondary = value; }

            public ShapeGenerator.Shapes shape = new();
            public Vector2 Offset = new Vector2(0.5f,0.5f);
            ComputeBuffer VertsBuffer;
            public Vector2[] Verts;
            public BlendModes BlendMode = BlendModes.RotLerp;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();

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
                computeshader.SetTexture(kernel, "DistortionTex", distortion.Generate());

                secondary.Dims = new(tex.width, tex.height);
                computeshader.SetTexture(kernel, "SecondaryInputTexture", secondary.Generate());


                computeshader.SetInt("OverrideWithMask", Logic.BoolToInt(secondary.OverrideWithMask));
                computeshader.SetInt("OverridePrimaryColor", Logic.BoolToInt(secondary.OverridePrimaryColor));




                computeshader.SetFloat("DistortionFactor", distortion.Influence);
            
                shape.GenerateCirclePoints();
                Verts = shape.Verts.ToVector2(Offset);




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
        public class StripesGpu : TextureLayer, IDistorionSubMaker, IMaskSubMaker
        {

            public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

         

            public MaskMaker secondary = new("Secondary Maker");
            public SubMaker DistortionSM { get => distortion; set => distortion = value; }

            public SubMaker distortion = new("Distortion Maker");

            public MaskMaker MaskSM { get => secondary; set => secondary = value; }

            public BlendModes BlendMode = BlendModes.RotLerp;

            public float Angle;
            public float Spacing = 128;
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
                computeshader.SetTexture(kernel, "DistortionTex", distortion.Generate());

                secondary.Parent = Parent;
                secondary.Dims = new(tex.width, tex.height);
                computeshader.SetTexture(kernel, "SecondaryInputTexture", secondary.Generate());
                computeshader.SetInt("OverrideWithMask", Logic.BoolToInt(secondary.OverrideWithMask));
                computeshader.SetInt("OverridePrimaryColor", Logic.BoolToInt(secondary.OverridePrimaryColor));






                base.PassValuesToShader(rt, kernel);

                computeshader.SetFloat("DistortionFactor", distortion.Influence);
                computeshader.SetFloat("Offset", Offset);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("Slope", Slope);
                computeshader.SetFloat("Spacing", Spacing);
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
            public Vector3 Pivot = new Vector3(0.5f, 0.5f, 0.5f);

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
            [Min(1f)] public float Scale = 1;
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
                // Seed = 1;
                BlendMode = (BlendModes)1;
                Type = TextureLayerType.noise;
                LastType = Type;



            }



            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {
                float floorscale = Floor(Scale);

                RenderTexture inputTexture;



                if (FeedInSeedNoise)
                {
                    inputTexture = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat);

                }
                else
                {
                   // Debug.Log((int)Floor(tex.width / floorscale));
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

                // im offseting by 5000 because if the input value is too small the result is too 
                //regular, horizontal banding and such, no need to make the randomizer more random 
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
            public GameObject Parent;
            public GameObject MakerObject;
            private TextureMaker Maker;
            [Range(0f,1f)]public float Opacity;
            

            public CompositeGpu(GameObject parent, string name)
            {
                Parent = parent;
                MakerObject = new(name, new System.Type[] { typeof(SpriteRenderer), typeof(TextureMaker)});
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
              
                Maker.Manager.Dimentions = new(tex.width,tex.height);
                Maker.Dimentions = Maker.Manager.Dimentions;
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
        public class RadialGradientGpu : TextureLayer, IDistorionSubMaker
        {

            public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

            public SubMaker distortion = new("Distortion Maker");

            public SubMaker DistortionSM { get => distortion; set => distortion = value; }

            public BlendModes BlendMode = BlendModes.RotLerp;
            public BlendModes GradientBlendMode = BlendModes.RotLerp;
            [Range(1f, 256f)] public float Blend;
            public Vector2 Center = Vector2.one/2f;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();
            public float Radius;

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
                distortion.Generate();

                base.PassValuesToShader(Mainrt, kernel);


                computeshader.SetTexture(kernel, "DistortionTex", distortion.SubTex);

                computeshader.SetFloat("DistortionFactor", distortion.Influence);


                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetFloat("Radius", Radius);
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
        public class SimpleGradientGpu : TextureLayer
        {
            public BlendModes BlendMode;
            public BlendModes GradientBlendMode;
            [Range(1f, 256f)] public float Blend;
            public float Angle;
            private Vector2 Slope => Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * Vector2.up;
            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();

            public SimpleGradientGpu()
            {
                Type = TextureLayerType.gradient;
                LastType = Type;

                Blend = 1;
              
            }

            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {
                base.PassValuesToShader(Mainrt, kernel);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetFloat("BlendPower", Sqrt(Blend));
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
        public class BumpyCircleGpu : TextureLayer ,IDistorionSubMaker
        {

            public GameObject Parent;
            public GameObject ParentSM { get => Parent; set => Parent = value; }

            public SubMaker distortion = new("Distortion Maker");
            public SubMaker DistortionSM { get => distortion; set => distortion = value; }

            public BlendModes BlendMode = BlendModes.RotLerp;

            static Octave Default = new(false, 0, 0, 10, 1, 6);
            public BumpyCircleGpu()
            {
            
                Type = TextureLayerType.bumpyCircle;
                LastType = Type;

            }

            public Color PrimaryColor = RandomColor();
            public Color SecondaryColor = RandomColor();
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

            void ApplyOctavesAndGenerate()
            {
                    distortion.Parent = Parent;
                distortion.Dims = new(tex.width, tex.height);
                distortion.Generate();

                if (computeshader == null)
                {
                    computeshader = Resources.Load<ComputeShader>("BumpyCircleShader");
                }

                int width = tex.width;
                int height = tex.height;

                RenderTexture rt = tex;
               // Debug.Log(tex.IsCreated());

                int kernel = computeshader.FindKernel("CSMain");
                computeshader.SetTexture(kernel, "Result", rt);

                int threadGroupsX = CeilToInt(width / 8f);
                int threadGroupsY = CeilToInt(height / 8f);

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
                    magnitude[i] = Octaves[i].magnitude;
                    power[i] = Octaves[i].power;
                    spacings[i] = Octaves[i].spacing;
                    //  Debug.Log(spacings[i]);

                }


            

                computeshader.SetFloat("DistortionFactor", distortion.Influence);
                computeshader.SetTexture(kernel, "DistortionTex", distortion.SubTex);

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

                // Set scalar values
                



                computeshader.SetInt("BlendMode", (int)BlendMode);

                computeshader.SetVector("Center", new Vector4(Center.x * width, Center.y * height));

                computeshader.SetVector("PrimaryColor", PrimaryColor);
                computeshader.SetVector("SecondaryColor", SecondaryColor);

                computeshader.SetInt("ArrayLength", Octaves.Count);
                computeshader.SetFloat("Radius", Radius);




                computeshader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

         

                offsetBuffer.Dispose();
                powerBuffer.Dispose();
                magnitudeBuffer.Dispose();

                absBuffer.Dispose();
                constantBuffer.Dispose();
                spacingBuffer.Dispose();


            }
            public List<Octave> Octaves = new();
            public Vector2 Center;
            public float Radius;

            public override RenderTexture Generate()
            {
                ApplyOctavesAndGenerate();

                return tex;
            }
        }


        public interface ISubmaker
        {
            GameObject ParentSM { get; set; }
        }

        public interface IDistorionSubMaker : ISubmaker
        {
            SubMaker DistortionSM { get; set; }
        }

        public interface IMaskSubMaker : ISubmaker
        {
            MaskMaker MaskSM { get; set; }
        }


        [System.Serializable]
        public class MaskMaker : SubMaker
        {

            public bool OverrideWithMask;
            public bool OverridePrimaryColor;

            public MaskMaker(string name)
            {
                Name = name;
            }



        }




        [System.Serializable]
        public class SubMaker
        {
            public string Name;

            public Vector2Int Dims = Vector2Int.one;
            [HideInInspector] public RenderTexture SubTex;
            public GameObject SubTextureMaker;
            public float Influence;

            [HideInInspector] public GameObject Parent;
            [HideInInspector] public TextureMaker Maker;

            public SubMaker()
            {
                
            }
            public SubMaker(string name)
            {
                Name = name;
            }

            public RenderTexture Generate()
            {

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

                    Maker.Dimentions = new Vector2Int(SubTex.width, SubTex.height);
                    Maker.GenerateAndApply();
                }
               
                if (Maker != null)
                {
                    Maker.Manager.GenerateTexture(true);
                    SubTex = Maker.Manager.texture;
                }

                

                if (SubTextureMaker == null)
                {
                    Influence = 0;
                }

          

                return SubTex;
            }
            public void CreateTextureMaker()
            {
                if (SubTextureMaker == null)
                {
                 //   Debug.Log("SubTextureMaker");

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
          

            public bool Enabled = true;
            
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

                PassValuesToShader(rt,kernel);

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


            [HideInInspector] public TextureLayerType LastType;
            public TextureLayerType Type;
            public ComputeShader computeshader;
            public RenderTexture tex;
            public RenderTexture OutTex => Generate();


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
                default:
                    return new SimpleGradientGpu();
            }
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
                texture = new RenderTexture(Dimentions.x, Dimentions.y, 0, RenderTextureFormat.ARGBFloat);
                texture.enableRandomWrite = true;
                texture.Create();
               
            }

            for (int i = 0; i < TextureLayers.Count; i++)
            {

                if (TextureLayers[i] == null)
                {
                    TextureLayers.RemoveAt(i);
                }


                if (TextureLayers[i] is ISubmaker sub)
                {

                    if (sub.ParentSM == null)
                    {
                        sub.ParentSM = Parent;
                    }

                    if (sub is IDistorionSubMaker subdis)
                    {
                        if (subdis.DistortionSM.Parent == null)
                        {
                            (subdis).DistortionSM.Parent = Parent;
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

                /*

                    else if (TextureLayers[i] is ISubMaker && (TextureLayers[i] as ISubMaker).SM.SubTextureMaker != null)
                    {
                        if ((TextureLayers[i] as ISubMaker).SM.SubTextureMaker.TryGetComponent(out TextureMaker _))
                        {
                            GameObject.DestroyImmediate((TextureLayers[i] as ISubMaker).SM.SubTextureMaker);
                        }
                    }
                  */





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
    }
}
