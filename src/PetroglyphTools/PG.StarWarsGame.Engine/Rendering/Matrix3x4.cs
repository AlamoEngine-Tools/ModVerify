using System;
using System.Globalization;
using System.Numerics;

namespace PG.StarWarsGame.Engine.Rendering;

public struct Matrix3x4 : IEquatable<Matrix3x4>
{
    public float M11;
    public float M12;
    public float M13;
    public float M14;
    public float M21;
    public float M22;
    public float M23;
    public float M24;
    public float M31;
    public float M32;
    public float M33;
    public float M34;

    public static Matrix3x4 Identity { get; } = new(
        1f, 0.0f, 0.0f, 0.0f,
        0.0f, 1f, 0.0f, 0.0f,
        0.0f, 0.0f, 1f, 0.0f);

    /// <summary>Constructs a Matrix3x4 from the given components.</summary>
    public Matrix3x4(
        float m11,
        float m12,
        float m13,
        float m14,
        float m21,
        float m22,
        float m23,
        float m24,
        float m31,
        float m32,
        float m33,
        float m34)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;
        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
    }

    public static Matrix3x4 Scale(Vector3 scale)
    {
        return Scale(scale.X, scale.Y, scale.Z);
    }

    public static Matrix3x4 Scale(float xScale, float yScale, float zScale)
    {
        return new Matrix3x4
        {
            M11 = xScale, M12 = 0, M13 = 0, M14 = 0,
            M21 = 0, M22 = yScale, M23 = 0, M24 = 0,
            M31 = 0, M32 = 0, M33 = zScale, M34 = 0
        };
    }

    public static Matrix3x4 CreateTranslation(Vector3 position)
    {
        return CreateTranslation(position.X, position.Y, position.Z);
    }

    public static Matrix3x4 CreateTranslation(float xPosition, float yPosition, float zPosition)
    {
        return new Matrix3x4
        {
            M11 = 1, M12 = 0, M13 = 0, M14 = xPosition,
            M21 = 0, M22 = 1, M23 = 0, M24 = yPosition,
            M31 = 0, M32 = 0, M33 = 1, M34 = zPosition
        };
    }

    public static Matrix3x4 operator *(Matrix3x4 value1, Matrix3x4 value2)
    {
        Matrix3x4 matrix3x4;
        matrix3x4.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31;
        matrix3x4.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32;
        matrix3x4.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33;
        matrix3x4.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14;
        matrix3x4.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31;
        matrix3x4.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32;
        matrix3x4.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33;
        matrix3x4.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24;
        matrix3x4.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31;
        matrix3x4.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32;
        matrix3x4.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33;
        matrix3x4.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34;
        return matrix3x4;
    }

    public static Matrix3x4 operator *(Matrix3x4 value1, float value2)
    {
        Matrix3x4 matrix3x4;
        matrix3x4.M11 = value1.M11 * value2;
        matrix3x4.M12 = value1.M12 * value2;
        matrix3x4.M13 = value1.M13 * value2;
        matrix3x4.M14 = value1.M14 * value2;
        matrix3x4.M21 = value1.M21 * value2;
        matrix3x4.M22 = value1.M22 * value2;
        matrix3x4.M23 = value1.M23 * value2;
        matrix3x4.M24 = value1.M24 * value2;
        matrix3x4.M31 = value1.M31 * value2;
        matrix3x4.M32 = value1.M32 * value2;
        matrix3x4.M33 = value1.M33 * value2;
        matrix3x4.M34 = value1.M34 * value2;
        return matrix3x4;
    }

    /// <summary>
    /// Returns a boolean indicating whether the given two matrices are equal.
    /// </summary>
    /// <param name="value1">The first matrix to compare.</param>
    /// <param name="value2">The second matrix to compare.</param>
    /// <returns>True if the given matrices are equal; False otherwise.</returns>
    public static bool operator ==(Matrix3x4 value1, Matrix3x4 value2)
    {
        return value1.M11 == (double)value2.M11 && value1.M22 == (double)value2.M22 && value1.M33 == (double)value2.M33 &&
               value1.M12 == (double)value2.M12 && value1.M13 == (double)value2.M13 && value1.M14 == (double)value2.M14 && 
               value1.M21 == (double)value2.M21 && value1.M23 == (double)value2.M23 && value1.M24 == (double)value2.M24 &&
               value1.M31 == (double)value2.M31 && value1.M32 == (double)value2.M32 && value1.M34 == (double)value2.M34;
    }

    /// <summary>
    /// Returns a boolean indicating whether the given two matrices are not equal.
    /// </summary>
    /// <param name="value1">The first matrix to compare.</param>
    /// <param name="value2">The second matrix to compare.</param>
    /// <returns>True if the given matrices are not equal; False if they are equal.</returns>
    public static bool operator !=(Matrix3x4 value1, Matrix3x4 value2)
    {
        return value1.M11 != (double)value2.M11 || value1.M12 != (double)value2.M12 || value1.M13 != (double)value2.M13 || value1.M14 != (double)value2.M14 ||
               value1.M21 != (double)value2.M21 || value1.M22 != (double)value2.M22 || value1.M23 != (double)value2.M23 || value1.M24 != (double)value2.M24 ||
               value1.M31 != (double)value2.M31 || value1.M32 != (double)value2.M32 || value1.M33 != (double)value2.M33 || value1.M34 != (double)value2.M34;
    }

    /// <summary>
    /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
    /// </summary>
    /// <param name="other">The matrix to compare this instance to.</param>
    /// <returns>True if the matrices are equal; False otherwise.</returns>
    public bool Equals(Matrix3x4 other)
    {
        return M11 == (double)other.M11 && M22 == (double)other.M22 && M33 == (double)other.M33 &&
               M12 == (double)other.M12 && M13 == (double)other.M13 && M14 == (double)other.M14 &&
               M21 == (double)other.M21 && M23 == (double)other.M23 && M24 == (double)other.M24 &&
               M31 == (double)other.M31 && M32 == (double)other.M32 && M34 == (double)other.M34;
    }

    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this matrix instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
    public override bool Equals(object? obj) => obj is Matrix3x4 other && Equals(other);

    /// <summary>Returns a String representing this matrix instance.</summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        var currentCulture = CultureInfo.CurrentCulture;
        return string.Format(currentCulture,
            "{{ {{M11:{0} M12:{1} M13:{2} M14:{3}}} {{M21:{4} M22:{5} M23:{6} M24:{7}}} {{M31:{8} M32:{9} M33:{10} M34:{11}}}",
            M11.ToString(currentCulture),
            M12.ToString(currentCulture),
            M13.ToString(currentCulture),
            M14.ToString(currentCulture),
            M21.ToString(currentCulture),
            M22.ToString(currentCulture),
            M23.ToString(currentCulture),
            M24.ToString(currentCulture),
            M31.ToString(currentCulture),
            M32.ToString(currentCulture),
            M33.ToString(currentCulture),
            M34.ToString(currentCulture));
    }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
               M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
               M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode();
    }
}