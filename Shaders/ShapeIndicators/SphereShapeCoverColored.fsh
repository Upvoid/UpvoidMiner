#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

uniform float uOutputOffset;
uniform vec4 uColor;

uniform vec4 uMidPointAndRadius;

uniform sampler2DRect uInDepth;

in vec3 vNormal;
in vec3 vEyePos;
in vec3 vWorldPos;

INPUT_CHANNEL_OutputColor(vec3)
INPUT_CHANNEL_Depth(float)
OUTPUT_CHANNEL_OutputColor(vec3)

void main()
{
    INIT_CHANNELS;

    vec4 transColor = uColor;
    vec3 normal = normalize(vNormal);

    float depth = texture(uInDepth, gl_FragCoord.xy + vec2(uOutputOffset)).r;
    vec4 screenPos = uProjectionMatrix * vec4(vEyePos, 1);
    screenPos /= screenPos.w;
    vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(screenPos.xy, depth * 2 - 1, 1.0);
    opaqueEyePos /= opaqueEyePos.w;

    float dist = distance(opaqueEyePos.xyz, vec3(uViewMatrix*vec4(uMidPointAndRadius.xyz,1)));


    float d2camOpaque = length(opaqueEyePos.xyz);
    float d2camShape = length(vEyePos.xyz);

    // indicate shape _on_ surface
    vec4 surfaceColor = uColor;
    {
       float e0 = max(0.0, uMidPointAndRadius.w-0.3);
       float e1 = max(0.5, uMidPointAndRadius.w-0.1);
       surfaceColor.a *= smoothstep(e0, e1, dist);
       surfaceColor.a += 0.3*uColor.a;
       surfaceColor.a *= smoothstep(1.0, 0.95, dist/uMidPointAndRadius.w);
    }

    // indicate shape _below_ surface
    vec4 shapeColor = uColor;
    {
       shapeColor.a = smoothstep(d2camOpaque, d2camOpaque + 0.05, d2camShape);
       shapeColor.a *= smoothstep(d2camShape - 5, d2camShape, d2camOpaque);

       // fresnel
       vec3 viewDir = normalize(uCameraPosition - vWorldPos);
       float dotVN = pow(abs(dot(viewDir, normal)), 1.0);
       shapeColor.a *= mix(1.0, 0.2, dotVN);

       shapeColor.a *= 0.6;

       // cos
       shapeColor.a *= smoothstep(0.8, 1.0, cos(uRuntime * 2 - dist));
    }

    //transColor = vec4(vec3(d2camOpaque / 10), 1);

    // mix colors
    transColor = shapeColor;
    transColor.rgb = mix(transColor.rgb, surfaceColor.rgb, surfaceColor.a);
    transColor.a = 1 - (1 - transColor.a) * (1 - surfaceColor.a);

    /*
    vec3 pos = opaqueWorldPos.xyz;
    const float scale = 30;
    transColor.a += max(0, sin(pos.x * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.y * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.z * scale) - .75) * 4;
    transColor.a = min(1, transColor.a);
    */


    OUTPUT_VEC4_OutputColor(transColor);
}
