#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

in vec3 vWorldPos;
in float vX;
in float vY;
in float vR;
uniform float uFadeDistance = 25.0;

uniform sampler2D uColor;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

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

   OUTPUT_Color(color);
   OUTPUT_Normal(normal);
   OUTPUT_Position(vWorldPos);
}
