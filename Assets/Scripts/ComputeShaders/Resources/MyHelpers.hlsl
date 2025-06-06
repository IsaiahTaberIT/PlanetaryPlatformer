static const float PI = 3.14159265;

static const float Sqrt2 = 1.41421356237;

static const float Sqrt3 = 1.7320508076;


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