using UnityEngine;

namespace _Game._Scripts.Support
{
    public static class MathExt
    {
        public static Vector3 MultiVector3(this Vector3 vt, Vector3 vt2)
        {
            Vector3 vtFinal;
            vtFinal.x = vt.x * vt2.x;
            vtFinal.y = vt.y * vt2.y;
            vtFinal.z = vt.z * vt2.z;

            return vtFinal;
        }
    }
}