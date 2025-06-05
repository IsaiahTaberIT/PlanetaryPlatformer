using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static TextureMaker.LayerManager;
using static UnityEngine.Mathf;
using static TextureMaker.LayerManager.TextureLayer;
using System.Linq.Expressions;




[System.Serializable]

public class TextureMaker : MonoBehaviour
{
    public Vector2Int Dimentions = new Vector2Int(256,256);
    public Texture2D OutputTexture;


    public LayerManager Manager = new();

   
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

        public class TransparencyFilterGpu : Filter
        {
            public BlendModes BlendMode;

            [Range(0, 1f)] public float Alpha = 0;

            public TransparencyFilterGpu()
            {
                FilterType = FilterTypes.transparency;
                LastFilterType = FilterType;
            }

            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);

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

        public class SaturationFilterGpu : Filter
        {
            public BlendModes BlendMode;

            [Range(0,1f)]public float Saturation = 0;

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
            public BlendModes BlendMode;

            public Color Color;

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
        public class StripesGpu : DistortableLayer
        {


            public BlendModes BlendMode;

            public float Angle;
            public float Spacing;
            [Min(0f)] public float Offset;
            private Vector2 Slope => Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * Vector2.up;
            public Color Color1;
            public Color Color2;

            public StripesGpu(GameObject parent)
            {
                Parent = parent;
                Type = TextureLayerType.stripes;
                LastType = Type;
            }


            public override void PassValuesToShader(RenderTexture rt, int kernel)
            {
                base.PassValuesToShader(rt, kernel);

                if (DistortionTex == null)
                {
                    DistortionTex  = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat);
                    DistortionTex.enableRandomWrite = true;
                    DistortionTex.Create();
                }
              

                computeshader.SetTexture(kernel, "DistortionTex", DistortionTex);
                computeshader.SetFloat("DistortionFactor", DistortionFactor);
                computeshader.SetFloat("Offset", Offset);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetVector("Slope", Slope);
                computeshader.SetFloat("Spacing", Spacing);
                computeshader.SetVector("Color1", Color1);
                computeshader.SetVector("Color2", Color2);
            }

            public override RenderTexture Generate()
            {
                if (Maker != null)
                {
                    DistortionTex = base.Generate();
                    Maker.Dimentions = new Vector2Int(tex.width, tex.height);
                    Maker.GenerateAndApply();
                }

               

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
        public class CompositeGpu : TextureLayer
        {
            public bool PassInBackgound = false;
            public BlendModes BlendMode;
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
        public class RadialGradientGpu : TextureLayer
        {
            public BlendModes BlendMode;
            public BlendModes GradientBlendMode;
            public float Distortion;
            [Range(1f, 256f)] public float Blend;
            public float Angle;
            public Vector2 Center;
            public Vector2 Squish = new Vector2(1,1);
           // private Vector2 Slope => Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * Vector2.up;
            public Color Color1;
            public Color Color2;
            public float Radius;

            public RadialGradientGpu()
            {
                Type = TextureLayerType.radialGradient;
                LastType = Type;

                Blend = 1;

            }

            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {
                base.PassValuesToShader(Mainrt, kernel);
                computeshader.SetFloat("Distortion", Distortion * 10f);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetFloat("Radius", Radius);


                computeshader.SetFloat("BlendPower", Sqrt(Blend));
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetVector("Squish", Squish);

                computeshader.SetVector("Center", Center);
                computeshader.SetVector("Color1", Color1);
                computeshader.SetVector("Color2", Color2);
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
            public float Distortion;
            [Range(1f, 256f)] public float Blend;
            public float Angle;
            private Vector2 Slope => Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * Vector2.up;
            public Color Color1;
            public Color Color2;

            public SimpleGradientGpu()
            {
                Type = TextureLayerType.gradient;
                LastType = Type;

                Blend = 1;
              
            }

            public override void PassValuesToShader(RenderTexture Mainrt, int kernel)
            {
                base.PassValuesToShader(Mainrt, kernel);
                computeshader.SetFloat("Distortion", Distortion * 10f);
                computeshader.SetVector("Dims", new Vector2(tex.width, tex.height));
                computeshader.SetInt("GradientBlendMode", (int)GradientBlendMode);
                computeshader.SetInt("BlendMode", (int)BlendMode);
                computeshader.SetFloat("BlendPower", Sqrt(Blend));
                computeshader.SetVector("Slope", Slope);
                computeshader.SetVector("Color1", Color1);
                computeshader.SetVector("Color2", Color2);
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
        public class BumpyCircleGpu : TextureLayer
        {
            public BlendModes BlendMode;

            static Octave Default = new(false, 0, 0, 10, 1, 6);
            public BumpyCircleGpu()
            {
            
                Type = TextureLayerType.bumpyCircle;
                LastType = Type;

            }

            public Color Color1 = Color.blue / 2f;
            public Color Color2 = Color.red / 2f;
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

                computeshader.SetVector("Color1", Color1);
                computeshader.SetVector("Color2", Color2);

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

        [System.Serializable]
        public class DistortableLayer : TextureLayer
        {
            [HideInInspector] public RenderTexture DistortionTex;
            public GameObject DistortionTextureMaker;
            public float DistortionFactor;
            [HideInInspector] public GameObject Parent;
            [HideInInspector] public TextureMaker Maker;

            public DistortableLayer()
            {
                
            }

            public override RenderTexture Generate()
            {
                Maker.Manager.GenerateTexture(true);

                if (DistortionTextureMaker == null)
                {
                    DistortionFactor = 0;
                }

                return Maker.Manager.texture;
            }
            public void CreateTextureMaker(string name)
            {
                if (DistortionTextureMaker == null)
                {
                    if (Parent == null)
                    {
                        return;
                    }

                    DistortionTextureMaker = new(name, new System.Type[] { typeof(SpriteRenderer), typeof(TextureMaker) });
                    DistortionTextureMaker.transform.SetParent(Parent.transform);
                    Maker = DistortionTextureMaker.GetComponent<TextureMaker>();
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
                case Filter.FilterTypes.transparency:
                    return new TransparencyFilterGpu();
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

                if (TextureLayers[i] is DistortableLayer)
                {
                    if ((TextureLayers[i] as DistortableLayer).Parent == null)
                    {
                        (TextureLayers[i] as DistortableLayer).Parent = Parent;
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
                    else if (TextureLayers[i] is DistortableLayer && (TextureLayers[i] as DistortableLayer).DistortionTextureMaker != null)
                    {
                        if ((TextureLayers[i] as DistortableLayer).DistortionTextureMaker.TryGetComponent(out TextureMaker _))
                        {
                            GameObject.DestroyImmediate((TextureLayers[i] as DistortableLayer).DistortionTextureMaker);
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
    }
}
