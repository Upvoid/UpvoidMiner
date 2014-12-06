#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

uniform float uFadeDistance = 25.0;

uniform float uRoughness = 0.5;
uniform float uFresnel = 1.3;
uniform float uGlossiness = 0.5;

in vec3 vWorldPos;
in float vX;
in float vY;
in float vR;

uniform sampler2D uColor;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

void main()
{
   INIT_CHANNELS;

   float posFactor = 1 - smoothstep(uFadeDistance * .8, uFadeDistance, distance(vWorldPos, uCameraPosition));
   if(int(13.479*gl_FragCoord.x + gl_FragCoord.y * 273.524 * gl_FragCoord.x) % 200 >= posFactor * 250)
      discard;

   /*
   float dirX = (vX - 0.5) * 2.0;
   float adX = abs(dirX);

   // COLOR BEGIN =======================
   vec3 colorMid = vec3(14., 87., 22.) / 255.0;
   vec3 colorEdge = vec3(60., 192., 12.) / 255.0;

   float midF = min(1.0, pow(adX + smoothstep(0.7, 1.0, vY), 2));

   vec3 vColor = mix(colorMid, colorEdge, midF); // fading between edge and mid
   vColor *= vY; // dark to the bottom
   vColor.r *= (1.0 + vR * 0.9); // random red modultion
   */


   vec3 vColor = texture(uColor, vec2(vX, vY)).rgb * vY;

   // COLOR END =========================

   // Shadowing
   vec3 color = vColor;

   vec4 gb1, gb2;
   writeGBuffer(
      color,
      vec3(0, 1, 0),
      uRoughness,
      uFresnel,
      uGlossiness,
      gb1, gb2
   );
   OUTPUT_GBuffer1(gb1);
   OUTPUT_GBuffer2(gb2);
}
