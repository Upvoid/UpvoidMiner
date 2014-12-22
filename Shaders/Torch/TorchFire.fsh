#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Particles.fsh>

uniform sampler2DArray uColor;

uniform sampler2DRect uOpaqueDepth;

in vec2 vTexCoord;
in vec3 vWorldPos;
in vec3 vEyePos;
in vec4 vScreenPos;
in vec3 iWorldPos;
in float quadID;


INPUT_CHANNEL_TransparentColor(vec4)
INPUT_CHANNEL_Distortion(vec2)
INPUT_CHANNEL_Color(vec3)
INPUT_CHANNEL_Depth(float)
OUTPUT_CHANNEL_Color(vec3)

void main()
{
    INIT_CHANNELS;

    const int numberOfSlices = 5;

    float texUnit = quadID + .7*uRuntime;

    // We got 5 fire textures (0..4)
    float fractPart = fract(texUnit);
    int intPart  = int(texUnit) % numberOfSlices;
    int intPart2 = (intPart+1) % numberOfSlices;

    // illumination
    vec4 texColor1 = texture(uColor, vec3(vTexCoord, float(intPart)));
    vec4 texColor2 = texture(uColor, vec3(vTexCoord, float(intPart2)));

    // interpolation
    vec4 baseColor = mix(texColor1, texColor2, fractPart);

    // get current depth
    float depth = texture(uOpaqueDepth, gl_FragCoord.xy).r;
    // get (underlying) opaque fragment position in eye-space
    vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(vScreenPos.xy, depth * 2 - 1, 1.0);
    opaqueEyePos /= opaqueEyePos.w;

    float dist = vEyePos.z - opaqueEyePos.z;

    baseColor.a *= smoothstep(0, .1, dist);

    vec3 color = baseColor.rgb * baseColor.a;
    //transColor.rgba = vec4(vLife, 0.0, 0.0, 1.0);
    OUTPUT_Color(color);
}
