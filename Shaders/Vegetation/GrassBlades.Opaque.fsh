#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

in vec3 vWorldPos;
in float vX;
in float vY;
in float vR;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
   INIT_CHANNELS;

   float dirX = (vX - 0.5) * 2.0;
   float adX = abs(dirX);

   // COLOR BEGIN =======================
   vec3 colorMid = vec3(14., 87., 22.) / 255.0;
   vec3 colorEdge = vec3(60., 192., 12.) / 255.0;

   float midF = min(1.0, pow(adX + smoothstep(0.7, 1.0, vY), 2));

   vec3 vColor = mix(colorMid, colorEdge, midF); // fading between edge and mid
   vColor *= vY; // dark to the bottom
   vColor.r *= (1.0 + vR * 0.9); // random red modultion

   // maybe make a texture out of http://www.athleat.co.uk/user/Grass_back.jpg

   // COLOR END =========================

   // Shadowing
   vec3 color = vColor * mix(0.07, 1, shadowFactor(vWorldPos));

   OUTPUT_Color(color);
   OUTPUT_Normal(normal);
   OUTPUT_Position(vWorldPos);
}
