#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

// material:
uniform sampler2D uColor;
uniform sampler2DRect uOpaqueDepth;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec4 vProj;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
   vec4 texColor = texture(uColor, vTexCoord);


   if(texColor.a < 0.01)
   discard;

   float fragDepth = gl_FragCoord.z;

   float depth = texture(uOpaqueDepth, gl_FragCoord.xy ).r;
   vec4 screenPos = vProj;
   screenPos /= screenPos.w;

   // TODO(ks) Optimize!
   vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(screenPos.xy, depth * 2 - 1, 1.0);
   opaqueEyePos /= opaqueEyePos.w;

   vec4 fragEyePos = uInverseProjectionMatrix * vec4(screenPos.xyz, 1.0);
   fragEyePos /= fragEyePos.w;


   float z_frag = fragEyePos.z;
   float z_depth = opaqueEyePos.z;

   // TODO(ks) Re-enable distance falloff
   float intensity = 0.35 * texColor.a * smoothstep(z_frag - 0.2, z_frag, z_depth) * (1 - smoothstep(z_frag, z_frag + 0.1, z_depth));

   OUTPUT_TransparentColor(vec4(0,0,0,intensity));
}
