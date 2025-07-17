static const float PI = 3.14159265;

static const float Sqrt2 = 1.41421356237;

static const float Sqrt3 = 1.7320508076;

static const uint uintmax = 4294967295;

float InverseLerp(float a, float b, float t)
{
    return (t - a) / (b - a);
    // (value - start) / (end - start)
}
   


void HandleMasking(bool OWM, bool OPC, inout float4 c1, inout float4 c2, float4 SITColor)
{
    if (OWM)
    {
        if (OPC)
        {
            c1 = SITColor;
            
        }
        else
        {
            c2 = SITColor;
        }
        
    }
    
}





uint2 DistortPos(float4 DT, uint2 pos, float factor)
{
   // DT = saturate(DT);    
    DT -= 0.5;
    DT *= 2.0;
    
    DT *= factor;
    DT *= DT.w;

    if (length(DT) <= 0.01)
    {
        DT = float4(1, 1, 1, 1) * 0.01;
    }
    
    return (DT.xy + pos);

}


float absPow(float x, float y)
{
    return pow(abs(x), y);
}



float EaseIn(float t, float power)
{
    return absPow(t, power);
}

float EaseOut(float t, float power)
{
    return absPow(t, 1 / power);
}


float EaseInOut(float t, float power)
{
    return lerp(EaseIn(t, power), EaseOut(t, power), t);
}

  
float FloatBasedRandomValue(float xdim, uint2 id, float localSeed)
{
    
    float value = id.y * xdim + id.x;
    value *= localSeed;
    value += localSeed;
    value /= 100.1;
    value = (sin(value * 153.7342 + 57.2) + 1) * 99.76;
    value += 57.31;
    value *= 19.37;
    float Floor1 = floor(value);
    float Decimal1 = value % 1.0;
    value = (cos(value * 314.1592) + 1) * 31.7;
    value += 69.31;
    value *= 15.92;
    value = (sin(value * 1589.2415) + 1) * 103.7;
    value += 1.231;
    float Floor2 = floor(value);
    float Decimal2 = value % 1.0;
    value = abs(dot(float2(Floor1, Decimal2 * 13211.232), float2(Decimal1 * 12312.931, Floor2 + 1.2134)));
    value /= 123.2145;
    value = abs(value);
    value = (sin(value * 200) + 1.0) * 115.1;

    value %= 1.0;
    return value;
    
     
    
}




float RandomValue(uint2 dims, uint2 id, uint localSeed)
{
    uint Uintvalue = id.y * dims.x + id.x;
    Uintvalue *= localSeed;
    Uintvalue = Uintvalue ^ 1123123123;
    Uintvalue *= 3;
    Uintvalue = Uintvalue ^ 2313123631;
    Uintvalue *= 7;
    Uintvalue = Uintvalue ^ 2098984732;
    Uintvalue *= 11;
    Uintvalue = Uintvalue ^ 3969759556;
    Uintvalue *= 17;
    Uintvalue = Uintvalue ^ 1653786262;
    Uintvalue *= 31;
    Uintvalue = Uintvalue ^ 3109876532;
    Uintvalue *= id.y + dims.y * id.x;
    
  
    
    float OutValue = (float)Uintvalue / uintmax;

    
    return OutValue;
    
     
    
}



float3 UndoGamma(float3 val)
{
    
    val = pow(abs(val), 1.0 / 2.2);
    return val;
}
float4 UndoGamma(float4 val)
{
    
    val.rgb = pow(abs(val.rgb), 1.0 / 2.2);
    return val;
}

float3 Gamma(float3 val)
{
    
    val = pow(abs(val), 2.2);
    return val;
}
float4 Gamma(float4 val)
{
    
    val.rgb = pow(abs(val.rgb), 2.2);
    return val;
}

float4 RotLerp(float4 v1, float4 v2, float t)
{
    float alpha = lerp(v1.w, v2.w, t);
    float rgbMag = lerp(length(v1.rgb), length(v2.rgb), t);
    float3 dir = normalize(lerp(v1.rgb, v2.rgb, t));
    
    float4 output;
    output.rgb = dir * rgbMag;
    output.w = alpha;
    return output;
    
    
    
    
    
}

float SignedAngle2D(float2 from, float2 to)
{
    float angle = atan2(to.y, to.x) - atan2(from.y, from.x);
    
    // Wrap the angle to [-PI, PI]
    
    if (angle > PI)
    {
        angle -= 2.0 * PI;

    }
    else if (angle < -PI)
    {
        angle += 2.0 * PI;
    }
        
      

    return angle; // in radians
}


float powInt(float x, int y)
{
    float output = x;
    for (int i = 0; i < y - 1; i++)
    {
        output *= x;
    }
    
    return output;

}





float4 Solid(uint3 pos, float4 Color)
{
  
    return Color;
    
}



float4 Circle(uint3 pos, float4 startColor, float4 endColor)
{
    float radius = 50;
    float t = distance(pos.xy, uint2(128, 128));
    
   
    // Linear interpolate betwe
    
    if (t < radius)
    {
        return startColor;
    }
    else
    {
        return endColor;
    }
    
   
    
}

float4 RadialGradient(uint3 pos, float4 startColor, float4 endColor)
{
    float radius = 50;
    float t = distance(pos.xy, uint2(140, 140));
    
    if (t < radius)
    {
        t /= radius;
    }
    else
    {
        t = 1;
    }
    
    // Start color (black)
    
   
    
    // Linear interpolate between startColor and endColor by t
    float4 color = lerp(startColor, endColor, 1 - t);
    
    return color;
    
}

float lerpMinMax(float a, float b,float t,bool invert)
{
    if (invert)
    {
        t = 1.0 - t;

    }
    
    float output = lerp(max(a, b), min(a, b), t);
    
    
    return output;
}


float4 Blend(float4 c1, float4 c2, float t, int type)
{
    float4 output = float4(1.0, 1.0, 1.0, 1.0);
    float alpha;
    switch (type)
    {

 
        case 0:
        // lerp
            output = lerp(c1, c2, t);
            break;
        case 1:
        // rotlerp
            output = RotLerp(c1, c2, t);
            break;
        case  2:
        // multiply
            alpha = c1.w;
        
        
            output = c1 * (c2 * (t) + (1 - t));
            output.w = alpha;
            break;
        case 3:
        // darken only 
            output = float4(lerpMinMax(c1.x, c2.x, t, false), lerpMinMax(c1.y, c2.y, t, false), lerpMinMax(c1.z, c2.z, t, false), lerpMinMax(c1.w, c2.w, t, false));
            break;
        case 4:
        // lighten only
            output = float4(lerpMinMax(c1.x, c2.x, t, true), lerpMinMax(c1.y, c2.y, t, true), lerpMinMax(c1.z, c2.z, t, true), lerpMinMax(c1.w, c2.w, t, true));
            break;
        
        case  5:
        // addition
            output = c1 + c2 * t;
            break;
        case 6:
        // subtract
           
       // float3 rgbcolor =

        
            output = float4(c1.rgb - c2.rgb * t, c1.w);
            break;
        case 7:
            output = c2;
       
            break;
        case  8:
            break;
        case  9:
            break;
        default:
            break;
    }
        
    return output;
}