#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Normalmapping.fsh>

uniform sampler2D uColorXY;
uniform sampler2D uColorXZ;
uniform sampler2D uColorZY;
uniform sampler2D uNormalXY;
uniform sampler2D uNormalXZ;
uniform sampler2D uNormalZY;

uniform float uRoughness = 0.8;
uniform float uFresnel = 8.3;
uniform float uGlossiness = 1.0;

uniform float uTexScale;
uniform float uBlendingCoefficient;

uniform float uBlackness = 1.0;

uniform mat4 uModelMatrix;

in vec3 vWorldPos;
in vec3 vObjectPos;
in vec3 vObjectNormal;
in mat3 vNormalMatrix;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

void main()
{
    INIT_CHANNELS;

    // texturing
    vec3 xyColor = texture(uColorXY, vObjectPos.xy / uTexScale).rgb;
    vec3 xzColor = texture(uColorXZ, vObjectPos.xz / uTexScale).rgb;
    vec3 zyColor = texture(uColorZY, vObjectPos.zy / uTexScale).rgb;

    // for AMD, pow(vec3(0, y, z), w) always returns zero for non-const vec3.
    // this is a hotfix.
    vec3 powFriendlyObjectNormal = abs(vObjectNormal) + vec3(0.001);

    vec3 weights = pow(powFriendlyObjectNormal, vec3(uBlendingCoefficient));
    weights /= weights.x + weights.y + weights.z;
    vec3 baseColor = xyColor * weights.z + xzColor * weights.y + zyColor * weights.x;

    // normal mapping
    vec3 xyNormalMap = unpack8bitNormalmap(texture(uNormalXY, vObjectPos.xy / uTexScale).rgb);
    vec3 xzNormalMap = unpack8bitNormalmap(texture(uNormalXZ, vObjectPos.xz / uTexScale).rgb);
    vec3 zyNormalMap = unpack8bitNormalmap(texture(uNormalZY, vObjectPos.zy / uTexScale).rgb);

    vec3 xyNormal = xyNormalMap.xyz * sign(vObjectNormal.z);
    vec3 xzNormal = xzNormalMap.xzy * sign(vObjectNormal.y);
    vec3 zyNormal = zyNormalMap.zyx * sign(vObjectNormal.x);

    vec3 normalWeights = abs(vObjectNormal);
    normalWeights /= normalWeights.x + normalWeights.y + normalWeights.z;
    vec3 objNormal = xyNormal * normalWeights.z + xzNormal * normalWeights.y + zyNormal * normalWeights.x;
    vec3 normal = normalize(mat3(uModelMatrix) * objNormal);

    // illumination

    // Skybox reflection
    /*vec3 reflDir = reflect(normalize(vWorldPos - uCameraPosition), normal);
    vec3 skyColor = sampleSkybox(reflDir).rgb;
    float reflFactor = .6;
    color = mix(color, skyColor, reflFactor);*/


    vec4 gb1, gb2;
    writeGBuffer(
       baseColor.rgb,
       normal,
       uRoughness,
       uFresnel,
       uGlossiness,
       gb1, gb2
    );
    OUTPUT_GBuffer1(gb1);
    OUTPUT_GBuffer2(gb2);
}
