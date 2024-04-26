﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

namespace Game.Utility
{
    class GameMath
    {
        public enum Axis {Forward, Backward,  Left, Right, Up, Down}

        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
        public static bool IsNumerical(string input)
        {
            foreach (char c in input)
            {
                if (c < '0' || c > '9')
                {
                    Debugging.Log("Char not numberical: " + ((byte)c));
                    return false; // Character is not a digit
                }
            }
            return true; // All characters are digits

        }

        public static float GetAxialSpeed(Axis axis, Rigidbody rb, Transform trans)
        {
            switch (axis)
            {
                case Axis.Forward:
                    Vector3 forwardVec = Vector3.Project(rb.velocity, trans.forward);
                    return Vector3.Dot(forwardVec, trans.forward.normalized);

                case Axis.Backward:
                    Vector3 backwardVec = Vector3.Project(rb.velocity, -trans.forward);
                    return Vector3.Dot(backwardVec, -trans.forward.normalized);

                case Axis.Left:
                    Vector3 leftVec = Vector3.Project(rb.velocity, -trans.right);
                    return Vector3.Dot(leftVec, -trans.right.normalized);

                case Axis.Right:
                    Vector3 rightVec = Vector3.Project(rb.velocity, trans.right);
                    return Vector3.Dot(rightVec, trans.right.normalized);

                case Axis.Up:
                    Vector3 upVec = Vector3.Project(rb.velocity, trans.up);
                    return Vector3.Dot(upVec, trans.up.normalized);

                case Axis.Down:
                    Vector3 downVec = Vector3.Project(rb.velocity, -trans.up);
                    return Vector3.Dot(downVec, -trans.up.normalized);

                default:
                    return 0f;
            }
        }



    }
}
